using BingoQuest.Gameplay.Loot;
using NUnit.Framework;

namespace BingoQuest.Tests.PlayMode
{
    public class LootSystemTests
    {
        [Test]
        public void ItemGenerator_CreatesItemWithExpectedCoreFields()
        {
            var generator = new ItemGenerator(seed: 42);
            var sword = new ItemDefinition
            {
                ItemId = "weapon_iron_sword",
                DisplayName = "Iron Sword",
                Type = ItemType.Weapon,
                BasePower = 8
            };

            var item = generator.Generate(sword, playerLevel: 5);

            Assert.AreEqual("weapon_iron_sword", item.Definition.ItemId);
            Assert.AreEqual(5, item.ItemLevel);
            Assert.Greater(item.RolledPower, 0);
        }

        [Test]
        public void ItemGenerator_HigherRarityAddsMoreAffixes()
        {
            var generator = new ItemGenerator(seed: 7);
            var definition = new ItemDefinition
            {
                ItemId = "staff",
                DisplayName = "Staff",
                Type = ItemType.Weapon,
                BasePower = 10
            };

            ItemInstance best = null;
            for (int i = 0; i < 200; i++)
            {
                var generated = generator.Generate(definition, 10, rarityBonus: 0.25f);
                if (best == null || generated.Rarity > best.Rarity)
                    best = generated;
            }

            Assert.IsNotNull(best);
            int expectedAffixCount = (int)best.Rarity;
            Assert.AreEqual(expectedAffixCount, best.Affixes.Count);
        }

        [Test]
        public void LootTable_RollsEligibleItemsByLevel()
        {
            var table = new LootTable();
            table.AddEntry(new LootTableEntry
            {
                Definition = new ItemDefinition { ItemId = "low", DisplayName = "Low", BasePower = 2, Type = ItemType.Material },
                Weight = 1f,
                MinimumItemLevel = 1
            });
            table.AddEntry(new LootTableEntry
            {
                Definition = new ItemDefinition { ItemId = "high", DisplayName = "High", BasePower = 10, Type = ItemType.Weapon },
                Weight = 1f,
                MinimumItemLevel = 10
            });

            var rng = new System.Random(123);
            var lowLevelRoll = table.RollDefinition(playerLevel: 2, rng);
            Assert.IsNotNull(lowLevelRoll);
            Assert.AreEqual("low", lowLevelRoll.ItemId);
        }

        [Test]
        public void Inventory_AddsAndRemovesItems()
        {
            var inventory = new Inventory();
            var item = new ItemInstance
            {
                Definition = new ItemDefinition { ItemId = "ring", DisplayName = "Ring", Type = ItemType.Accessory, BasePower = 1 },
                Rarity = ItemRarity.Rare,
                ItemLevel = 1,
                RolledPower = 1
            };

            inventory.AddItem(item);
            Assert.AreEqual(1, inventory.Items.Count);
            Assert.AreEqual(1, inventory.CountByRarity(ItemRarity.Rare));

            var removed = inventory.RemoveItem(item.InstanceId);
            Assert.IsTrue(removed);
            Assert.AreEqual(0, inventory.Items.Count);
        }

        [Test]
        public void Inventory_TracksCurrenciesAndMaterials()
        {
            var inventory = new Inventory();

            inventory.AddCurrency("fate_shards", 50);
            inventory.AddMaterial("ore_iron", 10);

            Assert.IsTrue(inventory.SpendCurrency("fate_shards", 20));
            Assert.AreEqual(30, inventory.Currencies["fate_shards"]);

            Assert.IsTrue(inventory.ConsumeMaterial("ore_iron", 5));
            Assert.AreEqual(5, inventory.Materials["ore_iron"]);
        }
    }
}
