using BingoQuest.Gameplay.Loot;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace BingoQuest.Tests.EditMode
{
    public class LootEconomyTests
    {
        private LootConfig lootConfig;
        private ItemGenerator generator;
        private ItemDefinition testItem;

        [SetUp]
        public void Setup()
        {
            lootConfig = ScriptableObject.CreateInstance<LootConfig>();
            generator = new ItemGenerator(12345, lootConfig);
            testItem = new ItemDefinition
            {
                ItemId = "test_sword",
                DisplayName = "Test Sword",
                BasePower = 10
            };
        }

        [Test]
        public void LootConfigIsValid()
        {
            Assert.That(lootConfig.IsValid(), Is.True);
        }

        [Test]
        public void RarityWeights_SumToOne()
        {
            var (common, uncommon, rare, epic, legendary, mythic) = lootConfig.GetRarityWeights();
            float total = common + uncommon + rare + epic + legendary + mythic;
            
            Assert.That(total, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void GetRarityWeight_ReturnsWeightForAllRarities()
        {
            Assert.That(lootConfig.GetRarityWeight(ItemRarity.Common), Is.GreaterThan(0f));
            Assert.That(lootConfig.GetRarityWeight(ItemRarity.Uncommon), Is.GreaterThan(0f));
            Assert.That(lootConfig.GetRarityWeight(ItemRarity.Rare), Is.GreaterThan(0f));
            Assert.That(lootConfig.GetRarityWeight(ItemRarity.Epic), Is.GreaterThan(0f));
            Assert.That(lootConfig.GetRarityWeight(ItemRarity.Legendary), Is.GreaterThan(0f));
            Assert.That(lootConfig.GetRarityWeight(ItemRarity.Mythic), Is.GreaterThan(0f));
        }

        [Test]
        public void AffixCount_MatchesRarity()
        {
            Assert.That(lootConfig.GetAffixCountForRarity(ItemRarity.Common), Is.EqualTo(0));
            Assert.That(lootConfig.GetAffixCountForRarity(ItemRarity.Uncommon), Is.EqualTo(1));
            Assert.That(lootConfig.GetAffixCountForRarity(ItemRarity.Rare), Is.EqualTo(2));
            Assert.That(lootConfig.GetAffixCountForRarity(ItemRarity.Epic), Is.EqualTo(3));
            Assert.That(lootConfig.GetAffixCountForRarity(ItemRarity.Legendary), Is.EqualTo(4));
            Assert.That(lootConfig.GetAffixCountForRarity(ItemRarity.Mythic), Is.EqualTo(5));
        }

        [Test]
        public void GenerateItem_CreatesValidItem()
        {
            var item = generator.Generate(testItem, playerLevel: 5);
            
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Definition, Is.EqualTo(testItem));
            Assert.That(item.ItemLevel, Is.EqualTo(5));
            Assert.That(item.RolledPower, Is.GreaterThan(0));
            Assert.That(item.Affixes.Count, Is.EqualTo(lootConfig.GetAffixCountForRarity(item.Rarity)));
        }

        [Test]
        public void GenerateItem_HigherLevelHasMorePower()
        {
            var level1 = generator.Generate(testItem, playerLevel: 1);
            var level10 = generator.Generate(testItem, playerLevel: 10);
            var level20 = generator.Generate(testItem, playerLevel: 20);
            
            Assert.That(level10.RolledPower, Is.GreaterThanOrEqualTo(level1.RolledPower));
            Assert.That(level20.RolledPower, Is.GreaterThanOrEqualTo(level10.RolledPower));
        }

        [Test]
        public void RarityMultiplier_IncreasesWithRarity()
        {
            float commonMult = lootConfig.CalculateRarityMultiplier(ItemRarity.Common);
            float rareMult = lootConfig.CalculateRarityMultiplier(ItemRarity.Rare);
            float epicMult = lootConfig.CalculateRarityMultiplier(ItemRarity.Epic);
            float mythicMult = lootConfig.CalculateRarityMultiplier(ItemRarity.Mythic);
            
            Assert.That(rareMult, Is.GreaterThan(commonMult));
            Assert.That(epicMult, Is.GreaterThan(rareMult));
            Assert.That(mythicMult, Is.GreaterThan(epicMult));
        }

        [Test]
        public void ItemLevelMultiplier_IncreasesWithLevel()
        {
            float level1 = lootConfig.CalculateItemLevelMultiplier(1);
            float level5 = lootConfig.CalculateItemLevelMultiplier(5);
            float level10 = lootConfig.CalculateItemLevelMultiplier(10);
            
            Assert.That(level5, Is.GreaterThan(level1));
            Assert.That(level10, Is.GreaterThan(level5));
        }

        [Test]
        public void AffixValueRange_IncreaseWithRarity()
        {
            var (cMin, cMax) = lootConfig.GetAffixValueRange(ItemRarity.Common);
            var (rMin, rMax) = lootConfig.GetAffixValueRange(ItemRarity.Rare);
            var (eMin, eMax) = lootConfig.GetAffixValueRange(ItemRarity.Epic);
            
            Assert.That(rMin, Is.GreaterThanOrEqualTo(cMin));
            Assert.That(rMax, Is.GreaterThan(cMax));
            Assert.That(eMin, Is.GreaterThanOrEqualTo(rMin));
            Assert.That(eMax, Is.GreaterThan(rMax));
        }

        [Test]
        public void GenerateItem_BossLoot_HasHigherPower()
        {
            // Generate many items to account for randomness
            const int samples = 100;
            float normalTotal = 0;
            float bossTotal = 0;
            
            for (int i = 0; i < samples; i++)
            {
                normalTotal += generator.Generate(testItem, 10, isBossLoot: false).RolledPower;
                bossTotal += generator.Generate(testItem, 10, isBossLoot: true).RolledPower;
            }
            
            float normalAvg = normalTotal / samples;
            float bossAvg = bossTotal / samples;
            
            Assert.That(bossAvg, Is.GreaterThan(normalAvg));
        }

        [Test]
        public void GenerateItem_WithRarityBonus_MoreLikelyRare()
        {
            // Generate items with and without rarity bonus
            const int samples = 100;
            int rareOrBetterNormal = 0;
            int rareOrBetterBonused = 0;
            
            for (int i = 0; i < samples; i++)
            {
                var normal = generator.Generate(testItem, 10, rarityBonus: 0f);
                if (normal.Rarity >= ItemRarity.Rare)
                    rareOrBetterNormal++;
                
                var bonused = generator.Generate(testItem, 10, rarityBonus: 0.1f);
                if (bonused.Rarity >= ItemRarity.Rare)
                    rareOrBetterBonused++;
            }
            
            Assert.That(rareOrBetterBonused, Is.GreaterThan(rareOrBetterNormal));
        }

        [Test]
        public void GenerateChestDrops_ReturnsExpectedCount()
        {
            var lootTable = new LootTable();
            lootTable.AddEntry(new LootTableEntry { Definition = testItem, Weight = 1f });
            
            var drops = generator.GenerateChestDrops(lootTable, playerLevel: 10);
            
            Assert.That(drops.Count, Is.EqualTo(lootConfig.ChestDropCount));
        }

        [Test]
        public void GenerateChestDrops_CustomDropCount()
        {
            var lootTable = new LootTable();
            lootTable.AddEntry(new LootTableEntry { Definition = testItem, Weight = 1f });
            
            var drops = generator.GenerateChestDrops(lootTable, playerLevel: 10, dropCount: 5);
            
            Assert.That(drops.Count, Is.EqualTo(5));
        }

        [Test]
        public void GenerateChestDrops_AllValidItems()
        {
            var lootTable = new LootTable();
            lootTable.AddEntry(new LootTableEntry { Definition = testItem, Weight = 1f });
            
            var drops = generator.GenerateChestDrops(lootTable, playerLevel: 10, dropCount: 3);
            
            foreach (var item in drops)
            {
                Assert.That(item, Is.Not.Null);
                Assert.That(item.RolledPower, Is.GreaterThan(0));
                Assert.That(item.Affixes.Count, Is.GreaterThanOrEqualTo(0));
            }
        }

        [Test]
        public void GenerateWithoutConfig_UsesDefaults()
        {
            var defaultGen = new ItemGenerator(12345, config: null);
            var item = defaultGen.Generate(testItem, playerLevel: 10);
            
            Assert.That(item, Is.Not.Null);
            Assert.That(item.RolledPower, Is.GreaterThan(0));
        }

        [Test]
        public void RarityDistribution_IsBiasedTowardCommon()
        {
            const int samples = 1000;
            var rarityCount = new Dictionary<ItemRarity, int>();
            foreach (ItemRarity rarity in System.Enum.GetValues(typeof(ItemRarity)))
            {
                rarityCount[rarity] = 0;
            }
            
            for (int i = 0; i < samples; i++)
            {
                var item = generator.Generate(testItem, 10);
                rarityCount[item.Rarity]++;
            }
            
            // Common should be most common
            Assert.That(rarityCount[ItemRarity.Common], Is.GreaterThan(rarityCount[ItemRarity.Uncommon]));
            Assert.That(rarityCount[ItemRarity.Uncommon], Is.GreaterThan(rarityCount[ItemRarity.Rare]));
            Assert.That(rarityCount[ItemRarity.Rare], Is.GreaterThan(rarityCount[ItemRarity.Epic]));
            Assert.That(rarityCount[ItemRarity.Epic], Is.GreaterThan(rarityCount[ItemRarity.Legendary]));
        }

        [Test]
        public void LootTable_RollsWeightedDefinitions()
        {
            var item1 = new ItemDefinition { ItemId = "sword", DisplayName = "Sword", BasePower = 10 };
            var item2 = new ItemDefinition { ItemId = "shield", DisplayName = "Shield", BasePower = 5 };
            
            var lootTable = new LootTable();
            lootTable.AddEntry(new LootTableEntry { Definition = item1, Weight = 10f });
            lootTable.AddEntry(new LootTableEntry { Definition = item2, Weight = 1f });
            
            var random = new System.Random(12345);
            const int samples = 100;
            var item1Count = 0;
            
            for (int i = 0; i < samples; i++)
            {
                var rolled = lootTable.RollDefinition(1, random);
                if (rolled.ItemId == "sword")
                    item1Count++;
            }
            
            // Sword has 10x weight, so should appear ~90% of time
            Assert.That(item1Count, Is.GreaterThan(samples * 0.7f));
        }
    }
}
