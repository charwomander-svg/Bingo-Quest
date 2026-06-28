using BingoQuest.Gameplay.Objectives;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Bingo
{
    /// <summary>
    /// Manages reward distribution when Bingo patterns complete.
    /// Decouples pattern completion from reward logic for easy customization.
    /// </summary>
    public interface IRewardHandler
    {
        void OnPatternComplete(BingoPattern pattern, BingoCard card);
    }

    /// <summary>
    /// Simple reward handler that grants buffs on pattern completion.
    /// Can be extended or swapped for different reward systems.
    /// </summary>
    public class PatternRewardHandler : IRewardHandler
    {
        public event Action<BingoPattern, string> OnRewardGranted;

        public void OnPatternComplete(BingoPattern pattern, BingoCard card)
        {
            string reward = pattern switch
            {
                BingoPattern.Row => "Row Complete: +20% Fire Damage",
                BingoPattern.Column => "Column Complete: +10% Dodge Chance",
                BingoPattern.DiagonalLeft => "Diagonal Complete: +15% Attack Speed",
                BingoPattern.DiagonalRight => "Diagonal Complete: +15% Critical Damage",
                BingoPattern.Corners => "Corners Complete: Summon Companion",
                BingoPattern.FullCard => "FULL HOUSE: Enter Bingo Avatar Overdrive (10s)",
                _ => "Unknown Reward"
            };

            OnRewardGranted?.Invoke(pattern, reward);
            Debug.Log($"<b>Pattern Reward:</b> {reward}");
        }
    }

    /// <summary>
    /// Main Bingo system: manages card state, objectives, pattern detection, and rewards.
    /// </summary>
    public class BingoSystem : MonoBehaviour
    {
        [SerializeField] private PatternRewardHandler rewardHandler = new();
        
        private BingoCard card;
        private PatternDetector patternDetector;
        private Dictionary<string, IObjective> objectiveMap = new();
        private ObjectiveContext objectiveContext;

        public static BingoSystem Instance { get; private set; }

        public event Action<int, int> OnSquareCompleted;
        public event Action<BingoPattern> OnPatternDetected;
        public event Action OnCardReset;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            card = new BingoCard();
            patternDetector = new PatternDetector(card);
            
            if (ObjectiveEventBus.Instance != null)
                ObjectiveEventBus.Instance.Subscribe(HandleObjectiveEvent);
        }

        /// <summary>Generate a new run with initial objectives.</summary>
        public void GenerateNewRun(ObjectiveContext context)
        {
            objectiveContext = context;
            card.Reset();
            objectiveMap.Clear();
            OnCardReset?.Invoke();

            // Populate card with objectives (placeholder - would be data-driven in production)
            var objectiveList = GenerateObjectivesForRun(context);
            for (int i = 0; i < objectiveList.Count && i < BingoCard.GRID_SIZE * BingoCard.GRID_SIZE; i++)
            {
                int row = i / BingoCard.GRID_SIZE;
                int col = i % BingoCard.GRID_SIZE;
                var objective = objectiveList[i];
                
                objective.Initialize(context);
                card.SetSquare(row, col, new BingoSquare(row, col, objective.ObjectiveId));
                objectiveMap[objective.ObjectiveId] = objective;
            }

            Debug.Log($"New Bingo run generated:\n{card}");
        }

        /// <summary>Handle incoming combat events and update objectives.</summary>
        private void HandleObjectiveEvent(ObjectiveEvent evt)
        {
            foreach (var objective in objectiveMap.Values)
            {
                objective.UpdateProgress(in evt);
                
                if (objective.IsComplete)
                {
                    CompleteObjective(objective.ObjectiveId);
                }
            }
        }

        /// <summary>Mark an objective as complete and evaluate patterns.</summary>
        private void CompleteObjective(string objectiveId)
        {
            // Find square matching this objective
            for (int row = 0; row < BingoCard.GRID_SIZE; row++)
            {
                for (int col = 0; col < BingoCard.GRID_SIZE; col++)
                {
                    var square = card.GetSquare(row, col);
                    if (square.ObjectiveId == objectiveId && !square.IsCompleted)
                    {
                        card.CompleteSquare(row, col);
                        OnSquareCompleted?.Invoke(row, col);
                        
                        // Re-evaluate patterns
                        EvaluatePatterns();
                        break;
                    }
                }
            }
        }

        /// <summary>Check for newly completed patterns and trigger rewards.</summary>
        private void EvaluatePatterns()
        {
            patternDetector.Evaluate();

            foreach (var newPattern in patternDetector.NewPatterns)
            {
                OnPatternDetected?.Invoke(newPattern);
                rewardHandler.OnPatternComplete(newPattern, card);
            }
        }

        /// <summary>Generate placeholder objectives for a run (data-driven in production).</summary>
        private List<IObjective> GenerateObjectivesForRun(ObjectiveContext context)
        {
            var objectives = new List<IObjective>
            {
                new KillEnemiesObjective("kill_5", 5),
                new DealDamageObjective("damage_100", 100),
                new CriticalHitsObjective("crits_3", 3),
                new UseAbilitiesObjective("abilities_10", 10),
                new DodgeActionsObjective("dodge_5", 5),
                new ApplyStatusEffectsObjective("burn_3", 3, ElementType.Fire),
                new LootItemsObjective("loot_5", 5),
                new DefeatBossObjective("boss_any"),
                new OpenChestsObjective("chests_3", 3),
                new DealDamageObjective("damage_fire_50", 50), // Fire damage
                new CriticalHitsObjective("crits_5", 5),
                new UseAbilitiesObjective("abilities_15", 15),
                new KillEnemiesObjective("kill_10", 10),
                new ApplyStatusEffectsObjective("freeze_2", 2, ElementType.Frost),
                new LootItemsObjective("rares_3", 3, 2),
                new DodgeActionsObjective("dodge_10", 10),
                new DealDamageObjective("damage_200", 200),
                new OpenChestsObjective("chests_5", 5),
                new DefeatBossObjective("boss_any_2"),
                new KillEnemiesObjective("kill_20", 20),
                new ApplyStatusEffectsObjective("any_status_5", 5),
                new UseAbilitiesObjective("abilities_20", 20),
                new LootItemsObjective("loot_10", 10),
                new CriticalHitsObjective("crits_10", 10),
                new DealDamageObjective("damage_300", 300),
            };

            return objectives;
        }

        public BingoCard GetCard() => card;
        public IObjective GetObjective(string id) => objectiveMap.TryGetValue(id, out var obj) ? obj : null;
        public int GetCompletedSquareCount() => card.CompletedSquareCount;

        private void OnDestroy()
        {
            if (ObjectiveEventBus.Instance != null)
                ObjectiveEventBus.Instance.Unsubscribe(HandleObjectiveEvent);
            if (Instance == this)
                Instance = null;
        }
    }
}
