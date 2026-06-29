using System;
using BingoQuest.Gameplay.Regions;
using NUnit.Framework;

namespace BingoQuest.Tests.EditMode
{
    public sealed class WorldSystemsTests
    {
        private WorldMapService _world;

        [SetUp]
        public void SetUp()
        {
            _world = new WorldMapService();
            _world.AddRegion(new RegionDefinition { RegionId = "a", DisplayName = "A", BaseCardDifficulty = 1 });
            _world.AddRegion(new RegionDefinition { RegionId = "b", DisplayName = "B", BaseCardDifficulty = 2 });
            _world.AddRegion(new RegionDefinition { RegionId = "c", DisplayName = "C", BaseCardDifficulty = 3 });
            _world.ConnectBidirectional("a", "b");
            _world.ConnectBidirectional("b", "c");
        }

        [Test]
        public void LockedRegion_CannotBeEntered()
        {
            Assert.IsFalse(_world.TryEnterRegion("a", out _));
        }

        [Test]
        public void UnlockedStartingRegion_CanBeEntered()
        {
            _world.UnlockRegion("a");
            Assert.IsTrue(_world.TryEnterRegion("a", out var entered));
            Assert.AreEqual("a", entered.RegionId);
            Assert.AreEqual("a", _world.CurrentRegionId);
        }

        [Test]
        public void TravelRequiresConnection()
        {
            _world.UnlockRegion("a");
            _world.UnlockRegion("c");
            _world.TryEnterRegion("a", out _);

            Assert.IsFalse(_world.TryEnterRegion("c", out _), "A cannot travel directly to C without passing through B.");
        }

        [Test]
        public void BossCompletion_UnlocksConnectedRegions()
        {
            _world.UnlockRegion("a");
            _world.TryEnterRegion("a", out _);
            _world.CompleteCurrentRegionBoss();

            Assert.IsTrue(_world.IsRegionUnlocked("b"));
        }

        [Test]
        public void ObjectiveContext_UsesCurrentRegionAndDifficulty()
        {
            _world.UnlockRegion("a");
            _world.TryEnterRegion("a", out _);
            var ctx = _world.BuildObjectiveContext("warrior", 100, cardDifficultyBonus: 2);

            Assert.AreEqual("a", ctx.ZoneId);
            Assert.AreEqual(3, ctx.CardDifficulty);
            Assert.AreEqual("warrior", ctx.PlayerClassId);
        }

        [Test]
        public void ObjectiveContext_SeedChangesPerVisit()
        {
            _world.UnlockRegion("a");
            _world.UnlockRegion("b");
            _world.TryEnterRegion("a", out _);
            int firstSeed = _world.BuildObjectiveContext("mage", 200).RunSeed;

            _world.TryEnterRegion("b", out _);
            _world.TryEnterRegion("a", out _);
            int secondSeed = _world.BuildObjectiveContext("mage", 200).RunSeed;

            Assert.AreNotEqual(firstSeed, secondSeed);
        }

        [Test]
        public void DuplicateRegionId_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _world.AddRegion(new RegionDefinition { RegionId = "a", DisplayName = "Duplicate" }));
        }
    }
}
