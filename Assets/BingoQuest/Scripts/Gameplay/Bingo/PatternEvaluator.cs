using System.Collections.Generic;

namespace BingoQuest.Gameplay.Bingo
{
    /// <summary>
    /// Stateless utility that detects completed Bingo patterns on a <see cref="BingoCard"/>.
    /// All methods are static to allow use without allocating an instance.
    /// </summary>
    public static class PatternEvaluator
    {
        private const int N = BingoCard.BoardSize; // 5

        // Pre-computed square indices for each named pattern.
        private static readonly int[][] RowIndices = BuildRows();
        private static readonly int[][] ColumnIndices = BuildColumns();
        private static readonly int[] MainDiagIndices = BuildMainDiagonal();
        private static readonly int[] AntiDiagIndices = BuildAntiDiagonal();
        private static readonly int[] FourCornersIndices = { 0, N - 1, N * (N - 1), N * N - 1 };
        private static readonly int[] CenterCrossIndices = BuildCenterCross();

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all patterns that are currently complete on <paramref name="card"/>.
        /// </summary>
        public static IReadOnlyList<PatternType> GetCompletedPatterns(BingoCard card)
        {
            var result = new List<PatternType>();
            CollectCompleted(card, result);
            return result;
        }

        /// <summary>
        /// Returns only the patterns that are now complete but were not yet in
        /// <paramref name="alreadyComplete"/>.  Used for incremental detection.
        /// </summary>
        public static IReadOnlyList<PatternType> EvaluateNewPatterns(
            BingoCard card,
            IReadOnlySet<PatternType> alreadyComplete)
        {
            var result = new List<PatternType>();
            foreach (var pattern in GetCompletedPatterns(card))
                if (!alreadyComplete.Contains(pattern))
                    result.Add(pattern);
            return result;
        }

        /// <summary>Returns true when the specific <paramref name="pattern"/> is complete.</summary>
        public static bool IsPatternComplete(BingoCard card, PatternType pattern) =>
            pattern switch
            {
                PatternType.FullCard      => card.IsFullCard,
                PatternType.FourCorners   => AreAllMarked(card, FourCornersIndices),
                PatternType.DiagonalMain  => AreAllMarked(card, MainDiagIndices),
                PatternType.DiagonalAnti  => AreAllMarked(card, AntiDiagIndices),
                PatternType.CenterCross   => AreAllMarked(card, CenterCrossIndices),
                PatternType.Row           => AnyRowComplete(card),
                PatternType.Column        => AnyColumnComplete(card),
                _ => false
            };

        /// <summary>Returns which row (0–4) is complete, or -1 if none.</summary>
        public static int GetCompletedRowIndex(BingoCard card)
        {
            for (int r = 0; r < N; r++)
                if (IsRowComplete(card, r)) return r;
            return -1;
        }

        /// <summary>Returns which column (0–4) is complete, or -1 if none.</summary>
        public static int GetCompletedColumnIndex(BingoCard card)
        {
            for (int c = 0; c < N; c++)
                if (IsColumnComplete(card, c)) return c;
            return -1;
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static void CollectCompleted(BingoCard card, List<PatternType> result)
        {
            for (int r = 0; r < N; r++)
                if (IsRowComplete(card, r)) { result.Add(PatternType.Row); break; }

            for (int c = 0; c < N; c++)
                if (IsColumnComplete(card, c)) { result.Add(PatternType.Column); break; }

            if (AreAllMarked(card, MainDiagIndices))  result.Add(PatternType.DiagonalMain);
            if (AreAllMarked(card, AntiDiagIndices))  result.Add(PatternType.DiagonalAnti);
            if (AreAllMarked(card, FourCornersIndices)) result.Add(PatternType.FourCorners);
            if (AreAllMarked(card, CenterCrossIndices)) result.Add(PatternType.CenterCross);
            if (card.IsFullCard) result.Add(PatternType.FullCard);
        }

        private static bool IsRowComplete(BingoCard card, int row)
        {
            for (int c = 0; c < N; c++)
                if (!card.GetSquare(row, c).IsMarked) return false;
            return true;
        }

        private static bool IsColumnComplete(BingoCard card, int col)
        {
            for (int r = 0; r < N; r++)
                if (!card.GetSquare(r, col).IsMarked) return false;
            return true;
        }

        private static bool AnyRowComplete(BingoCard card)
        {
            for (int r = 0; r < N; r++)
                if (IsRowComplete(card, r)) return true;
            return false;
        }

        private static bool AnyColumnComplete(BingoCard card)
        {
            for (int c = 0; c < N; c++)
                if (IsColumnComplete(card, c)) return true;
            return false;
        }

        private static bool AreAllMarked(BingoCard card, int[] indices)
        {
            foreach (int i in indices)
                if (!card.GetSquare(i).IsMarked) return false;
            return true;
        }

        // ── Index builders ────────────────────────────────────────────────────

        private static int[][] BuildRows()
        {
            var rows = new int[N][];
            for (int r = 0; r < N; r++)
            {
                rows[r] = new int[N];
                for (int c = 0; c < N; c++) rows[r][c] = r * N + c;
            }
            return rows;
        }

        private static int[][] BuildColumns()
        {
            var cols = new int[N][];
            for (int c = 0; c < N; c++)
            {
                cols[c] = new int[N];
                for (int r = 0; r < N; r++) cols[c][r] = r * N + c;
            }
            return cols;
        }

        private static int[] BuildMainDiagonal()
        {
            var d = new int[N];
            for (int i = 0; i < N; i++) d[i] = i * N + i;
            return d;
        }

        private static int[] BuildAntiDiagonal()
        {
            var d = new int[N];
            for (int i = 0; i < N; i++) d[i] = i * N + (N - 1 - i);
            return d;
        }

        private static int[] BuildCenterCross()
        {
            // Middle row + middle column (de-duplicated)
            var mid = N / 2;
            var result = new List<int>();
            for (int c = 0; c < N; c++) result.Add(mid * N + c);        // middle row
            for (int r = 0; r < N; r++) if (r != mid) result.Add(r * N + mid); // middle col sans centre
            return result.ToArray();
        }
    }
}
