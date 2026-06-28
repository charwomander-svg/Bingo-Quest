using System;
using System.Collections.Generic;

namespace BingoQuest.Gameplay.Bingo
{
    /// <summary>
    /// Detects completed Bingo patterns (rows, columns, diagonals, corners, full card).
    /// Handles pattern evaluation and change tracking.
    /// </summary>
    public class PatternDetector
    {
        private BingoCard card;
        private HashSet<BingoPattern> detectedPatterns = new();
        private HashSet<BingoPattern> previousPatterns = new();

        public IReadOnlyCollection<BingoPattern> DetectedPatterns => detectedPatterns;
        public IReadOnlyCollection<BingoPattern> NewPatterns { get; private set; } = new List<BingoPattern>();

        public PatternDetector(BingoCard card)
        {
            this.card = card;
        }

        public void Evaluate()
        {
            previousPatterns.Clear();
            previousPatterns.UnionWith(detectedPatterns);
            detectedPatterns.Clear();

            // Check all possible patterns
            CheckRows();
            CheckColumns();
            CheckDiagonals();
            CheckCorners();
            CheckFullCard();

            // Calculate newly completed patterns
            var newPatterns = new List<BingoPattern>(detectedPatterns);
            newPatterns.RemoveAll(p => previousPatterns.Contains(p));
            NewPatterns = newPatterns;
        }

        private void CheckRows()
        {
            for (int row = 0; row < BingoCard.GRID_SIZE; row++)
            {
                bool rowComplete = true;
                for (int col = 0; col < BingoCard.GRID_SIZE; col++)
                {
                    if (!card.GetSquare(row, col).IsCompleted)
                    {
                        rowComplete = false;
                        break;
                    }
                }
                if (rowComplete)
                    detectedPatterns.Add(BingoPattern.Row);
            }
        }

        private void CheckColumns()
        {
            for (int col = 0; col < BingoCard.GRID_SIZE; col++)
            {
                bool colComplete = true;
                for (int row = 0; row < BingoCard.GRID_SIZE; row++)
                {
                    if (!card.GetSquare(row, col).IsCompleted)
                    {
                        colComplete = false;
                        break;
                    }
                }
                if (colComplete)
                    detectedPatterns.Add(BingoPattern.Column);
            }
        }

        private void CheckDiagonals()
        {
            // Top-left to bottom-right
            bool diagLeftComplete = true;
            for (int i = 0; i < BingoCard.GRID_SIZE; i++)
            {
                if (!card.GetSquare(i, i).IsCompleted)
                {
                    diagLeftComplete = false;
                    break;
                }
            }
            if (diagLeftComplete)
                detectedPatterns.Add(BingoPattern.DiagonalLeft);

            // Top-right to bottom-left
            bool diagRightComplete = true;
            for (int i = 0; i < BingoCard.GRID_SIZE; i++)
            {
                if (!card.GetSquare(i, BingoCard.GRID_SIZE - 1 - i).IsCompleted)
                {
                    diagRightComplete = false;
                    break;
                }
            }
            if (diagRightComplete)
                detectedPatterns.Add(BingoPattern.DiagonalRight);
        }

        private void CheckCorners()
        {
            int last = BingoCard.GRID_SIZE - 1;
            bool allCornersComplete = 
                card.GetSquare(0, 0).IsCompleted &&
                card.GetSquare(0, last).IsCompleted &&
                card.GetSquare(last, 0).IsCompleted &&
                card.GetSquare(last, last).IsCompleted;

            if (allCornersComplete)
                detectedPatterns.Add(BingoPattern.Corners);
        }

        private void CheckFullCard()
        {
            if (card.CompletedSquareCount == BingoCard.GRID_SIZE * BingoCard.GRID_SIZE)
                detectedPatterns.Add(BingoPattern.FullCard);
        }
    }
}
