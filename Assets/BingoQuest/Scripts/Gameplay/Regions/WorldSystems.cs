using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Objectives;

namespace BingoQuest.Gameplay.Regions
{
    public sealed class RegionDefinition
    {
        public string RegionId { get; set; }
        public string DisplayName { get; set; }
        public int BaseCardDifficulty { get; set; } = 1;
        public int RecommendedLevel { get; set; } = 1;
        public float EnemyHealthMultiplier { get; set; } = 1f;
        public float EnemyAttackMultiplier { get; set; } = 1f;
        public float SpawnIntervalSeconds { get; set; } = 2.5f;
        public int MaxAliveEnemies { get; set; } = 8;
        public float SpawnRadius { get; set; } = 14f;
        public List<string> ConnectedRegionIds { get; set; } = new();
    }

    public sealed class RegionRuntimeState
    {
        public string RegionId { get; set; }
        public bool IsUnlocked { get; set; }
        public bool BossDefeated { get; set; }
        public int VisitCount { get; set; }
    }

    public readonly struct WorldTransition
    {
        public string PreviousRegionId { get; }
        public string CurrentRegionId { get; }
        public bool IsFirstVisit { get; }

        public WorldTransition(string previousRegionId, string currentRegionId, bool isFirstVisit)
        {
            PreviousRegionId = previousRegionId;
            CurrentRegionId = currentRegionId;
            IsFirstVisit = isFirstVisit;
        }
    }

    public sealed class WorldMapService
    {
        private readonly Dictionary<string, RegionDefinition> _regions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, RegionRuntimeState> _runtime = new(StringComparer.Ordinal);
        private string _currentRegionId;

        public string CurrentRegionId => _currentRegionId;
        public RegionDefinition CurrentRegion => _currentRegionId != null && _regions.TryGetValue(_currentRegionId, out var region)
            ? region
            : null;

        public event Action<WorldTransition, RegionDefinition> OnRegionEntered;

        public IReadOnlyCollection<RegionDefinition> Regions => _regions.Values;

        public void AddRegion(RegionDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrWhiteSpace(definition.RegionId))
                throw new ArgumentException("RegionId is required", nameof(definition));
            if (_regions.ContainsKey(definition.RegionId))
                throw new InvalidOperationException($"Region '{definition.RegionId}' is already defined.");

            _regions[definition.RegionId] = definition;
            _runtime[definition.RegionId] = new RegionRuntimeState
            {
                RegionId = definition.RegionId,
                IsUnlocked = false,
                BossDefeated = false,
                VisitCount = 0
            };
        }

        public void ConnectBidirectional(string a, string b)
        {
            var regionA = GetRegion(a);
            var regionB = GetRegion(b);

            if (!regionA.ConnectedRegionIds.Contains(b))
                regionA.ConnectedRegionIds.Add(b);
            if (!regionB.ConnectedRegionIds.Contains(a))
                regionB.ConnectedRegionIds.Add(a);
        }

        public void UnlockRegion(string regionId)
        {
            var state = GetRuntimeState(regionId);
            state.IsUnlocked = true;
            _runtime[regionId] = state;
        }

        public bool IsRegionUnlocked(string regionId) => GetRuntimeState(regionId).IsUnlocked;

        public bool TryEnterRegion(string regionId, out RegionDefinition enteredRegion)
        {
            enteredRegion = null;
            if (!_regions.TryGetValue(regionId, out var region))
                return false;

            var state = _runtime[regionId];
            if (!state.IsUnlocked)
                return false;

            if (!CanTravelTo(regionId))
                return false;

            bool firstVisit = state.VisitCount == 0;
            state.VisitCount++;
            _runtime[regionId] = state;

            string previous = _currentRegionId;
            _currentRegionId = regionId;
            enteredRegion = region;
            OnRegionEntered?.Invoke(new WorldTransition(previous, regionId, firstVisit), region);
            return true;
        }

        public void CompleteCurrentRegionBoss()
        {
            if (string.IsNullOrEmpty(_currentRegionId))
                return;

            var currentState = _runtime[_currentRegionId];
            currentState.BossDefeated = true;
            _runtime[_currentRegionId] = currentState;

            var current = _regions[_currentRegionId];
            for (int i = 0; i < current.ConnectedRegionIds.Count; i++)
            {
                string nextId = current.ConnectedRegionIds[i];
                var nextState = _runtime[nextId];
                nextState.IsUnlocked = true;
                _runtime[nextId] = nextState;
            }
        }

        public List<RegionDefinition> GetAvailableDestinations()
        {
            var results = new List<RegionDefinition>();

            if (string.IsNullOrEmpty(_currentRegionId))
            {
                foreach (var entry in _regions)
                {
                    if (_runtime[entry.Key].IsUnlocked)
                        results.Add(entry.Value);
                }
                return results;
            }

            var current = _regions[_currentRegionId];
            for (int i = 0; i < current.ConnectedRegionIds.Count; i++)
            {
                string id = current.ConnectedRegionIds[i];
                if (_runtime[id].IsUnlocked && _regions.TryGetValue(id, out var region))
                    results.Add(region);
            }

            return results;
        }

        public ObjectiveContext BuildObjectiveContext(string playerClassId, int runSeedBase, int cardDifficultyBonus = 0)
        {
            if (string.IsNullOrEmpty(_currentRegionId))
                throw new InvalidOperationException("Cannot build ObjectiveContext before entering a region.");

            var region = _regions[_currentRegionId];
            var state = _runtime[_currentRegionId];
            int seed = runSeedBase + (StableHash(region.RegionId) * 17) + (state.VisitCount * 101);

            return new ObjectiveContext
            {
                ZoneId = region.RegionId,
                CardDifficulty = Math.Max(1, region.BaseCardDifficulty + cardDifficultyBonus),
                PlayerClassId = playerClassId,
                RunSeed = seed
            };
        }

        private bool CanTravelTo(string targetRegionId)
        {
            if (string.IsNullOrEmpty(_currentRegionId))
                return true;
            if (_currentRegionId == targetRegionId)
                return true;

            var current = _regions[_currentRegionId];
            return current.ConnectedRegionIds.Contains(targetRegionId);
        }

        private RegionDefinition GetRegion(string regionId)
        {
            if (!_regions.TryGetValue(regionId, out var region))
                throw new InvalidOperationException($"Region '{regionId}' is not defined.");
            return region;
        }

        private RegionRuntimeState GetRuntimeState(string regionId)
        {
            if (!_runtime.TryGetValue(regionId, out var state))
                throw new InvalidOperationException($"Region runtime state for '{regionId}' is not defined.");
            return state;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < value.Length; i++)
                    hash = (hash * 31) + value[i];
                return hash;
            }
        }
    }
}
