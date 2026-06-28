using System;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Strongly-typed event emitted by combat systems when gameplay actions occur.
    /// Objectives listen to these and update their progress accordingly.
    /// </summary>
    public readonly record struct ObjectiveEvent(
        ObjectiveEventType Type,
        string SourceId,
        int Amount,
        ElementType Element = ElementType.Physical,
        bool IsCritical = false,
        bool InCoop = false,
        bool InPvp = false,
        double Timestamp = 0
    )
    {
        public static ObjectiveEvent Kill(string enemyId) =>
            new(ObjectiveEventType.Kill, enemyId, 1);

        public static ObjectiveEvent Damage(string sourceId, int amount, ElementType element = ElementType.Physical) =>
            new(ObjectiveEventType.Damage, sourceId, amount, element);

        public static ObjectiveEvent CriticalHit(string sourceId, int amount) =>
            new(ObjectiveEventType.CriticalHit, sourceId, amount, IsCritical: true);

        public static ObjectiveEvent DodgeAction() =>
            new(ObjectiveEventType.Dodge, "player", 1);

        public static ObjectiveEvent AbilityUsed(string abilityId) =>
            new(ObjectiveEventType.AbilityUsed, abilityId, 1);

        public static ObjectiveEvent StatusEffectApplied(string effectName, ElementType element) =>
            new(ObjectiveEventType.StatusEffectApplied, effectName, 1, element);

        public static ObjectiveEvent ItemLooted(string itemId, int rarity) =>
            new(ObjectiveEventType.ItemLooted, itemId, rarity);

        public static ObjectiveEvent ChestOpened() =>
            new(ObjectiveEventType.ChestOpened, "chest", 1);

        public static ObjectiveEvent BossDefeated(string bossId) =>
            new(ObjectiveEventType.BossDefeated, bossId, 1);
    }

    public enum ObjectiveEventType
    {
        Kill,
        Damage,
        CriticalHit,
        Dodge,
        AbilityUsed,
        StatusEffectApplied,
        ItemLooted,
        ChestOpened,
        BossDefeated,
        TimeSurvived,
        AreaEntered,
        NPCInteracted
    }

    public enum ElementType
    {
        Physical = 0,
        Fire = 1,
        Frost = 2,
        Lightning = 3,
        Poison = 4,
        Holy = 5,
        Shadow = 6
    }
}
