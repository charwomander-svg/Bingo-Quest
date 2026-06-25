using System.Collections.Generic;
using BingoQuest.Core;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Objectives;
using NUnit.Framework;

namespace BingoQuest.Tests.EditMode
{
    /// <summary>NUnit EditMode tests for <see cref="PatternEvaluator"/>.</summary>
    [TestFixture]
    public class PatternEvaluatorTests
    {
        /// <summary>Builds a blank card where all squares need 1 kill except the FREE centre.</summary>
        private static BingoCard BuildBlankCard()
        {
            var squares = new BingoSquare[BingoCard.TotalSquares];
            for (int i = 0; i < BingoCard.TotalSquares; i++)
            {
                bool isFree = i == BingoCard.FreeCentreIndex;
                IObjective obj = isFree
                    ? new FreeObjective()
                    : new KillCountObjective($"obj_{i}", 1);
                squares[i] = new BingoSquare(i, obj, isFree);
            }
            return new BingoCard("eval_test", "zone", squares);
        }

        private static void MarkSquare(BingoCard card, int index) =>
            card.GetSquare(index).Mark();

        private static void MarkRow(BingoCard card, int row)
        {
            for (int c = 0; c < BingoCard.BoardSize; c++)
                card.GetSquare(row, c).Mark();
        }

        private static void MarkColumn(BingoCard card, int col)
        {
            for (int r = 0; r < BingoCard.BoardSize; r++)
                card.GetSquare(r, col).Mark();
        }

        [Test]
        public void EmptyCard_NoCompletedPatterns()
        {
            var card = BuildBlankCard();
            var patterns = PatternEvaluator.GetCompletedPatterns(card);
            // Free centre exists but a single square is not a pattern
            CollectionAssert.DoesNotContain(patterns, PatternType.Row);
            CollectionAssert.DoesNotContain(patterns, PatternType.Column);
        }

        [Test]
        public void Row0Complete_DetectsRowPattern()
        {
            var card = BuildBlankCard();
            MarkRow(card, 0);
            var patterns = PatternEvaluator.GetCompletedPatterns(card);
            CollectionAssert.Contains(patterns, PatternType.Row);
        }

        [Test]
        public void Column3Complete_DetectsColumnPattern()
        {
            var card = BuildBlankCard();
            MarkColumn(card, 3);
            var patterns = PatternEvaluator.GetCompletedPatterns(card);
            CollectionAssert.Contains(patterns, PatternType.Column);
        }

        [Test]
        public void MainDiagonalComplete_DetectsDiagonalMain()
        {
            var card = BuildBlankCard();
            for (int i = 0; i < BingoCard.BoardSize; i++)
                card.GetSquare(i, i).Mark();
            var patterns = PatternEvaluator.GetCompletedPatterns(card);
            CollectionAssert.Contains(patterns, PatternType.DiagonalMain);
        }

        [Test]
        public void AntiDiagonalComplete_DetectsDiagonalAnti()
        {
            var card = BuildBlankCard();
            int n = BingoCard.BoardSize;
            for (int i = 0; i < n; i++)
                card.GetSquare(i, n - 1 - i).Mark();
            var patterns = PatternEvaluator.GetCompletedPatterns(card);
            CollectionAssert.Contains(patterns, PatternType.DiagonalAnti);
        }

        [Test]
        public void FourCornersMarked_DetectsFourCornersPattern()
        {
            var card = BuildBlankCard();
            int n = BingoCard.BoardSize;
            card.GetSquare(0, 0).Mark();
            card.GetSquare(0, n - 1).Mark();
            card.GetSquare(n - 1, 0).Mark();
            card.GetSquare(n - 1, n - 1).Mark();
            var patterns = PatternEvaluator.GetCompletedPatterns(card);
            CollectionAssert.Contains(patterns, PatternType.FourCorners);
        }

        [Test]
        public void AllSquaresMarked_DetectsFullCardPattern()
        {
            var card = BuildBlankCard();
            for (int i = 0; i < BingoCard.TotalSquares; i++)
                card.GetSquare(i).Mark();
            var patterns = PatternEvaluator.GetCompletedPatterns(card);
            CollectionAssert.Contains(patterns, PatternType.FullCard);
        }

        [Test]
        public void EvaluateNewPatterns_DoesNotReturnAlreadyKnownPatterns()
        {
            var card = BuildBlankCard();
            MarkRow(card, 0);
            var known = new HashSet<PatternType> { PatternType.Row };
            var newPatterns = PatternEvaluator.EvaluateNewPatterns(card, known);
            CollectionAssert.DoesNotContain(newPatterns, PatternType.Row,
                "Already-known patterns should not be returned again.");
        }

        [Test]
        public void IsPatternComplete_IncompleteRow_ReturnsFalse()
        {
            var card = BuildBlankCard();
            // Mark only 4 of 5 in row 1
            for (int c = 0; c < BingoCard.BoardSize - 1; c++)
                card.GetSquare(1, c).Mark();
            Assert.IsFalse(PatternEvaluator.IsPatternComplete(card, PatternType.Row),
                "An incomplete row should not be reported as complete.");
        }
    }
}
