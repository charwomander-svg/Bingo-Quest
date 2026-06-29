using System;
using System.Collections.Generic;

namespace BingoQuest.Platform.Save
{
    public sealed class CharacterSaveData
    {
        public string CharacterId { get; set; }
        public string CharacterName { get; set; }
        public string ClassId { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int SkillPoints { get; set; }
        public List<string> UnlockedSkillNodeIds { get; set; } = new();
    }

    public sealed class ItemSaveData
    {
        public string InstanceId { get; set; }
        public string ItemId { get; set; }
        public int Rarity { get; set; }
        public int ItemLevel { get; set; }
        public int RolledPower { get; set; }
        public List<AffixSaveData> Affixes { get; set; } = new();
    }

    public sealed class AffixSaveData
    {
        public string Name { get; set; }
        public string Stat { get; set; }
        public float Value { get; set; }
    }

    public sealed class InventorySaveData
    {
        public List<ItemSaveData> Items { get; set; } = new();
        public Dictionary<string, int> Materials { get; set; } = new();
        public Dictionary<string, int> Currencies { get; set; } = new();
    }

    public sealed class StatisticsSaveData
    {
        public int EnemiesKilled { get; set; }
        public int CardsCompleted { get; set; }
        public int ObjectivesCompleted { get; set; }
        public int BossesDefeated { get; set; }
        public int TotalDamageDealt { get; set; }
        public double FastestCardSeconds { get; set; } = double.MaxValue;
        public string RarestItemId { get; set; }
    }

    public sealed class SaveProfile
    {
        public string ProfileId { get; set; }
        public string DisplayName { get; set; }
        public int SchemaVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastSavedAtUtc { get; set; }
        public double TotalPlaySeconds { get; set; }

        public CharacterSaveData Character { get; set; } = new();
        public InventorySaveData Inventory { get; set; } = new();
        public StatisticsSaveData Statistics { get; set; } = new();
        public List<string> CompletedQuestIds { get; set; } = new();
        public List<string> AchievementIds { get; set; } = new();
        public Dictionary<string, string> Flags { get; set; } = new();

        public static int CurrentSchemaVersion => 1;
    }
}
