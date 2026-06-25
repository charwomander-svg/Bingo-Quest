using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Bingo;

namespace BingoQuest.Gameplay.Progression
{
    /// <summary>
    /// Tracks all transient data for the current run (resets on new run).
    /// Serialisable for autosave support.
    /// </summary>
    public sealed class RunProgress
    {
        // ── Identity ──────────────────────────────────────────────────────────

        public string RunId { get; } = Guid.NewGuid().ToString("N");
        public ClassType PlayerClass { get; init; }
        public string ZoneId { get; init; } = string.Empty;
        public int DifficultyTier { get; init; }
        public DateTime StartedAt { get; } = DateTime.UtcNow;

        // ── Stats ─────────────────────────────────────────────────────────────

        public int EnemiesKilled { get; private set; }
        public int CriticalHits { get; private set; }
        public int DodgesPerformed { get; private set; }
        public int StatusEffectsApplied { get; private set; }
        public int BossesDefeated { get; private set; }
        public float TotalDamageDealt { get; private set; }
        public float TotalDamageTaken { get; private set; }
        public float TotalHealingDone { get; private set; }
        public int ObjectivesCompleted { get; private set; }
        public int CardsCompleted { get; private set; }

        // ── Pattern tracking ──────────────────────────────────────────────────

        private readonly List<PatternType> _patternsCompleted = new();
        public IReadOnlyList<PatternType> PatternsCompleted => _patternsCompleted;

        // ── Mutators (called by the game layer) ───────────────────────────────

        public void RecordKill(bool isCritical)
        {
            EnemiesKilled++;
            if (isCritical) CriticalHits++;
        }

        public void RecordCrit() => CriticalHits++;
        public void RecordDodge() => DodgesPerformed++;
        public void RecordStatusApplied() => StatusEffectsApplied++;
        public void RecordBossDefeated() => BossesDefeated++;
        public void AddDamageDealt(float amount) => TotalDamageDealt += amount;
        public void AddDamageTaken(float amount) => TotalDamageTaken += amount;
        public void AddHealingDone(float amount) => TotalHealingDone += amount;
        public void RecordObjectiveCompleted() => ObjectivesCompleted++;
        public void RecordCardCompleted() => CardsCompleted++;
        public void RecordPatternCompleted(PatternType pattern) => _patternsCompleted.Add(pattern);
    }
}
