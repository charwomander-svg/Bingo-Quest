using BingoQuest.Gameplay.Balance;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Progression;
using System.Reflection;
using UnityEngine;

namespace BingoQuest.Gameplay.Difficulty
{
    /// <summary>
    /// Code-driven factory for the four built-in difficulty presets.
    /// Creates fully wired DifficultyPreset objects without requiring Unity .asset files.
    /// </summary>
    public static class DifficultyPresetFactory
    {
        public static DifficultyPreset CreateEasy()
        {
            var balance = ScriptableObject.CreateInstance<BalanceConfig>();
            SetPrivate(balance, "baseEnemyHealth", 38);
            SetPrivate(balance, "easyModifier", 0.55f);
            SetPrivate(balance, "normalModifier", 0.70f);
            SetPrivate(balance, "hardModifier", 0.90f);
            SetPrivate(balance, "nightmareModifier", 1.15f);
            SetPrivate(balance, "regionHealthMultStep", 0.20f);
            SetPrivate(balance, "regionDamageMultStep", 0.15f);
            SetPrivate(balance, "commonDropRate", 1.2f);
            SetPrivate(balance, "rareDropRate", 1.5f);
            SetPrivate(balance, "epicDropRate", 1.8f);

            var progression = ScriptableObject.CreateInstance<ProgressionConfig>();
            SetPrivate(progression, "baseXPThreshold", 80);
            SetPrivate(progression, "easyXPMultiplier", 1.4f);
            SetPrivate(progression, "normalXPMultiplier", 1.2f);
            SetPrivate(progression, "hardXPMultiplier", 1.0f);
            SetPrivate(progression, "nightmareXPMultiplier", 0.8f);
            SetPrivate(progression, "startingSkillPoints", 3);

            var loot = ScriptableObject.CreateInstance<LootConfig>();
            SetPrivate(loot, "commonWeight", 40f);
            SetPrivate(loot, "uncommonWeight", 28f);
            SetPrivate(loot, "rareWeight", 18f);
            SetPrivate(loot, "epicWeight", 9f);
            SetPrivate(loot, "legendaryWeight", 3.5f);
            SetPrivate(loot, "enemyDropRate", 0.95f);

            var ability = ScriptableObject.CreateInstance<AbilityConfig>();
            SetPrivate(ability, "easyCooldownMultiplier", 0.55f);
            SetPrivate(ability, "normalCooldownMultiplier", 0.70f);
            SetPrivate(ability, "hardCooldownMultiplier", 0.90f);
            SetPrivate(ability, "nightmareCooldownMultiplier", 1.10f);
            SetPrivate(ability, "globalDamageMultiplier", 1.15f);

            return Build(
                "Easy",
                "Relaxed experience. Great for learning mechanics.",
                1.0f,
                new Color(0.25f, 0.75f, 0.30f, 1f),
                balance, progression, loot, ability);
        }

        public static DifficultyPreset CreateNormal()
        {
            var balance = ScriptableObject.CreateInstance<BalanceConfig>();
            // Normal uses all default values from ScriptableObject

            var progression = ScriptableObject.CreateInstance<ProgressionConfig>();

            var loot = ScriptableObject.CreateInstance<LootConfig>();

            var ability = ScriptableObject.CreateInstance<AbilityConfig>();

            return Build(
                "Normal",
                "Balanced challenge. The intended experience.",
                2.0f,
                new Color(0.25f, 0.55f, 0.90f, 1f),
                balance, progression, loot, ability);
        }

        public static DifficultyPreset CreateHard()
        {
            var balance = ScriptableObject.CreateInstance<BalanceConfig>();
            SetPrivate(balance, "baseEnemyHealth", 65);
            SetPrivate(balance, "easyModifier", 0.80f);
            SetPrivate(balance, "normalModifier", 1.10f);
            SetPrivate(balance, "hardModifier", 1.50f);
            SetPrivate(balance, "nightmareModifier", 2.00f);
            SetPrivate(balance, "regionHealthMultStep", 0.45f);
            SetPrivate(balance, "regionDamageMultStep", 0.35f);
            SetPrivate(balance, "defenseMitigationFactor", 0.35f);

            var progression = ScriptableObject.CreateInstance<ProgressionConfig>();
            SetPrivate(progression, "baseXPThreshold", 115);
            SetPrivate(progression, "easyXPMultiplier", 1.0f);
            SetPrivate(progression, "normalXPMultiplier", 0.85f);
            SetPrivate(progression, "hardXPMultiplier", 0.70f);
            SetPrivate(progression, "nightmareXPMultiplier", 0.55f);
            SetPrivate(progression, "startingSkillPoints", 1);

            var loot = ScriptableObject.CreateInstance<LootConfig>();
            SetPrivate(loot, "commonWeight", 58f);
            SetPrivate(loot, "uncommonWeight", 22f);
            SetPrivate(loot, "rareWeight", 12f);
            SetPrivate(loot, "enemyDropRate", 0.70f);

            var ability = ScriptableObject.CreateInstance<AbilityConfig>();
            SetPrivate(ability, "easyCooldownMultiplier", 0.80f);
            SetPrivate(ability, "normalCooldownMultiplier", 1.10f);
            SetPrivate(ability, "hardCooldownMultiplier", 1.45f);
            SetPrivate(ability, "nightmareCooldownMultiplier", 1.80f);

            return Build(
                "Hard",
                "Punishing combat. Careful positioning required.",
                3.0f,
                new Color(0.90f, 0.60f, 0.15f, 1f),
                balance, progression, loot, ability);
        }

        public static DifficultyPreset CreateNightmare()
        {
            var balance = ScriptableObject.CreateInstance<BalanceConfig>();
            SetPrivate(balance, "baseEnemyHealth", 90);
            SetPrivate(balance, "easyModifier", 1.00f);
            SetPrivate(balance, "normalModifier", 1.50f);
            SetPrivate(balance, "hardModifier", 2.00f);
            SetPrivate(balance, "nightmareModifier", 2.80f);
            SetPrivate(balance, "regionHealthMultStep", 0.60f);
            SetPrivate(balance, "regionDamageMultStep", 0.50f);
            SetPrivate(balance, "defenseMitigationFactor", 0.20f);
            SetPrivate(balance, "bossHealthMultiplier", 4.5f);

            var progression = ScriptableObject.CreateInstance<ProgressionConfig>();
            SetPrivate(progression, "baseXPThreshold", 140);
            SetPrivate(progression, "easyXPMultiplier", 0.8f);
            SetPrivate(progression, "normalXPMultiplier", 0.65f);
            SetPrivate(progression, "hardXPMultiplier", 0.50f);
            SetPrivate(progression, "nightmareXPMultiplier", 0.35f);
            SetPrivate(progression, "startingSkillPoints", 1);
            SetPrivate(progression, "skillPointsPerLevel", 1);

            var loot = ScriptableObject.CreateInstance<LootConfig>();
            SetPrivate(loot, "commonWeight", 65f);
            SetPrivate(loot, "uncommonWeight", 20f);
            SetPrivate(loot, "rareWeight", 9f);
            SetPrivate(loot, "epicWeight", 4f);
            SetPrivate(loot, "legendaryWeight", 1.5f);
            SetPrivate(loot, "enemyDropRate", 0.55f);
            SetPrivate(loot, "bossLootPowerMultiplier", 2.5f);

            var ability = ScriptableObject.CreateInstance<AbilityConfig>();
            SetPrivate(ability, "easyCooldownMultiplier", 1.00f);
            SetPrivate(ability, "normalCooldownMultiplier", 1.40f);
            SetPrivate(ability, "hardCooldownMultiplier", 1.80f);
            SetPrivate(ability, "nightmareCooldownMultiplier", 2.20f);
            SetPrivate(ability, "globalDamageMultiplier", 0.85f);

            return Build(
                "Nightmare",
                "Brutally hard. Mastery of every system required.",
                5.0f,
                new Color(0.82f, 0.18f, 0.18f, 1f),
                balance, progression, loot, ability);
        }

        public static DifficultyPreset[] CreateAll()
        {
            return new[]
            {
                CreateEasy(),
                CreateNormal(),
                CreateHard(),
                CreateNightmare()
            };
        }

        private static DifficultyPreset Build(
            string name,
            string description,
            float rating,
            Color color,
            BalanceConfig balance,
            ProgressionConfig progression,
            LootConfig loot,
            AbilityConfig ability)
        {
            var preset = ScriptableObject.CreateInstance<DifficultyPreset>();
            preset.name = name;

            var info = new DifficultyPreset.PresetInfo
            {
                PresetName = name,
                Description = description,
                DifficultyRating = rating,
                PresetColor = color
            };

            SetPrivate(preset, "presetInfo", info);
            SetPrivate(preset, "balanceConfig", balance);
            SetPrivate(preset, "progressionConfig", progression);
            SetPrivate(preset, "lootConfig", loot);
            SetPrivate(preset, "abilityConfig", ability);

            return preset;
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
