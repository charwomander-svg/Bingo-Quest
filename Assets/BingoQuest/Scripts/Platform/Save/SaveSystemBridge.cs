using System;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Objectives;
using BingoQuest.Gameplay.Progression;
using BingoQuest.Gameplay.Combat;

namespace BingoQuest.Platform.Save
{
    // SaveSystemBridge converts live runtime objects <-> SaveProfile DTOs.
    // This intentionally lives outside of Unity's MonoBehaviour lifecycle so it
    // can be unit tested without spinning up a scene.
    public static class SaveSystemBridge
    {
        public static void CaptureCharacter(SaveProfile profile, CharacterProgression progression, string characterId, string characterName)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (progression == null) throw new ArgumentNullException(nameof(progression));
            profile.Character.CharacterId = characterId;
            profile.Character.CharacterName = characterName;
            profile.Character.ClassId = progression.Class?.Id ?? "unknown";
            profile.Character.Level = progression.Level;
            profile.Character.Experience = progression.Experience;
            profile.Character.SkillPoints = progression.SkillPoints;
            profile.Character.UnlockedSkillNodeIds = new System.Collections.Generic.List<string>(progression.SkillTree.GetUnlockedNodeIds());
        }

        public static void CaptureInventory(SaveProfile profile, Inventory inventory)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            profile.Inventory.Items.Clear();
            foreach (var item in inventory.AllItems)
            {
                var dto = new ItemSaveData
                {
                    InstanceId = item.InstanceId.ToString(),
                    ItemId = item.Definition?.ItemId ?? "unknown_item",
                    DisplayName = item.Definition?.DisplayName ?? item.Definition?.ItemId ?? "Unknown Item",
                    ItemType = (int)(item.Definition?.Type ?? ItemType.Material),
                    ElementType = (int)(item.Definition?.Element ?? ElementType.Physical),
                    BasePower = item.Definition?.BasePower ?? Math.Max(1, item.RolledPower),
                    Rarity = (int)item.Rarity,
                    ItemLevel = item.ItemLevel,
                    RolledPower = item.RolledPower,
                };
                foreach (var affix in item.Affixes)
                    dto.Affixes.Add(new AffixSaveData { Name = affix.Name, Stat = affix.Stat, Value = affix.Value });
                profile.Inventory.Items.Add(dto);
            }

            profile.Inventory.Materials.Clear();
            foreach (var kv in inventory.AllMaterials)
                profile.Inventory.Materials[kv.Key] = kv.Value;

            profile.Inventory.Currencies.Clear();
            foreach (var kv in inventory.AllCurrencies)
                profile.Inventory.Currencies[kv.Key] = kv.Value;
        }

        public static void RestoreProgression(SaveProfile profile, CharacterProgression progression)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (progression == null) throw new ArgumentNullException(nameof(progression));
            var cd = profile.Character;
            progression.ForceSetLevel(Math.Max(1, cd.Level), Math.Max(0, cd.Experience));
            progression.SkillTree.Reset();
            foreach (var nodeId in cd.UnlockedSkillNodeIds)
                progression.SkillTree.ForceUnlock(nodeId);
            progression.RecalculateBonusesFromUnlockedSkills();
            progression.ForceSetSkillPoints(Math.Max(0, cd.SkillPoints));
        }

        public static void RestoreInventory(SaveProfile profile, Inventory inventory)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));

            inventory.ClearItems();
            inventory.ClearMaterials();
            inventory.ClearCurrencies();

            for (int i = 0; i < profile.Inventory.Items.Count; i++)
            {
                var item = profile.Inventory.Items[i];
                var definition = new ItemDefinition
                {
                    ItemId = item.ItemId,
                    DisplayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.ItemId : item.DisplayName,
                    Type = (ItemType)item.ItemType,
                    Element = (ElementType)item.ElementType,
                    BasePower = Math.Max(1, item.BasePower)
                };

                var instance = new ItemInstance
                {
                    Definition = definition,
                    Rarity = (ItemRarity)Math.Max(0, item.Rarity),
                    ItemLevel = Math.Max(1, item.ItemLevel),
                    RolledPower = Math.Max(1, item.RolledPower)
                };

                for (int j = 0; j < item.Affixes.Count; j++)
                {
                    var affix = item.Affixes[j];
                    instance.Affixes.Add(new ItemAffix
                    {
                        Name = affix.Name,
                        Stat = affix.Stat,
                        Value = affix.Value
                    });
                }

                inventory.AddItem(instance);
            }

            foreach (var kv in profile.Inventory.Materials)
                inventory.SetMaterial(kv.Key, kv.Value);
            foreach (var kv in profile.Inventory.Currencies)
                inventory.SetCurrency(kv.Key, kv.Value);
        }
    }
}
