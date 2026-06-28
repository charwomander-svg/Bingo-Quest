using UnityEngine;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Core contract for all objective implementations in Bingo Quest.
    /// Objectives are triggered by combat events and track completion progress.
    /// </summary>
    public interface IObjective
    {
        string ObjectiveId { get; }
        int CurrentProgress { get; }
        int RequiredProgress { get; }
        bool IsComplete { get; }
        ObjectiveCategory Category { get; }

        /// <summary>Called when objective is placed on the Bingo card.</summary>
        void Initialize(ObjectiveContext context);

        /// <summary>Called by the objective system when a relevant combat event occurs.</summary>
        void UpdateProgress(in ObjectiveEvent evt);

        /// <summary>Called when objective reaches RequiredProgress. Triggers reward system.</summary>
        void Complete();

        /// <summary>Resets objective to starting state for new runs.</summary>
        void Reset();
    }

    public enum ObjectiveCategory
    {
        Combat,
        Survive,
        Loot,
        Explore,
        Social
    }

    /// <summary>Context passed to objectives on initialization.</summary>
    public struct ObjectiveContext
    {
        public string ZoneId;
        public int CardDifficulty;
        public string PlayerClassId;
        public int RunSeed;
    }
}
