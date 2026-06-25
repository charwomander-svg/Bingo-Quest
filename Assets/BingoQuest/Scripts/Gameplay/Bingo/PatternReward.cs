using BingoQuest.Gameplay.Combat;

namespace BingoQuest.Gameplay.Bingo
{
    /// <summary>
    /// Defines the buff or power-spike reward granted when a Bingo pattern is completed.
    /// Matches the design spec:
    ///   Row    → offensive buff
    ///   Column → defensive/utility buff
    ///   Diagonal → special unlock
    ///   FullCard → "Bingo Avatar" overdrive
    /// </summary>
    public enum PatternRewardType
    {
        None = 0,
        OffensiveBuff,      // Row completion
        DefensiveBuff,      // Column completion
        UtilityBuff,        // Special column variants
        SpecialUnlock,      // Diagonal – unlock companion / ability
        OverdriveState,     // Full card – Bingo Avatar
        ChestSpawn,         // Extra loot chest
        StatBonus,          // Flat stat boost for the rest of the run
        CooldownReset       // All ability cooldowns reset immediately
    }

    /// <summary>
    /// Data describing the reward granted upon completing a <see cref="PatternType"/>.
    /// </summary>
    public sealed class PatternReward
    {
        /// <summary>Which pattern triggers this reward.</summary>
        public PatternType Pattern { get; init; }

        /// <summary>Category of the reward effect.</summary>
        public PatternRewardType RewardType { get; init; }

        /// <summary>Short player-facing label shown in the HUD toast.</summary>
        public string DisplayLabel { get; init; } = string.Empty;

        /// <summary>Description shown in the card UI tooltip.</summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Magnitude of the buff as a multiplier or flat value, depending on <see cref="RewardType"/>.
        /// Examples: 0.20 = +20 %, 100 = +100 flat damage.
        /// </summary>
        public float Magnitude { get; init; }

        /// <summary>Duration in seconds. 0 = permanent for the run.</summary>
        public float DurationSeconds { get; init; }

        /// <summary>
        /// Optional element associated with the buff (used for elemental damage bonuses).
        /// </summary>
        public Core.ElementType BonusElement { get; init; }

        // ── Factory helpers matching the design spec ──────────────────────────

        public static PatternReward RowOffensive(int rowIndex, Core.ElementType element, float bonusPct) =>
            new()
            {
                Pattern = PatternType.Row,
                RewardType = PatternRewardType.OffensiveBuff,
                DisplayLabel = $"Row {rowIndex + 1} Bonus",
                Description = $"+{bonusPct * 100:0}% {element} damage",
                Magnitude = bonusPct,
                BonusElement = element,
                DurationSeconds = 0f // permanent for run
            };

        public static PatternReward ColumnDefensive(int colIndex, float dodgeBonusPct) =>
            new()
            {
                Pattern = PatternType.Column,
                RewardType = PatternRewardType.DefensiveBuff,
                DisplayLabel = $"Column {colIndex + 1} Bonus",
                Description = $"+{dodgeBonusPct * 100:0}% dodge chance",
                Magnitude = dodgeBonusPct,
                DurationSeconds = 0f
            };

        public static PatternReward DiagonalSpecial(string unlockLabel) =>
            new()
            {
                Pattern = PatternType.DiagonalMain,
                RewardType = PatternRewardType.SpecialUnlock,
                DisplayLabel = "Diagonal Complete",
                Description = unlockLabel,
                Magnitude = 1f,
                DurationSeconds = 0f
            };

        public static PatternReward FullCardOverdrive(float durationSeconds) =>
            new()
            {
                Pattern = PatternType.FullCard,
                RewardType = PatternRewardType.OverdriveState,
                DisplayLabel = "BINGO AVATAR",
                Description = "Overdrive state unlocked!",
                Magnitude = 1f,
                DurationSeconds = durationSeconds
            };
    }
}
