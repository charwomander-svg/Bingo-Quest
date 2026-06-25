namespace BingoQuest.Core
{
    /// <summary>
    /// Data passed to an objective on initialisation, providing zone/difficulty/class context
    /// so the objective can scale or adjust requirements appropriately.
    /// </summary>
    public sealed class ObjectiveContext
    {
        /// <summary>Unique identifier of the current region/zone.</summary>
        public string ZoneId { get; init; } = string.Empty;

        /// <summary>Current difficulty tier (0 = Normal … 3 = Chaos).</summary>
        public int DifficultyTier { get; init; }

        /// <summary>Class identifier of the player character.</summary>
        public string ClassId { get; init; } = string.Empty;

        /// <summary>Character level for requirement scaling.</summary>
        public int CharacterLevel { get; init; }

        /// <summary>True when the session is a co-op run.</summary>
        public bool IsCoopSession { get; init; }

        /// <summary>True when the session is a PvP match.</summary>
        public bool IsPvpSession { get; init; }
    }
}
