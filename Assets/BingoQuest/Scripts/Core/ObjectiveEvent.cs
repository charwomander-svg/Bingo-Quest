namespace BingoQuest.Core
{
    /// <summary>
    /// Immutable event value passed through the objective system.
    /// Emitted by combat, exploration, and economy systems.
    /// </summary>
    public readonly record struct ObjectiveEvent(
        ObjectiveEventType Type,
        string SourceId,
        int Amount,
        ElementType Element,
        bool IsCritical,
        bool InCoop,
        bool InPvp
    )
    {
        /// <summary>Creates a simple kill event with no special flags.</summary>
        public static ObjectiveEvent Kill(string enemyId, ElementType element = ElementType.None, bool isCritical = false)
            => new(ObjectiveEventType.EnemyKilled, enemyId, 1, element, isCritical, false, false);

        /// <summary>Creates a critical hit event.</summary>
        public static ObjectiveEvent Crit(string sourceId, ElementType element = ElementType.None)
            => new(ObjectiveEventType.CriticalHit, sourceId, 1, element, true, false, false);

        /// <summary>Creates a dodge event.</summary>
        public static ObjectiveEvent Dodge()
            => new(ObjectiveEventType.DodgeSuccess, string.Empty, 1, ElementType.None, false, false, false);

        /// <summary>Creates an ability-used event.</summary>
        public static ObjectiveEvent AbilityUsed(string abilityId)
            => new(ObjectiveEventType.AbilityUsed, abilityId, 1, ElementType.None, false, false, false);

        /// <summary>Creates a status-applied event.</summary>
        public static ObjectiveEvent StatusApplied(string statusId, ElementType element)
            => new(ObjectiveEventType.StatusApplied, statusId, 1, element, false, false, false);

        /// <summary>Creates a boss-defeated event.</summary>
        public static ObjectiveEvent BossDefeated(string bossId)
            => new(ObjectiveEventType.BossDefeated, bossId, 1, ElementType.None, false, false, false);
    }
}
