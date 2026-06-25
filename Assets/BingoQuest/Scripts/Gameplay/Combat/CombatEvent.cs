using BingoQuest.Core;

namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Immutable event describing a combat-layer occurrence.
    /// Published through <see cref="CombatEventBus"/> and translated to
    /// <see cref="Core.ObjectiveEvent"/> records for the objective system.
    /// </summary>
    public readonly record struct CombatEvent(
        CombatEventCategory Category,
        string ActorId,
        string TargetId,
        float Amount,
        ElementType Element,
        bool IsCritical,
        StatusEffectType StatusEffect
    )
    {
        // ── Factory helpers ───────────────────────────────────────────────────

        public static CombatEvent DamageDealt(string actorId, string targetId,
            float amount, ElementType element, bool isCritical) =>
            new(CombatEventCategory.DamageDealt, actorId, targetId, amount, element, isCritical, StatusEffectType.None);

        public static CombatEvent EnemyKilled(string actorId, string enemyId,
            ElementType lastHitElement, bool isCritical) =>
            new(CombatEventCategory.EnemyKilled, actorId, enemyId, 0f, lastHitElement, isCritical, StatusEffectType.None);

        public static CombatEvent StatusApplied(string actorId, string targetId, StatusEffectType status) =>
            new(CombatEventCategory.StatusApplied, actorId, targetId, 0f, ElementType.None, false, status);

        public static CombatEvent DodgePerformed(string actorId) =>
            new(CombatEventCategory.DodgePerformed, actorId, string.Empty, 0f, ElementType.None, false, StatusEffectType.None);

        public static CombatEvent AbilityUsed(string actorId, string abilityId) =>
            new(CombatEventCategory.AbilityUsed, actorId, abilityId, 0f, ElementType.None, false, StatusEffectType.None);

        public static CombatEvent BossDefeated(string actorId, string bossId) =>
            new(CombatEventCategory.BossDefeated, actorId, bossId, 0f, ElementType.None, false, StatusEffectType.None);
    }

    /// <summary>High-level categories for a <see cref="CombatEvent"/>.</summary>
    public enum CombatEventCategory
    {
        DamageDealt = 0,
        DamageTaken,
        EnemyKilled,
        StatusApplied,
        StatusExpired,
        DodgePerformed,
        AbilityUsed,
        HealingDone,
        BossDefeated,
        PlayerDied
    }
}
