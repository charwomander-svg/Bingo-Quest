using BingoQuest.Gameplay.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Progression
{
    /// <summary>
    /// Represents a character class (Warrior, Mage, Ranger, Rogue, Cleric, etc.)
    /// with starting stats, abilities, and progression paths.
    /// </summary>
    public class ClassDefinition
    {
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string Description { get; set; }

        // Starting stats modifier (multiplied against base)
        public float HealthMultiplier { get; set; } = 1.0f;
        public float AttackMultiplier { get; set; } = 1.0f;
        public float DefenseMultiplier { get; set; } = 1.0f;
        public float CritChanceBonus { get; set; } = 0.0f;
        public float DodgeChanceBonus { get; set; } = 0.0f;

        // Starting abilities (slots 0-3)
        public List<string> StartingAbilityIds { get; set; } = new();

        // Class passive ability ID
        public string PassiveAbilityId { get; set; }

        public ClassDefinition() { }

        public ClassDefinition(string id, string name)
        {
            ClassId = id;
            ClassName = name;
        }

        public override string ToString() => $"{ClassName} ({ClassId})";
    }

    /// <summary>
    /// Preset class definitions (Warrior, Mage, Ranger, Rogue, Cleric).
    /// </summary>
    public static class BuiltInClasses
    {
        public static ClassDefinition Warrior => new ClassDefinition("warrior", "Warrior")
        {
            Description = "Melee tank with high health and defense",
            HealthMultiplier = 1.3f,
            AttackMultiplier = 1.1f,
            DefenseMultiplier = 1.4f,
            CritChanceBonus = -0.05f,
            DodgeChanceBonus = -0.05f,
            PassiveAbilityId = "warrior_shield_mastery"
        };

        public static ClassDefinition Mage => new ClassDefinition("mage", "Mage")
        {
            Description = "Ranged caster with high elemental damage",
            HealthMultiplier = 0.8f,
            AttackMultiplier = 1.4f,
            DefenseMultiplier = 0.7f,
            CritChanceBonus = 0.1f,
            DodgeChanceBonus = 0.1f,
            PassiveAbilityId = "mage_mana_shield"
        };

        public static ClassDefinition Ranger => new ClassDefinition("ranger", "Ranger")
        {
            Description = "Agile archer with high crit and dodge",
            HealthMultiplier = 1.0f,
            AttackMultiplier = 1.2f,
            DefenseMultiplier = 0.9f,
            CritChanceBonus = 0.15f,
            DodgeChanceBonus = 0.15f,
            PassiveAbilityId = "ranger_steady_aim"
        };

        public static ClassDefinition Rogue => new ClassDefinition("rogue", "Rogue")
        {
            Description = "Swift striker with high attack speed",
            HealthMultiplier = 0.9f,
            AttackMultiplier = 1.3f,
            DefenseMultiplier = 0.8f,
            CritChanceBonus = 0.2f,
            DodgeChanceBonus = 0.2f,
            PassiveAbilityId = "rogue_evasion"
        };

        public static ClassDefinition Cleric => new ClassDefinition("cleric", "Cleric")
        {
            Description = "Hybrid healer with support abilities",
            HealthMultiplier = 1.1f,
            AttackMultiplier = 0.9f,
            DefenseMultiplier = 1.1f,
            CritChanceBonus = 0.0f,
            DodgeChanceBonus = 0.05f,
            PassiveAbilityId = "cleric_holy_shield"
        };

        public static List<ClassDefinition> AllClasses => new()
        {
            Warrior, Mage, Ranger, Rogue, Cleric
        };
    }

    /// <summary>
    /// Applies class stats to a character.
    /// </summary>
    public static class ClassStatApplier
    {
        public static void ApplyClassStats(CharacterStats stats, ClassDefinition classDefinition)
        {
            stats.MaxHealth = (int)(100 * classDefinition.HealthMultiplier);
            stats.Health = stats.MaxHealth;
            stats.Attack = (int)(10 * classDefinition.AttackMultiplier);
            stats.Defense = (int)(5 * classDefinition.DefenseMultiplier);
            stats.CritChance = Mathf.Clamp01(0.1f + classDefinition.CritChanceBonus);
            stats.DodgeChance = Mathf.Clamp01(0.0f + classDefinition.DodgeChanceBonus);
        }
    }
}
