using BingoQuest.Gameplay.Progression;

namespace BingoQuest.Gameplay.Bingo
{
    /// <summary>
    /// Input parameters passed to <see cref="Core.ICardGenerator.Generate"/>.
    /// Generators use this context to produce an appropriate objective pool.
    /// </summary>
    public sealed class CardGenerationRequest
    {
        /// <summary>Which zone the run is taking place in.</summary>
        public string ZoneId { get; init; } = string.Empty;

        /// <summary>Player's character class.</summary>
        public ClassType PlayerClass { get; init; }

        /// <summary>Difficulty tier (0 = Normal … 3 = Chaos).</summary>
        public int DifficultyTier { get; init; }

        /// <summary>Player character level for objective scaling.</summary>
        public int CharacterLevel { get; init; }

        /// <summary>Board dimension. Defaults to 5 (standard 5×5).</summary>
        public int BoardSize { get; init; } = BingoCard.BoardSize;

        /// <summary>Seed for the random number generator (0 = generate fresh seed).</summary>
        public int Seed { get; init; }

        /// <summary>True when the run is a co-op session.</summary>
        public bool IsCoopSession { get; init; }

        /// <summary>True when the run is a PvP match.</summary>
        public bool IsPvpSession { get; init; }
    }
}
