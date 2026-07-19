using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Objectives;
using UnityEngine;

namespace BingoQuest.Gameplay.Loot
{
    public sealed class LootTableEntry
    {
        public ItemDefinition Definition { get; set; }
        public float Weight { get; set; } = 1f;
        public int MinimumItemLevel { get; set; } = 1;
    }

    public sealed class LootTable
    {
        private readonly List<LootTableEntry> _entries = new();

        public IReadOnlyList<LootTableEntry> Entries => _entries;

        public void AddEntry(LootTableEntry entry)
        {
            if (entry == null || entry.Definition == null || entry.Weight <= 0f)
                return;

            _entries.Add(entry);
        }

        public ItemDefinition RollDefinition(int playerLevel, System.Random random)
        {
            float totalWeight = 0f;
            foreach (var entry in _entries)
            {
                if (playerLevel >= entry.MinimumItemLevel)
                    totalWeight += entry.Weight;
            }

            if (totalWeight <= 0f)
                return null;

            var roll = (float)(random.NextDouble() * totalWeight);
            float cumulative = 0f;

            ItemDefinition lastEligible = null;
            foreach (var entry in _entries)
            {
                if (playerLevel < entry.MinimumItemLevel)
                    continue;

                lastEligible = entry.Definition;
                cumulative += entry.Weight;
                if (roll <= cumulative)
                    return entry.Definition;
            }

            return lastEligible;
        }
    }

    public sealed class ItemGenerator
    {
        private readonly System.Random _random;
        private readonly LootConfig _config;

        public ItemGenerator(int seed, LootConfig config = null)
        {
            _random = new System.Random(seed);
            _config = config;
        }

        public ItemGenerator(LootConfig config = null) : this(Environment.TickCount, config)
        {
        }

        public ItemInstance Generate(ItemDefinition definition, int playerLevel, float rarityBonus = 0f, bool isBossLoot = false)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            var rarity = RollRarity(rarityBonus, isBossLoot);
            int itemLevel = Mathf.Max(1, playerLevel);
            
            // Calculate power scaling
            float itemLevelMult = _config?.CalculateItemLevelMultiplier(itemLevel) ?? Mathf.Pow(1.0f, itemLevel - 1);
            float rarityMult = _config?.CalculateRarityMultiplier(rarity) ?? (1.0f + ((int)rarity * 0.25f));
            float bossMultiplier = isBossLoot && _config != null ? _config.BossLootPowerMultiplier : 1.0f;
            
            int variance = _random.Next((int)(_config?.PowerRollVariance ?? 90f), (int)(_config?.PowerRollVariance ?? 110f) + 1);
            int rolledPower = Mathf.Max(1, (int)(definition.BasePower * itemLevelMult * rarityMult * bossMultiplier * variance / 100f));

            var item = new ItemInstance
            {
                Definition = definition,
                Rarity = rarity,
                ItemLevel = itemLevel,
                RolledPower = rolledPower
            };

            AddAffixes(item);
            return item;
        }

        public List<ItemInstance> GenerateChestDrops(LootTable table, int playerLevel, int dropCount = -1)
        {
            var results = new List<ItemInstance>();
            if (table == null)
                return results;
            
            if (dropCount <= 0)
                dropCount = _config?.ChestDropCount ?? 3;

            for (int i = 0; i < dropCount; i++)
            {
                var definition = table.RollDefinition(playerLevel, _random);
                if (definition == null)
                    continue;

                float rarityBonus = _config?.ChestRarityBonus ?? 0.03f;
                results.Add(Generate(definition, playerLevel, rarityBonus));
            }

            return results;
        }

        private ItemRarity RollRarity(float rarityBonus, bool isBossLoot = false)
        {
            if (_config != null)
            {
                float roll = (float)_random.NextDouble();
                
                // Apply boss rarity bonus
                if (isBossLoot)
                {
                    rarityBonus += (1f / _config.BossRarityBonusMultiplier);
                }
                
                roll -= Mathf.Clamp(rarityBonus, 0f, 0.3f);
                
                var (common, uncommon, rare, epic, legendary, mythic) = _config.GetRarityWeights();
                
                if (roll < mythic) return ItemRarity.Mythic;
                if (roll < mythic + legendary) return ItemRarity.Legendary;
                if (roll < mythic + legendary + epic) return ItemRarity.Epic;
                if (roll < mythic + legendary + epic + rare) return ItemRarity.Rare;
                if (roll < mythic + legendary + epic + rare + uncommon) return ItemRarity.Uncommon;
                return ItemRarity.Common;
            }
            else
            {
                // Fallback to hardcoded weights
                var roll = (float)_random.NextDouble() - Mathf.Clamp(rarityBonus, 0f, 0.25f);
                if (roll < 0.005f) return ItemRarity.Mythic;
                if (roll < 0.02f) return ItemRarity.Legendary;
                if (roll < 0.08f) return ItemRarity.Epic;
                if (roll < 0.22f) return ItemRarity.Rare;
                if (roll < 0.48f) return ItemRarity.Uncommon;
                return ItemRarity.Common;
            }
        }

        private void AddAffixes(ItemInstance item)
        {
            int affixCount = _config?.GetAffixCountForRarity(item.Rarity) ?? item.Rarity switch
            {
                ItemRarity.Common => 0,
                ItemRarity.Uncommon => 1,
                ItemRarity.Rare => 2,
                ItemRarity.Epic => 3,
                ItemRarity.Legendary => 4,
                ItemRarity.Mythic => 5,
                _ => 0
            };

            for (int i = 0; i < affixCount; i++)
            {
                item.Affixes.Add(RollAffix(item.Rarity));
            }
        }

        private ItemAffix RollAffix(ItemRarity rarity)
        {
            string[] statPool = _config?.AffixableStats ?? new[] { "Attack", "Defense", "CritChance", "DodgeChance", "ElementalPower", "MaxHealth" };
            string stat = statPool[_random.Next(0, statPool.Length)];

            (float min, float max) = _config?.GetAffixValueRange(rarity) ?? (
                1f + ((int)rarity * 0.5f),
                4f + ((int)rarity * 1.5f)
            );
            
            float value = (float)(_random.NextDouble() * (max - min) + min);

            return new ItemAffix
            {
                Name = $"{stat} Boost",
                Stat = stat,
                Value = (float)Math.Round(value, 2)
            };
        }
    }

    public sealed class LootService
    {
        private readonly ItemGenerator _generator;
        private readonly Inventory _inventory;

        public LootService(ItemGenerator generator, Inventory inventory)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        public ItemInstance DropEnemyLoot(ItemDefinition definition, int playerLevel, float rarityBonus = 0f)
        {
            var item = _generator.Generate(definition, playerLevel, rarityBonus);
            _inventory.AddItem(item);
            ObjectiveEventBus.Instance?.Emit(ObjectiveEvent.ItemLooted(item.Definition.ItemId, (int)item.Rarity));
            return item;
        }

        public List<ItemInstance> OpenChest(LootTable table, int playerLevel, int dropCount = 3)
        {
            var drops = _generator.GenerateChestDrops(table, playerLevel, dropCount);
            foreach (var item in drops)
            {
                _inventory.AddItem(item);
                ObjectiveEventBus.Instance?.Emit(ObjectiveEvent.ItemLooted(item.Definition.ItemId, (int)item.Rarity));
            }

            ObjectiveEventBus.Instance?.Emit(ObjectiveEvent.ChestOpened());
            return drops;
        }
    }
}
