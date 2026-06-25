using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Objectives;
using BingoQuest.Gameplay.Progression;

namespace BingoQuest.Core
{
    /// <summary>
    /// Bootstraps and wires all game services for a run session.
    /// Call <see cref="Initialise"/> once at game start / scene load.
    /// In a Unity project this would live on a persistent GameObject in the Boot scene.
    /// </summary>
    public sealed class GameBootstrap
    {
        private CombatEventBus? _combatEventBus;
        private BingoCard? _activeCard;
        private RunProgress? _runProgress;

        /// <summary>The combat event bus wired during initialisation.</summary>
        public CombatEventBus CombatEventBus =>
            _combatEventBus ?? throw new InvalidOperationException("Not yet initialised.");

        /// <summary>The active bingo card for the current run.</summary>
        public BingoCard ActiveCard =>
            _activeCard ?? throw new InvalidOperationException("Not yet initialised.");

        /// <summary>Run-level statistics tracker.</summary>
        public RunProgress RunProgress =>
            _runProgress ?? throw new InvalidOperationException("Not yet initialised.");

        /// <summary>
        /// Wires all core services and generates the opening card.
        /// </summary>
        public void Initialise(
            ClassType playerClass,
            string zoneId,
            int difficultyTier,
            int characterLevel,
            IReadOnlyList<ObjectiveDefinition> objectivePool,
            int cardSeed = 0)
        {
            // Create and register the event bus
            _combatEventBus = new CombatEventBus();
            ServiceLocator.Register(_combatEventBus);

            // Create run progress tracker
            _runProgress = new RunProgress
            {
                PlayerClass = playerClass,
                ZoneId = zoneId,
                DifficultyTier = difficultyTier
            };

            // Generate the opening bingo card
            var generator = new CardGenerator(objectivePool);
            var request = new CardGenerationRequest
            {
                ZoneId = zoneId,
                PlayerClass = playerClass,
                DifficultyTier = difficultyTier,
                CharacterLevel = characterLevel,
                Seed = cardSeed
            };
            _activeCard = generator.Generate(request);

            // Wire combat events → bingo card objectives
            _combatEventBus.OnObjectiveEvent += evt => _activeCard.ProcessEvent(evt);

            // Wire bingo card events → run progress
            _activeCard.OnSquareMarked += (_, _) => _runProgress.RecordObjectiveCompleted();
            _activeCard.OnPatternCompleted += (_, pattern) => _runProgress.RecordPatternCompleted(pattern);
            _activeCard.OnFullCard += _ => _runProgress.RecordCardCompleted();

            // Wire combat events → run stats
            _combatEventBus.OnCombatEvent += TrackRunStats;
        }

        /// <summary>Tears down all wired events (call on scene unload).</summary>
        public void Shutdown()
        {
            if (_combatEventBus != null)
                _combatEventBus.OnCombatEvent -= TrackRunStats;
            ServiceLocator.Reset();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void TrackRunStats(CombatEvent evt)
        {
            if (_runProgress is null) return;
            switch (evt.Category)
            {
                case CombatEventCategory.EnemyKilled:
                    _runProgress.RecordKill(evt.IsCritical);
                    break;
                case CombatEventCategory.DodgePerformed:
                    _runProgress.RecordDodge();
                    break;
                case CombatEventCategory.StatusApplied:
                    _runProgress.RecordStatusApplied();
                    break;
                case CombatEventCategory.BossDefeated:
                    _runProgress.RecordBossDefeated();
                    break;
                case CombatEventCategory.DamageDealt:
                    _runProgress.AddDamageDealt(evt.Amount);
                    if (evt.IsCritical) _runProgress.RecordCrit();
                    break;
                case CombatEventCategory.DamageTaken:
                    _runProgress.AddDamageTaken(evt.Amount);
                    break;
                case CombatEventCategory.HealingDone:
                    _runProgress.AddHealingDone(evt.Amount);
                    break;
            }
        }
    }
}
