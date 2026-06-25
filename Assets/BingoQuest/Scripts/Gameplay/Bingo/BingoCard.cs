using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BingoQuest.Core;

namespace BingoQuest.Gameplay.Bingo
{
    /// <summary>
    /// Central data model for a 5×5 Bingo board.
    /// Owns the collection of <see cref="BingoSquare"/> objects and raises
    /// events when squares or patterns are completed.
    /// Pure C# – no Unity dependency.
    /// </summary>
    public sealed class BingoCard
    {
        /// <summary>Standard board side length.</summary>
        public const int BoardSize = 5;

        /// <summary>Total number of squares (BoardSize²).</summary>
        public const int TotalSquares = BoardSize * BoardSize;

        /// <summary>Index of the centre FREE square.</summary>
        public const int FreeCentreIndex = TotalSquares / 2; // 12

        private readonly BingoSquare[] _squares;
        private readonly HashSet<PatternType> _completedPatterns = new();

        // ── Public surface ────────────────────────────────────────────────────

        /// <summary>Read-only view of all 25 squares.</summary>
        public ReadOnlyCollection<BingoSquare> Squares { get; }

        /// <summary>Unique identifier for this card (useful for save/load).</summary>
        public string CardId { get; }

        /// <summary>Zone this card was generated for.</summary>
        public string ZoneId { get; }

        /// <summary>Number of marked squares (including FREE).</summary>
        public int MarkedCount { get; private set; }

        /// <summary>True when all 25 squares are marked.</summary>
        public bool IsFullCard => MarkedCount == TotalSquares;

        /// <summary>Patterns that have been completed at least once this run.</summary>
        public IReadOnlySet<PatternType> CompletedPatterns => _completedPatterns;

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Raised every time a square becomes marked.</summary>
        public event Action<BingoCard, BingoSquare>? OnSquareMarked;

        /// <summary>Raised each time a new pattern is completed.</summary>
        public event Action<BingoCard, PatternType>? OnPatternCompleted;

        /// <summary>Raised once when all 25 squares are marked.</summary>
        public event Action<BingoCard>? OnFullCard;

        // ── Construction ──────────────────────────────────────────────────────

        public BingoCard(string cardId, string zoneId, BingoSquare[] squares)
        {
            if (squares is null) throw new ArgumentNullException(nameof(squares));
            if (squares.Length != TotalSquares)
                throw new ArgumentException($"Expected {TotalSquares} squares, got {squares.Length}.", nameof(squares));

            CardId = cardId ?? throw new ArgumentNullException(nameof(cardId));
            ZoneId = zoneId ?? throw new ArgumentNullException(nameof(zoneId));
            _squares = squares;
            Squares = new ReadOnlyCollection<BingoSquare>(_squares);

            // Subscribe to each square's mark event
            foreach (var square in _squares)
            {
                if (square.IsMarked) MarkedCount++; // count FREE
                square.OnMarked += HandleSquareMarked;
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Returns the square at the given row and column (both 0-based).</summary>
        public BingoSquare GetSquare(int row, int col)
        {
            if ((uint)row >= BoardSize) throw new ArgumentOutOfRangeException(nameof(row));
            if ((uint)col >= BoardSize) throw new ArgumentOutOfRangeException(nameof(col));
            return _squares[row * BoardSize + col];
        }

        /// <summary>Returns the square at the given linear index (0–24).</summary>
        public BingoSquare GetSquare(int index)
        {
            if ((uint)index >= TotalSquares) throw new ArgumentOutOfRangeException(nameof(index));
            return _squares[index];
        }

        /// <summary>
        /// Broadcasts an objective event to every unmarked square on the card.
        /// </summary>
        public void ProcessEvent(in ObjectiveEvent evt)
        {
            foreach (var square in _squares)
                square.ProcessEvent(evt);
        }

        /// <summary>Resets the card to its initial state (all marks cleared except FREE).</summary>
        public void Reset()
        {
            _completedPatterns.Clear();
            foreach (var square in _squares)
                square.Reset();
            // Recount (FREE squares re-mark themselves on Reset)
            MarkedCount = 0;
            foreach (var square in _squares)
                if (square.IsMarked) MarkedCount++;
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void HandleSquareMarked(BingoSquare square)
        {
            MarkedCount++;
            OnSquareMarked?.Invoke(this, square);

            // Ask the evaluator which new patterns (if any) this mark completed
            var newPatterns = PatternEvaluator.EvaluateNewPatterns(this, _completedPatterns);
            foreach (var pattern in newPatterns)
            {
                _completedPatterns.Add(pattern);
                OnPatternCompleted?.Invoke(this, pattern);
            }

            if (IsFullCard)
                OnFullCard?.Invoke(this);
        }
    }
}
