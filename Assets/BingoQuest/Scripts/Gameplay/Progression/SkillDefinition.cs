using BingoQuest.Core;

namespace BingoQuest.Gameplay.Progression
{
    /// <summary>
    /// Data definition for a single active or passive skill/ability.
    /// In a Unity project this would be a ScriptableObject.
    /// </summary>
    public sealed class SkillDefinition
    {
        public string SkillId { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public bool IsPassive { get; init; }

        /// <summary>Cooldown in seconds. 0 for passives or instant skills.</summary>
        public float CooldownSeconds { get; init; }

        /// <summary>Base damage or healing value before stat scaling.</summary>
        public float BasePower { get; init; }

        /// <summary>Elemental type for damage and objective matching.</summary>
        public ElementType Element { get; init; }

        /// <summary>Status effect inflicted on hit (if any).</summary>
        public Combat.StatusEffectType AppliesStatus { get; init; }

        /// <summary>Duration of applied status in seconds. 0 if no status.</summary>
        public float StatusDuration { get; init; }

        /// <summary>Required class. None = usable by all.</summary>
        public ClassType RequiredClass { get; init; } = ClassType.None;
    }
}
