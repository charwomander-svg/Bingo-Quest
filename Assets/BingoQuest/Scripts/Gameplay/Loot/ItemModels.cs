using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Objectives;

namespace BingoQuest.Gameplay.Loot
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Accessory,
        Consumable,
        Material
    }

    public enum ItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5
    }

    public sealed class ItemDefinition
    {
        public string ItemId { get; set; }
        public string DisplayName { get; set; }
        public ItemType Type { get; set; }
        public ElementType Element { get; set; } = ElementType.Physical;
        public int BasePower { get; set; } = 1;
    }

    public sealed class ItemAffix
    {
        public string Name { get; set; }
        public string Stat { get; set; }
        public float Value { get; set; }
    }

    public sealed class ItemInstance
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
        public ItemDefinition Definition { get; set; }
        public ItemRarity Rarity { get; set; }
        public int ItemLevel { get; set; }
        public int RolledPower { get; set; }
        public List<ItemAffix> Affixes { get; } = new();

        public override string ToString() =>
            $"{Definition?.DisplayName ?? "Unknown"} [{Rarity}] ilvl {ItemLevel} power {RolledPower}";
    }
}
