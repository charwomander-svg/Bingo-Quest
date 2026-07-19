using UnityEngine;

namespace BingoQuest.Gameplay.Balance
{
    /// <summary>
    /// Centralized balance configuration for all combat and difficulty parameters.
    /// Exposed as a ScriptableObject so designers can tune without rebuilding code.
    /// </summary>
    [CreateAssetMenu(menuName = "BingoQuest/Balance Config", fileName = "BalanceConfig")]
    public class BalanceConfig : ScriptableObject
    {
        [Header("Damage Scaling")]
        [SerializeField][Range(0.5f, 2f)] private float baseDamageScale = 1.0f;
        [SerializeField][Range(1f, 3f)] private float critDamageMultiplier = 1.5f;
        [SerializeField][Range(0.01f, 1f)] private float elementalPowerScale = 1.0f;

        [Header("Defense & Mitigation")]
        [SerializeField][Range(0.1f, 1f)] private float defenseMitigationFactor = 0.5f; // How much DEF reduces damage
        [SerializeField][Range(0f, 1f)] private float minDamagePercent = 0.05f; // Minimum damage as % of base

        [Header("Difficulty Multipliers")]
        [SerializeField][Range(0.5f, 2f)] private float easyModifier = 0.75f;
        [SerializeField][Range(0.5f, 2f)] private float normalModifier = 1.0f;
        [SerializeField][Range(0.5f, 2f)] private float hardModifier = 1.35f;
        [SerializeField][Range(0.5f, 2f)] private float nightmareModifier = 1.75f;

        [Header("Enemy HP Scaling")]
        [SerializeField][Range(0.8f, 1.2f)] private float levelHealthMultiplier = 1.05f; // Health per level
        [SerializeField] private int baseEnemyHealth = 50;
        [SerializeField][Range(1f, 3f)] private float bossHealthMultiplier = 3.0f;

        [Header("Region Scaling")]
        [SerializeField] private float startingRegionHealthMult = 1.0f;
        [SerializeField] private float startingRegionDamageMult = 1.0f;
        [SerializeField] private float regionHealthMultStep = 0.35f;  // Increase per region tier
        [SerializeField] private float regionDamageMultStep = 0.25f;

        [Header("Progression")]
        [SerializeField] private int baseXPPerEnemy = 10;
        [SerializeField][Range(1f, 1.5f)] private float xpScalingPerLevel = 1.08f;
        [SerializeField] private float bossDamageMultiplier = 1.5f;

        [Header("Loot Multipliers")]
        [SerializeField][Range(0.5f, 2f)] private float commonDropRate = 1.0f;
        [SerializeField][Range(0.5f, 2f)] private float rareDropRate = 1.0f;
        [SerializeField][Range(0.5f, 2f)] private float epicDropRate = 1.0f;

        public float BaseDamageScale => baseDamageScale;
        public float CritDamageMultiplier => Mathf.Clamp(critDamageMultiplier, 1f, 3f);
        public float ElementalPowerScale => elementalPowerScale;

        public float DefenseMitigationFactor => defenseMitigationFactor;
        public float MinDamagePercent => minDamagePercent;

        public float GetDifficultyMultiplier(BingoQuest.Gameplay.Progression.DifficultyMode mode) => mode switch
        {
            BingoQuest.Gameplay.Progression.DifficultyMode.Easy => easyModifier,
            BingoQuest.Gameplay.Progression.DifficultyMode.Normal => normalModifier,
            BingoQuest.Gameplay.Progression.DifficultyMode.Hard => hardModifier,
            BingoQuest.Gameplay.Progression.DifficultyMode.Nightmare => nightmareModifier,
            _ => normalModifier
        };

        public float LevelHealthMultiplier => levelHealthMultiplier;
        public int BaseEnemyHealth => baseEnemyHealth;
        public float BossHealthMultiplier => bossHealthMultiplier;

        public float StartingRegionHealthMult => startingRegionHealthMult;
        public float StartingRegionDamageMult => startingRegionDamageMult;
        public float RegionHealthMultStep => regionHealthMultStep;
        public float RegionDamageMultStep => regionDamageMultStep;

        public int BaseXPPerEnemy => baseXPPerEnemy;
        public float XPScalingPerLevel => xpScalingPerLevel;
        public float BossDamageMultiplier => bossDamageMultiplier;

        public float GetCommonDropRate() => Mathf.Clamp01(commonDropRate);
        public float GetRareDropRate() => Mathf.Clamp01(rareDropRate);
        public float GetEpicDropRate() => Mathf.Clamp01(epicDropRate);

        /// <summary>Calculate enemy base health scaled by player level.</summary>
        public int CalculateEnemyHealth(int playerLevel, bool isBoss = false)
        {
            int leveledHealth = (int)(baseEnemyHealth * Mathf.Pow(levelHealthMultiplier, playerLevel - 1));
            return isBoss ? (int)(leveledHealth * bossHealthMultiplier) : leveledHealth;
        }

        /// <summary>Get region multiplier for enemy stats based on region tier.</summary>
        public (float health, float damage) GetRegionMultipliers(int regionTier)
        {
            float healthMult = startingRegionHealthMult + (regionHealthMultStep * regionTier);
            float damageMult = startingRegionDamageMult + (regionDamageMultStep * regionTier);
            return (Mathf.Max(0.5f, healthMult), Mathf.Max(0.5f, damageMult));
        }

        /// <summary>Calculate minimum damage threshold to prevent reduction to 0.</summary>
        public int CalculateMinDamage(int baseDamage)
        {
            return Mathf.Max(1, (int)(baseDamage * minDamagePercent));
        }
    }

}
