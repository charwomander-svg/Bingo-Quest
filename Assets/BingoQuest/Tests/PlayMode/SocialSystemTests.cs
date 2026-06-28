using System.Collections.Generic;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Objectives;
using BingoQuest.Gameplay.Social;
using NUnit.Framework;

namespace BingoQuest.Tests.PlayMode
{
    public class SocialSystemTests
    {
        [Test]
        public void GhostBoardService_ReturnsFastestRunFirst()
        {
            var service = new GhostBoardService();
            service.SubmitRun(new GhostRunRecord { PlayerId = "p1", ClassId = "warrior", RegionId = "forest", CardSeed = 11, CompletionSeconds = 140 });
            service.SubmitRun(new GhostRunRecord { PlayerId = "p2", ClassId = "mage", RegionId = "forest", CardSeed = 11, CompletionSeconds = 122 });

            var best = service.GetBestGhost("forest", 11);
            Assert.IsNotNull(best);
            Assert.AreEqual("p2", best.PlayerId);
        }

        [Test]
        public void GuildBoardService_UnlocksFirstLineMilestone()
        {
            var service = new GuildBoardService();
            var board = service.CreateBoard("guild-alpha", size: 5);

            for (int col = 0; col < 5; col++)
                service.ContributeSquare(board, "member-1", 0, col);

            int lines = service.CountCompletedLines(board);
            Assert.AreEqual(1, lines);
            Assert.IsTrue(board.Milestones[0].IsUnlocked);
        }

        [Test]
        public void PvpBingoWarService_FirstPatternWinsMatch()
        {
            var service = new PvpBingoWarService();
            var objectives = new List<string>();
            for (int i = 0; i < 25; i++)
                objectives.Add($"obj-{i}");

            var match = service.CreateMatch("p1", "p2", objectives, 5);
            for (int i = 0; i < 5; i++)
                service.CompleteObjective(match, "p1", $"obj-{i}");

            Assert.AreEqual("p1", match.WinnerPlayerId);
            Assert.AreEqual(BingoPattern.Row, match.WinningPattern);
        }

        [Test]
        public void CoopSynergyService_TracksRevivesAndCombos()
        {
            var service = new CoopSynergyService();
            service.RecordRevive("p1");
            service.RecordRevive("p1");
            service.RecordCombo("team-a");

            Assert.AreEqual(2, service.GetPlayerRevives("p1"));
            Assert.AreEqual(1, service.GetTeamComboCount("team-a"));
        }

        [Test]
        public void CoopSynergyService_CreatesCoopObjectiveEvent()
        {
            var service = new CoopSynergyService();
            var evt = service.CreateCoopObjectiveEvent(ObjectiveEventType.AbilityUsed, "combo", 3);

            Assert.AreEqual(ObjectiveEventType.AbilityUsed, evt.Type);
            Assert.AreEqual("combo", evt.SourceId);
            Assert.IsTrue(evt.InCoop);
            Assert.IsFalse(evt.InPvp);
            Assert.AreEqual(3, evt.Amount);
        }
    }
}
