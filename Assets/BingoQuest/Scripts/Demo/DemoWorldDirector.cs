using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Objectives;
using BingoQuest.Gameplay.Progression;
using BingoQuest.Gameplay.Regions;
using UnityEngine;

namespace BingoQuest.Demo
{
    public sealed class DemoWorldDirector : MonoBehaviour
    {
        private readonly Dictionary<string, RegionVisualSettings> _visuals = new();

        private DemoEnemySpawner _spawner;
        private CharacterProgression _progression;
        private Renderer _floorRenderer;
        private Action<ObjectiveContext> _onObjectiveContextReady;
        private WorldMapService _worldMap;
        private int _transitionCursor;

        public string CurrentRegionName => _worldMap?.CurrentRegion?.DisplayName ?? "Unknown";
        public string CurrentRegionId => _worldMap?.CurrentRegionId ?? "unknown";
        public float CurrentEnemyHealthMultiplier => _worldMap?.CurrentRegion?.EnemyHealthMultiplier ?? 1f;
        public float CurrentEnemyAttackMultiplier => _worldMap?.CurrentRegion?.EnemyAttackMultiplier ?? 1f;

        public void Initialize(
            DemoEnemySpawner spawner,
            CharacterProgression progression,
            Renderer floorRenderer,
            Action<ObjectiveContext> onObjectiveContextReady)
        {
            _spawner = spawner;
            _progression = progression;
            _floorRenderer = floorRenderer;
            _onObjectiveContextReady = onObjectiveContextReady;
            _transitionCursor = 0;

            _worldMap = BuildWorldMap();
            _worldMap.OnRegionEntered += HandleRegionEntered;

            if (ObjectiveEventBus.Instance != null)
                ObjectiveEventBus.Instance.Subscribe(HandleObjectiveEvent);
        }

        public void BeginWorldRun()
        {
            _worldMap.UnlockRegion("town_hub");
            _worldMap.UnlockRegion("whispering_forest");
            _worldMap.TryEnterRegion("whispering_forest", out _);
        }

        public bool TravelToNextRegion()
        {
            var destinations = _worldMap.GetAvailableDestinations();
            if (destinations.Count == 0)
                return false;

            _transitionCursor = (_transitionCursor + 1) % destinations.Count;
            var next = destinations[_transitionCursor];
            return _worldMap.TryEnterRegion(next.RegionId, out _);
        }

        private WorldMapService BuildWorldMap()
        {
            var map = new WorldMapService();

            map.AddRegion(new RegionDefinition
            {
                RegionId = "town_hub",
                DisplayName = "Town Hub",
                BaseCardDifficulty = 1,
                EnemyHealthMultiplier = 0.8f,
                EnemyAttackMultiplier = 0.75f,
                SpawnIntervalSeconds = 9999f,
                MaxAliveEnemies = 0,
                SpawnRadius = 0f
            });
            map.AddRegion(new RegionDefinition
            {
                RegionId = "whispering_forest",
                DisplayName = "Whispering Forest",
                BaseCardDifficulty = 1,
                EnemyHealthMultiplier = 1f,
                EnemyAttackMultiplier = 1f,
                SpawnIntervalSeconds = 2.5f,
                MaxAliveEnemies = 8,
                SpawnRadius = 14f
            });
            map.AddRegion(new RegionDefinition
            {
                RegionId = "forgotten_mines",
                DisplayName = "Forgotten Mines",
                BaseCardDifficulty = 2,
                EnemyHealthMultiplier = 1.25f,
                EnemyAttackMultiplier = 1.2f,
                SpawnIntervalSeconds = 2.15f,
                MaxAliveEnemies = 10,
                SpawnRadius = 15f
            });
            map.AddRegion(new RegionDefinition
            {
                RegionId = "frozen_peaks",
                DisplayName = "Frozen Peaks",
                BaseCardDifficulty = 3,
                EnemyHealthMultiplier = 1.45f,
                EnemyAttackMultiplier = 1.35f,
                SpawnIntervalSeconds = 1.95f,
                MaxAliveEnemies = 12,
                SpawnRadius = 16f
            });

            map.ConnectBidirectional("town_hub", "whispering_forest");
            map.ConnectBidirectional("whispering_forest", "forgotten_mines");
            map.ConnectBidirectional("forgotten_mines", "frozen_peaks");

            _visuals["town_hub"] = new RegionVisualSettings(
                new Color(0.10f, 0.12f, 0.15f, 1f),
                new Color(0.28f, 0.25f, 0.20f, 1f),
                2.8f);
            _visuals["whispering_forest"] = new RegionVisualSettings(
                new Color(0.07f, 0.11f, 0.09f, 1f),
                new Color(0.16f, 0.30f, 0.17f, 1f),
                2.9f);
            _visuals["forgotten_mines"] = new RegionVisualSettings(
                new Color(0.10f, 0.09f, 0.08f, 1f),
                new Color(0.28f, 0.24f, 0.20f, 1f),
                3.1f);
            _visuals["frozen_peaks"] = new RegionVisualSettings(
                new Color(0.06f, 0.09f, 0.12f, 1f),
                new Color(0.70f, 0.75f, 0.82f, 1f),
                3.3f);

            return map;
        }

        private void HandleRegionEntered(WorldTransition transition, RegionDefinition region)
        {
            _spawner?.DespawnAllEnemies();
            ApplyVisuals(region.RegionId);
            _spawner?.ApplyRegionSettings(region.RegionId, region.SpawnIntervalSeconds, region.MaxAliveEnemies, region.SpawnRadius);

            int progressionDifficultyBonus = ((_progression?.Level ?? 1) - 1) / 5;
            var context = _worldMap.BuildObjectiveContext(
                _progression?.Class?.ClassId ?? "warrior",
                runSeedBase: 777,
                cardDifficultyBonus: progressionDifficultyBonus);

            _onObjectiveContextReady?.Invoke(context);
        }

        private void HandleObjectiveEvent(ObjectiveEvent evt)
        {
            if (evt.Type != ObjectiveEventType.BossDefeated)
                return;

            _worldMap.CompleteCurrentRegionBoss();
        }

        private void ApplyVisuals(string regionId)
        {
            if (!_visuals.TryGetValue(regionId, out var visual))
                return;

            if (Camera.main != null)
                Camera.main.backgroundColor = visual.BackgroundColor;

            if (_floorRenderer == null)
                return;

            _floorRenderer.transform.localScale = new Vector3(visual.FloorScale, 1f, visual.FloorScale);
            var material = _floorRenderer.material;
            material.color = visual.FloorColor;
        }

        private void OnDestroy()
        {
            if (_worldMap != null)
                _worldMap.OnRegionEntered -= HandleRegionEntered;
            if (ObjectiveEventBus.Instance != null)
                ObjectiveEventBus.Instance.Unsubscribe(HandleObjectiveEvent);
        }

        private readonly struct RegionVisualSettings
        {
            public Color BackgroundColor { get; }
            public Color FloorColor { get; }
            public float FloorScale { get; }

            public RegionVisualSettings(Color backgroundColor, Color floorColor, float floorScale)
            {
                BackgroundColor = backgroundColor;
                FloorColor = floorColor;
                FloorScale = floorScale;
            }
        }
    }
}
