using System;
using BingoQuest.Core;
using BingoQuest.Gameplay.Progression;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Data-only description of an objective type.
    /// One instance per unique objective template lives in the objective pool.
    /// In a Unity project this would be a ScriptableObject; here it is a plain POCO
    /// to allow unit-testing without the Unity runtime.
    /// </summary>
    public sealed class ObjectiveDefinition
    {
        /// <summary>Unique stable identifier used to create and look up objectives.</summary>
        public string ObjectiveId { get; init; } = string.Empty;

        /// <summary>Discriminates which concrete <see cref="IObjective"/> type to instantiate.</summary>
        public ObjectiveType Type { get; init; }

        /// <summary>Base required-progress count (may be scaled by context at runtime).</summary>
        public int BaseRequiredCount { get; init; } = 1;

        /// <summary>Optional filter for enemy type, status type, ability id, etc.</summary>
        public string Filter { get; init; } = string.Empty;

        /// <summary>Element associated with this objective (for elemental kills).</summary>
        public ElementType Element { get; init; }

        /// <summary>If set, this objective is only valid in the specified zone.</summary>
        public string RequiredZoneId { get; init; } = string.Empty;

        /// <summary>If not None, restricted to a specific class.</summary>
        public ClassType RequiredClass { get; init; } = ClassType.None;

        /// <summary>Minimum difficulty tier for this objective to appear (0 = any).</summary>
        public int MinimumDifficultyTier { get; init; }
    }

    /// <summary>Discriminator used by <see cref="ObjectiveFactory"/>.</summary>
    public enum ObjectiveType
    {
        KillCount = 0,
        ElementalKill,
        CritKill,
        StatusApply,
        DodgeSuccess,
        AbilityUse,
        BossDefeat
    }
}
