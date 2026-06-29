using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Loot
{
    public sealed class Inventory
    {
        private readonly List<ItemInstance> _items = new();
        private readonly Dictionary<string, int> _materials = new();
        private readonly Dictionary<string, int> _currencies = new()
        {
            { "fate_shards", 0 },
            { "bingo_tokens", 0 },
            { "hero_medals", 0 }
        };

        public IReadOnlyList<ItemInstance> Items => _items;
        public IReadOnlyList<ItemInstance> AllItems => _items;
        public IReadOnlyDictionary<string, int> Materials => _materials;
        public IReadOnlyDictionary<string, int> AllMaterials => _materials;
        public IReadOnlyDictionary<string, int> Currencies => _currencies;
        public IReadOnlyDictionary<string, int> AllCurrencies => _currencies;

        public void AddItem(ItemInstance item)
        {
            if (item == null)
                return;

            _items.Add(item);
        }

        public bool RemoveItem(System.Guid instanceId)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].InstanceId == instanceId)
                {
                    _items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void AddMaterial(string materialId, int amount)
        {
            if (string.IsNullOrWhiteSpace(materialId) || amount <= 0)
                return;

            if (!_materials.ContainsKey(materialId))
                _materials[materialId] = 0;

            _materials[materialId] += amount;
        }

        public bool ConsumeMaterial(string materialId, int amount)
        {
            if (!_materials.TryGetValue(materialId, out var current) || amount <= 0 || current < amount)
                return false;

            _materials[materialId] = current - amount;
            return true;
        }

        public void AddCurrency(string currencyId, int amount)
        {
            if (!_currencies.ContainsKey(currencyId))
                _currencies[currencyId] = 0;

            _currencies[currencyId] = Mathf.Max(0, _currencies[currencyId] + amount);
        }

        public bool SpendCurrency(string currencyId, int amount)
        {
            if (!_currencies.TryGetValue(currencyId, out var current) || amount <= 0 || current < amount)
                return false;

            _currencies[currencyId] = current - amount;
            return true;
        }

        public int CountByRarity(ItemRarity rarity)
        {
            int count = 0;
            foreach (var item in _items)
            {
                if (item.Rarity == rarity)
                    count++;
            }

            return count;
        }
    }
}
