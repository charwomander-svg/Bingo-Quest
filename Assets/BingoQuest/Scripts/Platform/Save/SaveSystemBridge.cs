using System;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Progression;
using BingoQuest.Gameplay.Combat;
using UnityEngine;

namespace BingoQuest.Platform.Save
{
    // ?????????????????????????????????????????????????????????????????????????????
    // SaveSystemBridge ? converts live runtime objects <-> SaveProfile DTOs.
    // This intentionally lives outside of Unity's MonoBehaviour lifecycle so it
    // can be unit tested without spinning up a scene.
    // ?????????????????????????????????????????????????????????????????????????????
    public static class SaveSystemBridge
    {
        // ?? Capture ??????????????????????????????????????????????????????????????
        public static void CaptureCharacter(SaveProfile profile, CharacterProgression progression, string characterId, string characterName)
        {
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
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            profile.Inventory.Items.Clear();
            foreach (var item in inventory.AllItems)
            {
                var dto = new ItemSaveData
                {
                    InstanceId = item.InstanceId.ToString(),
                    ItemId = item.Definition.Id,
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

        // ?? Restore ??????????????????????????????????????????????????????????????
        public static void RestoreProgression(SaveProfile profile, CharacterProgression progression)
        {
            if (progression == null) throw new ArgumentNullException(nameof(progression));
            var cd = profile.Character;
            progression.ForceSetLevel(cd.Level, cd.Experience);
            progression.ForceSetSkillPoints(cd.SkillPoints);
            foreach (var nodeId in cd.UnlockedSkillNodeIds)
                progression.SkillTree.ForceUnlock(nodeId);
        }
    }
}
