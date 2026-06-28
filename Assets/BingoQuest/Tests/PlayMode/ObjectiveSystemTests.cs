using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Objectives;
using NUnit.Framework;
using UnityEngine;

namespace BingoQuest.Tests.PlayMode
{
    public class ObjectiveSystemTests
    {
        [Test]
        public void KillEnemiesObjective_ProgressesOnKillEvent()
        {
            var objective = new KillEnemiesObjective("test_kill", 5);
            var context = new ObjectiveContext { ZoneId = "forest", PlayerClassId = "warrior" };
            objective.Initialize(context);

            for (int i = 0; i < 5; i++)
            {
                objective.UpdateProgress(ObjectiveEvent.Kill($"enemy_{i}"));
            }

            Assert.IsTrue(objective.IsComplete);
            Assert.AreEqual(5, objective.CurrentProgress);
        }

        [Test]
        public void DealDamageObjective_AccumulatesDamage()
        {
            var objective = new DealDamageObjective("test_dmg", 300);
            var context = new ObjectiveContext { ZoneId = "mines" };
            objective.Initialize(context);

            objective.UpdateProgress(ObjectiveEvent.Damage("player", 100));
            objective.UpdateProgress(ObjectiveEvent.Damage("player", 150));

            Assert.AreEqual(250, objective.CurrentProgress);
            Assert.IsFalse(objective.IsComplete);

            objective.UpdateProgress(ObjectiveEvent.Damage("player", 50));
            Assert.IsTrue(objective.IsComplete);
        }

        [Test]
        public void CriticalHitsObjective_OnlyCountsCrits()
        {
            var objective = new CriticalHitsObjective("test_crit", 3);
            var context = new ObjectiveContext { };
            objective.Initialize(context);

            objective.UpdateProgress(ObjectiveEvent.Damage("player", 50)); // No effect
            Assert.AreEqual(0, objective.CurrentProgress);

            objective.UpdateProgress(ObjectiveEvent.CriticalHit("player", 100));
            objective.UpdateProgress(ObjectiveEvent.CriticalHit("player", 120));
            objective.UpdateProgress(ObjectiveEvent.CriticalHit("player", 90));

            Assert.IsTrue(objective.IsComplete);
        }

        [Test]
        public void StatusEffectObjective_FiltersByElement()
        {
            var objective = new ApplyStatusEffectsObjective("test_burn", 2, ElementType.Fire);
            var context = new ObjectiveContext { };
            objective.Initialize(context);

            objective.UpdateProgress(ObjectiveEvent.StatusEffectApplied("freeze", ElementType.Frost)); // No match
            Assert.AreEqual(0, objective.CurrentProgress);

            objective.UpdateProgress(ObjectiveEvent.StatusEffectApplied("burn", ElementType.Fire));
            objective.UpdateProgress(ObjectiveEvent.StatusEffectApplied("burn2", ElementType.Fire));

            Assert.IsTrue(objective.IsComplete);
        }
    }

    public class BingoCardTests
    {
        [Test]
        public void BingoCard_CompletesSquares()
        {
            var card = new BingoCard();
            card.SetSquare(0, 0, new BingoSquare(0, 0, "obj1"));

            card.CompleteSquare(0, 0);
            Assert.IsTrue(card.GetSquare(0, 0).IsCompleted);
            Assert.AreEqual(1, card.CompletedSquareCount);
        }

        [Test]
        public void BingoCard_RespectsBounds()
        {
            var card = new BingoCard();
            card.CompleteSquare(-1, 0); // Should not crash
            card.CompleteSquare(5, 5);  // Should not crash
            Assert.AreEqual(0, card.CompletedSquareCount);
        }

        [Test]
        public void BingoCard_CanReset()
        {
            var card = new BingoCard();
            card.CompleteSquare(0, 0);
            card.CompleteSquare(1, 1);
            Assert.AreEqual(2, card.CompletedSquareCount);

            card.Reset();
            Assert.AreEqual(0, card.CompletedSquareCount);
            Assert.IsFalse(card.GetSquare(0, 0).IsCompleted);
        }
    }

    public class PatternDetectorTests
    {
        [Test]
        public void PatternDetector_DetectsRowCompletion()
        {
            var card = new BingoCard();
            var detector = new PatternDetector(card);

            // Complete entire first row
            for (int col = 0; col < BingoCard.GRID_SIZE; col++)
            {
                card.SetSquare(0, col, new BingoSquare(0, col, $"obj_{col}"));
                card.CompleteSquare(0, col);
            }

            detector.Evaluate();
            Assert.IsTrue(detector.DetectedPatterns.Contains(BingoPattern.Row));
        }

        [Test]
        public void PatternDetector_DetectsColumnCompletion()
        {
            var card = new BingoCard();
            var detector = new PatternDetector(card);

            // Complete entire first column
            for (int row = 0; row < BingoCard.GRID_SIZE; row++)
            {
                card.SetSquare(row, 0, new BingoSquare(row, 0, $"obj_{row}"));
                card.CompleteSquare(row, 0);
            }

            detector.Evaluate();
            Assert.IsTrue(detector.DetectedPatterns.Contains(BingoPattern.Column));
        }

        [Test]
        public void PatternDetector_DetectsDiagonalCompletion()
        {
            var card = new BingoCard();
            var detector = new PatternDetector(card);

            // Complete diagonal (top-left to bottom-right)
            for (int i = 0; i < BingoCard.GRID_SIZE; i++)
            {
                card.SetSquare(i, i, new BingoSquare(i, i, $"obj_{i}"));
                card.CompleteSquare(i, i);
            }

            detector.Evaluate();
            Assert.IsTrue(detector.DetectedPatterns.Contains(BingoPattern.DiagonalLeft));
        }

        [Test]
        public void PatternDetector_DetectsCornerCompletion()
        {
            var card = new BingoCard();
            var detector = new PatternDetector(card);
            int last = BingoCard.GRID_SIZE - 1;

            card.CompleteSquare(0, 0);
            card.CompleteSquare(0, last);
            card.CompleteSquare(last, 0);
            card.CompleteSquare(last, last);

            detector.Evaluate();
            Assert.IsTrue(detector.DetectedPatterns.Contains(BingoPattern.Corners));
        }

        [Test]
        public void PatternDetector_DetectsFullCard()
        {
            var card = new BingoCard();
            var detector = new PatternDetector(card);

            // Complete all 25 squares
            for (int row = 0; row < BingoCard.GRID_SIZE; row++)
            {
                for (int col = 0; col < BingoCard.GRID_SIZE; col++)
                {
                    card.CompleteSquare(row, col);
                }
            }

            detector.Evaluate();
            Assert.IsTrue(detector.DetectedPatterns.Contains(BingoPattern.FullCard));
        }

        [Test]
        public void PatternDetector_TracksNewPatterns()
        {
            var card = new BingoCard();
            var detector = new PatternDetector(card);

            // First evaluation (no patterns)
            detector.Evaluate();
            Assert.AreEqual(0, detector.NewPatterns.Count);

            // Complete a row
            for (int col = 0; col < BingoCard.GRID_SIZE; col++)
                card.CompleteSquare(0, col);

            // Second evaluation (new row pattern)
            detector.Evaluate();
            Assert.AreEqual(1, detector.NewPatterns.Count);
            Assert.IsTrue(detector.NewPatterns.Contains(BingoPattern.Row));

            // Third evaluation (no new patterns)
            detector.Evaluate();
            Assert.AreEqual(0, detector.NewPatterns.Count);
        }
    }
}
