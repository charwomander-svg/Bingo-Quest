namespace BingoQuest.Core
{
    /// <summary>
    /// Contract for a single Bingo-square objective.
    /// Implementations are pure C# and must not depend on Unity's main thread.
    /// </summary>
    public interface IObjective
    {
        /// <summary>Stable unique identifier used for serialisation and lookup.</summary>
        string ObjectiveId { get; }

        /// <summary>Display name shown in the card UI.</summary>
        string DisplayName { get; }

        /// <summary>Progress accumulated so far.</summary>
        int CurrentProgress { get; }

        /// <summary>Total progress required to complete this objective.</summary>
        int RequiredProgress { get; }

        /// <summary>True when <see cref="CurrentProgress"/> >= <see cref="RequiredProgress"/>.</summary>
        bool IsComplete { get; }

        /// <summary>Sets zone/difficulty/class context. Called once after construction.</summary>
        void Initialize(ObjectiveContext context);

        /// <summary>
        /// Attempts to apply an incoming gameplay event to progress.
        /// Implementations should return quickly and must be thread-safe if possible.
        /// </summary>
        void UpdateProgress(in ObjectiveEvent evt);

        /// <summary>Forcefully marks the objective complete (used by cheat/debug tools).</summary>
        void Complete();

        /// <summary>Resets progress to zero (used when regenerating a card mid-run).</summary>
        void Reset();
    }
}
