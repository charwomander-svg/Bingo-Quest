using System.Collections.Generic;

namespace BingoQuest.Gameplay.Progression
{
    /// <summary>
    /// Data definition for a playable class.
    /// In a Unity project this would be a ScriptableObject; here it is a plain POCO
    /// so it can be instantiated and tested without the Unity runtime.
    /// </summary>
    public sealed class ClassDefinition
    {
        /// <summary>Stable identifier (matches <see cref="ClassType"/> name).</summary>
        public string ClassId { get; init; } = string.Empty;

        public ClassType ClassType { get; init; }

        public string DisplayName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;

        /// <summary>Whether this class must be unlocked via meta progression.</summary>
        public bool IsHidden { get; init; }

        // ── Base stats ────────────────────────────────────────────────────────

        public float BaseMaxHealth { get; init; }
        public float BaseAttackPower { get; init; }
        public float BaseAbilityPower { get; init; }
        public float BaseArmor { get; init; }
        public float BaseMagicResist { get; init; }
        public float BaseCritChance { get; init; }
        public float BaseDodgeChance { get; init; }
        public float BaseMoveSpeed { get; init; }

        /// <summary>Starting ability identifiers (up to 4 active + 1 passive).</summary>
        public IReadOnlyList<string> StartingAbilityIds { get; init; } = new List<string>();

        // ── Pre-built class definitions ───────────────────────────────────────

        public static ClassDefinition Warrior() => new()
        {
            ClassId = "Warrior",
            ClassType = ClassType.Warrior,
            DisplayName = "Warrior",
            Description = "A front-line fighter who excels at melee combat and drawing enemy attention.",
            BaseMaxHealth    = 250f,
            BaseAttackPower  = 80f,
            BaseAbilityPower = 30f,
            BaseArmor        = 40f,
            BaseMagicResist  = 20f,
            BaseCritChance   = 0.10f,
            BaseDodgeChance  = 0.05f,
            BaseMoveSpeed    = 5f,
            StartingAbilityIds = new[] { "warrior_slash", "warrior_shield_bash", "warrior_rally", "warrior_whirlwind" }
        };

        public static ClassDefinition Mage() => new()
        {
            ClassId = "Mage",
            ClassType = ClassType.Mage,
            DisplayName = "Mage",
            Description = "A glass-cannon spellcaster who deals massive elemental damage.",
            BaseMaxHealth    = 140f,
            BaseAttackPower  = 25f,
            BaseAbilityPower = 120f,
            BaseArmor        = 10f,
            BaseMagicResist  = 35f,
            BaseCritChance   = 0.15f,
            BaseDodgeChance  = 0.08f,
            BaseMoveSpeed    = 4.5f,
            StartingAbilityIds = new[] { "mage_fireball", "mage_frost_nova", "mage_arcane_bolt", "mage_blink" }
        };

        public static ClassDefinition Ranger() => new()
        {
            ClassId = "Ranger",
            ClassType = ClassType.Ranger,
            DisplayName = "Ranger",
            Description = "A ranged marksman who picks off enemies and places traps.",
            BaseMaxHealth    = 175f,
            BaseAttackPower  = 70f,
            BaseAbilityPower = 50f,
            BaseArmor        = 20f,
            BaseMagicResist  = 15f,
            BaseCritChance   = 0.18f,
            BaseDodgeChance  = 0.12f,
            BaseMoveSpeed    = 5.5f,
            StartingAbilityIds = new[] { "ranger_arrow_shot", "ranger_multi_shot", "ranger_poison_trap", "ranger_hawk_eye" }
        };

        public static ClassDefinition Rogue() => new()
        {
            ClassId = "Rogue",
            ClassType = ClassType.Rogue,
            DisplayName = "Rogue",
            Description = "A high-speed skirmisher who stacks bleeds and crits.",
            BaseMaxHealth    = 160f,
            BaseAttackPower  = 75f,
            BaseAbilityPower = 40f,
            BaseArmor        = 15f,
            BaseMagicResist  = 15f,
            BaseCritChance   = 0.22f,
            BaseDodgeChance  = 0.18f,
            BaseMoveSpeed    = 6.5f,
            StartingAbilityIds = new[] { "rogue_backstab", "rogue_smoke_bomb", "rogue_blade_dance", "rogue_shadowstep" }
        };

        public static ClassDefinition Cleric() => new()
        {
            ClassId = "Cleric",
            ClassType = ClassType.Cleric,
            DisplayName = "Cleric",
            Description = "A holy warrior who heals allies and smites undead.",
            BaseMaxHealth    = 200f,
            BaseAttackPower  = 55f,
            BaseAbilityPower = 80f,
            BaseArmor        = 30f,
            BaseMagicResist  = 40f,
            BaseCritChance   = 0.08f,
            BaseDodgeChance  = 0.06f,
            BaseMoveSpeed    = 4.8f,
            StartingAbilityIds = new[] { "cleric_smite", "cleric_heal", "cleric_holy_shield", "cleric_consecrate" }
        };
    }
}
