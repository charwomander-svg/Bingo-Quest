using System;
using System.Collections.Generic;
using System.Text;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Progression;

namespace BingoQuest.Presentation.UI
{
    public readonly struct CombatHudState
    {
        public CombatHudState(string healthText, float[] cooldownPercents, string statusEffectsText)
        {
            HealthText = healthText;
            CooldownPercents = cooldownPercents;
            StatusEffectsText = statusEffectsText;
        }

        public string HealthText { get; }
        public float[] CooldownPercents { get; }
        public string StatusEffectsText { get; }
    }

    public readonly struct BingoHudState
    {
        public BingoHudState(int completedSquares, bool[] squareCompleted, string[] objectiveIds)
        {
            CompletedSquares = completedSquares;
            SquareCompleted = squareCompleted;
            ObjectiveIds = objectiveIds;
        }

        public int CompletedSquares { get; }
        public bool[] SquareCompleted { get; }
        public string[] ObjectiveIds { get; }
    }

    public readonly struct ProgressionHudState
    {
        public ProgressionHudState(string classText, string levelText, string experienceText, string skillPointText)
        {
            ClassText = classText;
            LevelText = levelText;
            ExperienceText = experienceText;
            SkillPointText = skillPointText;
        }

        public string ClassText { get; }
        public string LevelText { get; }
        public string ExperienceText { get; }
        public string SkillPointText { get; }
    }

    public readonly struct InventoryHudState
    {
        public InventoryHudState(string itemCountText, string rarityBreakdownText, string currenciesText)
        {
            ItemCountText = itemCountText;
            RarityBreakdownText = rarityBreakdownText;
            CurrenciesText = currenciesText;
        }

        public string ItemCountText { get; }
        public string RarityBreakdownText { get; }
        public string CurrenciesText { get; }
    }

    public static class CombatHudPresenter
    {
        public static CombatHudState BuildState(CharacterStats stats, ActionBar actionBar, StatusEffectManager effects)
        {
            if (stats == null)
                return new CombatHudState("HP: N/A", new float[4], "No active effects");

            var cooldowns = new float[4];
            if (actionBar != null)
            {
                for (int i = 0; i < cooldowns.Length; i++)
                {
                    cooldowns[i] = actionBar.GetCooldownPercent((AbilitySlot)i);
                }
            }

            string statusText = "No active effects";
            if (effects != null && effects.ActiveEffects.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var pair in effects.ActiveEffects)
                {
                    if (sb.Length > 0)
                        sb.Append(" | ");

                    sb.Append(pair.Key);
                    sb.Append(" x");
                    sb.Append(pair.Value.StackCount);
                }

                statusText = sb.ToString();
            }

            return new CombatHudState(
                $"HP: {stats.Health}/{stats.MaxHealth}",
                cooldowns,
                statusText
            );
        }
    }

    public static class BingoHudPresenter
    {
        public static BingoHudState BuildState(BingoCard card)
        {
            if (card == null)
                return new BingoHudState(0, new bool[0], new string[0]);

            var completed = new bool[BingoCard.GRID_SIZE * BingoCard.GRID_SIZE];
            var objectiveIds = new string[BingoCard.GRID_SIZE * BingoCard.GRID_SIZE];

            for (int row = 0; row < BingoCard.GRID_SIZE; row++)
            {
                for (int col = 0; col < BingoCard.GRID_SIZE; col++)
                {
                    int index = (row * BingoCard.GRID_SIZE) + col;
                    var square = card.GetSquare(row, col);
                    completed[index] = square.IsCompleted;
                    objectiveIds[index] = square.ObjectiveId;
                }
            }

            return new BingoHudState(card.CompletedSquareCount, completed, objectiveIds);
        }
    }

    public static class ProgressionHudPresenter
    {
        public static ProgressionHudState BuildState(CharacterProgression progression)
        {
            if (progression == null)
                return new ProgressionHudState("Class: N/A", "Level: N/A", "EXP: N/A", "Skill Points: N/A");

            int expToNext = progression.GetExperienceForNextLevel();
            int spentEstimate = progression.LevelUpThreshold - expToNext;
            return new ProgressionHudState(
                $"Class: {progression.Class.ClassName}",
                $"Level: {progression.Level}",
                $"EXP: {spentEstimate}/{progression.LevelUpThreshold}",
                $"Skill Points: {progression.SkillPoints}"
            );
        }
    }

    public static class InventoryHudPresenter
    {
        public static InventoryHudState BuildState(Inventory inventory)
        {
            if (inventory == null)
                return new InventoryHudState("Items: 0", "Rarities: N/A", "Currencies: N/A");

            var rarityLines = new List<string>();
            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
            {
                int count = inventory.CountByRarity(rarity);
                if (count > 0)
                    rarityLines.Add($"{rarity}: {count}");
            }

            string rarityText = rarityLines.Count == 0 ? "Rarities: None" : $"Rarities: {string.Join(", ", rarityLines)}";

            var currencyPairs = new List<string>();
            foreach (var pair in inventory.Currencies)
                currencyPairs.Add($"{pair.Key}={pair.Value}");

            string currencies = currencyPairs.Count == 0 ? "Currencies: None" : $"Currencies: {string.Join(", ", currencyPairs)}";

            return new InventoryHudState(
                $"Items: {inventory.Items.Count}",
                rarityText,
                currencies
            );
        }
    }
}

