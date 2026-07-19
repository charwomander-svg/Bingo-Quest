using System;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Strongly-typed event emitted by combat systems when gameplay actions occur.
    /// Objectives listen to these and update their progress accordingly.
    /// </summary>
    public readonly struct ObjectiveEvent
    {
        public ObjectiveEvent(
            ObjectiveEventType type,
            string sourceId,
            int amount,
            ElementType element = ElementType.Physical,
            bool isCritical = false,
            bool inCoop = false,
            bool inPvp = false,
            double timestamp = 0)
        {
            Type = type;
            SourceId = sourceId;
            Amount = amount;
            Element = element;
            IsCritical = isCritical;
            InCoop = inCoop;
            InPvp = inPvp;
            Timestamp = timestamp;
        }

        public ObjectiveEventType Type { get; }
        public string SourceId { get; }
        public int Amount { get; }
        public ElementType Element { get; }
        public bool IsCritical { get; }
        public bool InCoop { get; }
        public bool InPvp { get; }
        public double Timestamp { get; }

        public static ObjectiveEvent Kill(string enemyId) =>
            new ObjectiveEvent(ObjectiveEventType.Kill, enemyId, 1);

        public static ObjectiveEvent Damage(string sourceId, int amount, ElementType element = ElementType.Physical) =>
            new ObjectiveEvent(ObjectiveEventType.Damage, sourceId, amount, element);

        public static ObjectiveEvent CriticalHit(string sourceId, int amount) =>
            new ObjectiveEvent(ObjectiveEventType.CriticalHit, sourceId, amount, isCritical: true);

        public static ObjectiveEvent DodgeAction() =>
            new ObjectiveEvent(ObjectiveEventType.Dodge, "player", 1);

        public static ObjectiveEvent AbilityUsed(string abilityId) =>
            new ObjectiveEvent(ObjectiveEventType.AbilityUsed, abilityId, 1);

        public static ObjectiveEvent StatusEffectApplied(string effectName, ElementType element) =>
            new ObjectiveEvent(ObjectiveEventType.StatusEffectApplied, effectName, 1, element);

        public static ObjectiveEvent ItemLooted(string itemId, int rarity) =>
            new ObjectiveEvent(ObjectiveEventType.ItemLooted, itemId, rarity);

        public static ObjectiveEvent ChestOpened() =>
            new ObjectiveEvent(ObjectiveEventType.ChestOpened, "chest", 1);

        public static ObjectiveEvent BossDefeated(string bossId) =>
            new ObjectiveEvent(ObjectiveEventType.BossDefeated, bossId, 1);
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
