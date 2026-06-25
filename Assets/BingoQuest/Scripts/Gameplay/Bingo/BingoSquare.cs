using System;
using BingoQuest.Core;

namespace BingoQuest.Gameplay.Bingo
{
    /// <summary>
    /// Represents a single cell on the 5×5 Bingo board.
    /// Wraps an <see cref="IObjective"/> and tracks completion state.
    /// </summary>
    public sealed class BingoSquare
    {
        private readonly IObjective _objective;

        /// <summary>0-based index on the card (0 = top-left, 24 = bottom-right).</summary>
        public int Index { get; }

        /// <summary>Row index (0–4).</summary>
        public int Row => Index / BingoCard.BoardSize;

        /// <summary>Column index (0–4).</summary>
        public int Column => Index % BingoCard.BoardSize;

        /// <summary>The objective that must be fulfilled to mark this square.</summary>
        public IObjective Objective => _objective;

        /// <summary>True when the objective is satisfied (or the square is FREE).</summary>
        public bool IsMarked { get; private set; }

        /// <summary>True when this is the centre FREE square.</summary>
        public bool IsFree { get; }

        /// <summary>Raised after the square becomes marked.</summary>
        public event Action<BingoSquare>? OnMarked;

        public BingoSquare(int index, IObjective objective, bool isFree = false)
        {
            Index = index;
            _objective = objective ?? throw new ArgumentNullException(nameof(objective));
            IsFree = isFree;
            IsMarked = isFree; // FREE squares start marked
        }

        /// <summary>
        /// Feeds a gameplay event into the underlying objective and marks the
        /// square if it is now complete.
        /// </summary>
        public void ProcessEvent(in ObjectiveEvent evt)
        {
            if (IsMarked) return;

            _objective.UpdateProgress(evt);

            if (_objective.IsComplete)
                Mark();
        }

        /// <summary>Forcibly marks the square (used by debug tools or card-complete checks).</summary>
        public void Mark()
        {
            if (IsMarked) return;
            if (!IsFree) _objective.Complete();
            IsMarked = true;
            OnMarked?.Invoke(this);
        }

        /// <summary>Resets the square and its objective to the initial state.</summary>
        public void Reset()
        {
            if (IsFree) return;
            _objective.Reset();
            IsMarked = false;
        }

        public override string ToString()
            => $"[{Row},{Column}] {(IsFree ? "FREE" : _objective.DisplayName)} – {(IsMarked ? "✓" : $"{_objective.CurrentProgress}/{_objective.RequiredProgress}")}";
    }
}
