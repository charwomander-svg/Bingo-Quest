namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Immutable snapshot of a damage application before and after modifiers.
    /// Produced by <see cref="DamagePipeline"/> and read by combat components.
    /// </summary>
    public readonly record struct DamageData(
        string ActorId,
        string TargetId,
        float BaseDamage,
        float FinalDamage,
        Core.ElementType Element,
        bool IsCritical,
        bool IsStatusTick,
        StatusEffectType SourceStatus
    )
    {
        /// <summary>Net critical multiplier applied (FinalDamage / BaseDamage if IsCritical).</summary>
        public float CritMultiplier =>
            (IsCritical && BaseDamage > 0f) ? FinalDamage / BaseDamage : 1f;

        /// <summary>Creates a simple physical damage record.</summary>
        public static DamageData Physical(string actorId, string targetId,
            float baseDamage, float finalDamage, bool isCritical) =>
            new(actorId, targetId, baseDamage, finalDamage, Core.ElementType.None, isCritical, false, StatusEffectType.None);

        /// <summary>Creates an elemental damage record.</summary>
        public static DamageData Elemental(string actorId, string targetId,
            float baseDamage, float finalDamage, Core.ElementType element, bool isCritical) =>
            new(actorId, targetId, baseDamage, finalDamage, element, isCritical, false, StatusEffectType.None);

        /// <summary>Creates a damage-over-time tick record.</summary>
        public static DamageData DotTick(string actorId, string targetId,
            float damage, Core.ElementType element, StatusEffectType status) =>
            new(actorId, targetId, damage, damage, element, false, true, status);
    }
}
