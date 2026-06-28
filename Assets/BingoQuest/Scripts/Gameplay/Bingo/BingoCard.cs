using UnityEngine;

namespace BingoQuest.Gameplay.Bingo
{
    /// <summary>
    /// Represents a single square on the Bingo card.
    /// Tracks completion and references the underlying objective.
    /// </summary>
    public struct BingoSquare
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string ObjectiveId { get; set; }
        public bool IsCompleted { get; set; }

        public BingoSquare(int row, int col, string objectiveId)
        {
            Row = row;
            Column = col;
            ObjectiveId = objectiveId;
            IsCompleted = false;
        }

        public override string ToString() => $"Square({Row},{Column}) - {ObjectiveId}: {(IsCompleted ? "✓" : "○")}";
    }

    /// <summary>
    /// Represents a completed line pattern on the card.
    /// </summary>
    public enum BingoPattern
    {
        None = 0,
        Row = 1,
        Column = 2,
        DiagonalLeft = 4,     // Top-left to bottom-right
        DiagonalRight = 8,    // Top-right to bottom-left
        Corners = 16,         // All four corner squares
        FullCard = 32,        // All squares completed
        Any = Row | Column | DiagonalLeft | DiagonalRight | Corners | FullCard
    }

    /// <summary>
    /// Core Bingo card data structure (5x5 grid for standard Bingo).
    /// Manages square state and pattern detection.
    /// </summary>
    public class BingoCard
    {
        public const int GRID_SIZE = 5;
        public BingoSquare[] Squares { get; private set; }
        public int CompletedSquareCount { get; private set; }

        public BingoCard()
        {
            Squares = new BingoSquare[GRID_SIZE * GRID_SIZE];
            CompletedSquareCount = 0;
        }

        public BingoSquare GetSquare(int row, int col) => Squares[row * GRID_SIZE + col];

        public void SetSquare(int row, int col, BingoSquare square)
        {
            Squares[row * GRID_SIZE + col] = square;
        }

        public void CompleteSquare(int row, int col)
        {
            if (row < 0 || row >= GRID_SIZE || col < 0 || col >= GRID_SIZE)
                return;

            var square = GetSquare(row, col);
            if (!square.IsCompleted)
            {
                square.IsCompleted = true;
                SetSquare(row, col, square);
                CompletedSquareCount++;
            }
        }

        public void Reset()
        {
            for (int i = 0; i < Squares.Length; i++)
            {
                var sq = Squares[i];
                sq.IsCompleted = false;
                Squares[i] = sq;
            }
            CompletedSquareCount = 0;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Bingo Card ({CompletedSquareCount}/{GRID_SIZE * GRID_SIZE} completed)");
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    var sq = GetSquare(row, col);
                    sb.Append(sq.IsCompleted ? "[✓] " : "[ ] ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
