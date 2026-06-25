using System.Collections.Generic;
using BingoQuest.Core;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Objectives;
using NUnit.Framework;

namespace BingoQuest.Tests.EditMode
{
    /// <summary>NUnit EditMode tests for <see cref="BingoCard"/>.</summary>
    [TestFixture]
    public class BingoCardTests
    {
        private static BingoCard BuildCard()
        {
            var squares = new BingoSquare[BingoCard.TotalSquares];
            for (int i = 0; i < BingoCard.TotalSquares; i++)
            {
                bool isFree = i == BingoCard.FreeCentreIndex;
                var objective = isFree
                    ? (IObjective)new FreeObjective()
                    : new KillCountObjective($"kill_{i}", 1);
                squares[i] = new BingoSquare(i, objective, isFree);
            }
            return new BingoCard("test_card", "zone_forest", squares);
        }

        [Test]
        public void NewCard_CentreSquareIsMarked()
        {
            var card = BuildCard();
            Assert.IsTrue(card.GetSquare(BingoCard.FreeCentreIndex).IsMarked,
                "Centre FREE square should start marked.");
        }

        [Test]
        public void NewCard_MarkedCount_EqualsOne()
        {
            var card = BuildCard();
            Assert.AreEqual(1, card.MarkedCount, "Only the FREE square should be marked initially.");
        }

        [Test]
        public void ProcessEvent_KillEvent_MarksMatchingSquare()
        {
            var card = BuildCard();
            int squaresToMark = 0;
            card.OnSquareMarked += (_, _) => squaresToMark++;

            card.ProcessEvent(ObjectiveEvent.Kill("enemy_goblin"));

            // At least one non-FREE square should now be marked
            Assert.Greater(squaresToMark, 0, "A kill event should mark at least one square.");
        }

        [Test]
        public void ProcessEvent_BroadcastsToAllUnmarkedSquares()
        {
            var card = BuildCard();
            // Fire 24 kill events (enough to mark all non-FREE squares)
            for (int i = 0; i < BingoCard.TotalSquares - 1; i++)
                card.ProcessEvent(ObjectiveEvent.Kill("enemy_goblin"));

            Assert.AreEqual(BingoCard.TotalSquares, card.MarkedCount, "All squares should be marked.");
        }

        [Test]
        public void FullCard_RaisesOnFullCardEvent()
        {
            var card = BuildCard();
            bool fullCardFired = false;
            card.OnFullCard += _ => fullCardFired = true;

            for (int i = 0; i < BingoCard.TotalSquares - 1; i++)
                card.ProcessEvent(ObjectiveEvent.Kill("enemy_goblin"));

            Assert.IsTrue(fullCardFired, "OnFullCard should fire when all squares are marked.");
        }

        [Test]
        public void Reset_ClearsAllMarks_ExceptFree()
        {
            var card = BuildCard();
            for (int i = 0; i < BingoCard.TotalSquares - 1; i++)
                card.ProcessEvent(ObjectiveEvent.Kill("enemy_goblin"));

            card.Reset();

            Assert.AreEqual(1, card.MarkedCount, "After reset only the FREE square should be marked.");
        }

        [Test]
        public void GetSquare_RowCol_ReturnsCorrectSquare()
        {
            var card = BuildCard();
            var sq = card.GetSquare(2, 2);
            Assert.AreEqual(BingoCard.FreeCentreIndex, sq.Index, "Row 2, Col 2 should be the centre square.");
        }
    }
}
