using UnityEngine;

namespace BingoQuest.Gameplay.Loot
{
    /// <summary>
    /// Centralized loot economy configuration for drop rates, rarity weights, affix scaling, and rewards.
    /// Designers tune this to control gear progression and economy without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "BingoQuest/Loot Config", fileName = "LootConfig")]
    public class LootConfig : ScriptableObject
    {
        [Header("Rarity Distribution (summed to ~100 for weighting)")]
        [SerializeField][Range(30f, 80f)] private float commonWeight = 50f;
        [SerializeField][Range(10f, 40f)] private float uncommonWeight = 25f;
        [SerializeField][Range(5f, 20f)] private float rareWeight = 15f;
        [SerializeField][Range(1f, 10f)] private float epicWeight = 7f;
        [SerializeField][Range(0.1f, 3f)] private float legendaryWeight = 2f;
        [SerializeField][Range(0.01f, 1f)] private float mythicWeight = 0.5f;

        [Header("Rarity Bonus Modifiers")]
        [SerializeField][Range(0.5f, 1.5f)] private float rarityPowerMultiplier = 1.0f; // Base power multiplier per rarity tier
        [SerializeField][Range(1f, 5f)] private float rarityAfixBonusScale = 1.0f; // How much rarity increases affix values

        [Header("Affix Generation")]
        [SerializeField][Range(0f, 1f)] private float affixValueBaseMin = 1f;
        [SerializeField][Range(1f, 5f)] private float affixValueBaseMax = 4f;
        [SerializeField][Range(0.5f, 2f)] private float affixValueRarityScale = 0.5f; // Increase per rarity level
        [SerializeField] private string[] affixableStats = { "Attack", "Defense", "CritChance", "DodgeChance", "ElementalPower", "MaxHealth" };

        [Header("Item Level Scaling")]
        [SerializeField][Range(0.8f, 1.2f)] private float powerPerItemLevel = 1.0f; // Multiplier per item level
        [SerializeField][Range(90f, 110f)] private float powerRollVariance = 100f; // Variance in power generation (90-110 = ±10%)

        [Header("Boss & Milestone Drops")]
        [SerializeField][Range(1f, 5f)] private float bossDearityBonusMultiplier = 2.0f; // Boss drops are more rare
        [SerializeField][Range(1, 5)] private int chestDropCount = 3;
        [SerializeField][Range(0.5f, 2f)] private float chestRarityBonus = 0.05f; // Bonus rarity roll per chest item
        [SerializeField][Range(1f, 10f)] private float bossLootPowerMultiplier = 1.5f;

        [Header("Drop Rate Modifiers")]
        [SerializeField][Range(0f, 1f)] private float enemyDropRate = 0.8f; // Chance to drop loot on kill
        [SerializeField][Range(0f, 1f)] private float chestDropRate = 1.0f; // Chance to drop item from chest
        [SerializeField][Range(0f, 1f)] private float bossDropRate = 1.0f;  // Boss always drops loot

        public float CommonWeight => commonWeight;
        public float UncommonWeight => uncommonWeight;
        public float RareWeight => rareWeight;
        public float EpicWeight => epicWeight;
        public float LegendaryWeight => legendaryWeight;
        public float MythicWeight => mythicWeight;

        public float RarityPowerMultiplier => rarityPowerMultiplier;
        public float RarityAffixBonusScale => rarityAfixBonusScale;

        public float AffixValueBaseMin => affixValueBaseMin;
        public float AffixValueBaseMax => affixValueBaseMax;
        public float AffixValueRarityScale => affixValueRarityScale;
        public string[] AffixableStats => affixableStats;

        public float PowerPerItemLevel => powerPerItemLevel;
        public float PowerRollVariance => powerRollVariance;

        public float BossRarityBonusMultiplier => bossDearityBonusMultiplier;
        public int ChestDropCount => chestDropCount;
        public float ChestRarityBonus => chestRarityBonus;
        public float BossLootPowerMultiplier => bossLootPowerMultiplier;

        public float EnemyDropRate => Mathf.Clamp01(enemyDropRate);
        public float ChestDropRate => Mathf.Clamp01(chestDropRate);
        public float BossDropRate => Mathf.Clamp01(bossDropRate);

        /// <summary>Get all rarity weights as normalized probabilities (0-1).</summary>
        public (float common, float uncommon, float rare, float epic, float legendary, float mythic) GetRarityWeights()
        {
            float total = commonWeight + uncommonWeight + rareWeight + epicWeight + legendaryWeight + mythicWeight;
            if (total <= 0) total = 1f; // Prevent division by zero

            return (
                commonWeight / total,
                uncommonWeight / total,
                rareWeight / total,
                epicWeight / total,
                legendaryWeight / total,
                mythicWeight / total
            );
        }

        /// <summary>Get rarity weight for a specific rarity.</summary>
        public float GetRarityWeight(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Common => commonWeight,
            ItemRarity.Uncommon => uncommonWeight,
            ItemRarity.Rare => rareWeight,
            ItemRarity.Epic => epicWeight,
            ItemRarity.Legendary => legendaryWeight,
            ItemRarity.Mythic => mythicWeight,
            _ => commonWeight
        };

        /// <summary>Determine number of affixes for a rarity.</summary>
        public int GetAffixCountForRarity(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Common => 0,
            ItemRarity.Uncommon => 1,
            ItemRarity.Rare => 2,
            ItemRarity.Epic => 3,
            ItemRarity.Legendary => 4,
            ItemRarity.Mythic => 5,
            _ => 0
        };

        /// <summary>Calculate power scaling based on item level and rarity.</summary>
        public float CalculateItemLevelMultiplier(int itemLevel)
        {
            return Mathf.Pow(powerPerItemLevel, itemLevel - 1);
        }

        /// <summary>Calculate power multiplier for rarity.</summary>
        public float CalculateRarityMultiplier(ItemRarity rarity)
        {
            float rarityValue = (int)rarity; // 0-5
            return Mathf.Pow(rarityPowerMultiplier, rarityValue);
        }

        /// <summary>Get affix value range for a specific rarity.</summary>
        public (float min, float max) GetAffixValueRange(ItemRarity rarity)
        {
            float rarityBonus = (int)rarity * affixValueRarityScale;
            return (
                affixValueBaseMin + rarityBonus,
                affixValueBaseMax + rarityBonus
            );
        }

        /// <summary>Validate configuration is sensible.</summary>
        public bool IsValid()
        {
            if (commonWeight < 0 || uncommonWeight < 0 || rareWeight < 0) return false;
            if (epicWeight < 0 || legendaryWeight < 0 || mythicWeight < 0) return false;
            if (affixableStats == null || affixableStats.Length == 0) return false;
            if (powerPerItemLevel <= 0) return false;
            if (chestDropCount <= 0) return false;
            return true;
        }
    }
}
