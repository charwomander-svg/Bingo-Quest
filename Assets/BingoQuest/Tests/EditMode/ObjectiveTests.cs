using BingoQuest.Core;
using BingoQuest.Gameplay.Objectives;
using NUnit.Framework;

namespace BingoQuest.Tests.EditMode
{
    /// <summary>NUnit EditMode tests for objective implementations.</summary>
    [TestFixture]
    public class ObjectiveTests
    {
        private static ObjectiveContext DefaultContext() => new()
        {
            ZoneId = "zone_forest",
            DifficultyTier = 0,
            ClassId = "Warrior",
            CharacterLevel = 1
        };

        // ── KillCountObjective ─────────────────────────────────────────────────

        [Test]
        public void KillCount_NotComplete_Initially()
        {
            var obj = new KillCountObjective("kill_5", 5);
            obj.Initialize(DefaultContext());
            Assert.IsFalse(obj.IsComplete);
            Assert.AreEqual(0, obj.CurrentProgress);
        }

        [Test]
        public void KillCount_Increments_OnKillEvent()
        {
            var obj = new KillCountObjective("kill_3", 3);
            obj.Initialize(DefaultContext());
            obj.UpdateProgress(ObjectiveEvent.Kill("goblin"));
            Assert.AreEqual(1, obj.CurrentProgress);
        }

        [Test]
        public void KillCount_Completes_WhenTargetReached()
        {
            var obj = new KillCountObjective("kill_2", 2);
            obj.Initialize(DefaultContext());
            obj.UpdateProgress(ObjectiveEvent.Kill("goblin"));
            obj.UpdateProgress(ObjectiveEvent.Kill("goblin"));
            Assert.IsTrue(obj.IsComplete);
        }

        [Test]
        public void KillCount_DoesNotExceedRequired()
        {
            var obj = new KillCountObjective("kill_1", 1);
            obj.Initialize(DefaultContext());
            obj.UpdateProgress(ObjectiveEvent.Kill("goblin"));
            obj.UpdateProgress(ObjectiveEvent.Kill("goblin")); // extra kill
            Assert.AreEqual(1, obj.CurrentProgress);
        }

        [Test]
        public void KillCount_Filter_IgnoresWrongEnemyType()
        {
            var obj = new KillCountObjective("kill_troll", 2, "Troll");
            obj.Initialize(DefaultContext());
            obj.UpdateProgress(ObjectiveEvent.Kill("Goblin_01")); // doesn't match
            Assert.AreEqual(0, obj.CurrentProgress);
        }

        [Test]
        public void KillCount_Filter_MatchesCorrectEnemyType()
        {
            var obj = new KillCountObjective("kill_troll", 2, "Troll");
            obj.Initialize(DefaultContext());
            obj.UpdateProgress(ObjectiveEvent.Kill("Troll_Cave_01")); // matches prefix
            Assert.AreEqual(1, obj.CurrentProgress);
        }

        // ── ElementalKillObjective ─────────────────────────────────────────────

        [Test]
        public void ElementalKill_CountsOnlyMatchingElement()
        {
            var obj = new ElementalKillObjective("fire_kill", 3, ElementType.Fire);
            obj.Initialize(DefaultContext());

            obj.UpdateProgress(ObjectiveEvent.Kill("enemy", ElementType.Ice));  // wrong
            obj.UpdateProgress(ObjectiveEvent.Kill("enemy", ElementType.Fire)); // correct
            obj.UpdateProgress(ObjectiveEvent.Kill("enemy", ElementType.Fire)); // correct

            Assert.AreEqual(2, obj.CurrentProgress);
        }

        // ── CritKillObjective ──────────────────────────────────────────────────

        [Test]
        public void CritKill_OnlyCountsCritEvents()
        {
            var obj = new CritKillObjective("crit_5", 5);
            obj.Initialize(DefaultContext());

            obj.UpdateProgress(ObjectiveEvent.Kill("enemy"));                              // not crit
            obj.UpdateProgress(ObjectiveEvent.Kill("enemy", isCritical: true));           // crit kill
            obj.UpdateProgress(ObjectiveEvent.Crit("player"));                             // crit hit

            Assert.AreEqual(2, obj.CurrentProgress);
        }

        // ── DodgeSuccessObjective ─────────────────────────────────────────────

        [Test]
        public void DodgeObjective_Increments_OnDodgeEvent()
        {
            var obj = new DodgeSuccessObjective("dodge_10", 10);
            obj.Initialize(DefaultContext());
            obj.UpdateProgress(ObjectiveEvent.Dodge());
            obj.UpdateProgress(ObjectiveEvent.Dodge());
            Assert.AreEqual(2, obj.CurrentProgress);
        }

        // ── StatusApplyObjective ──────────────────────────────────────────────

        [Test]
        public void StatusApply_Increments_OnStatusAppliedEvent()
        {
            var obj = new StatusApplyObjective("status_burn_3", 3, "Burn");
            obj.Initialize(DefaultContext());
            obj.UpdateProgress(ObjectiveEvent.StatusApplied("Burn", ElementType.Fire));
            obj.UpdateProgress(ObjectiveEvent.StatusApplied("Poison", ElementType.Poison)); // wrong
            Assert.AreEqual(1, obj.CurrentProgress);
        }

        // ── BossDefeatObjective ───────────────────────────────────────────────

        [Test]
        public void BossDefeat_CompletesOnBossKilled()
        {
            var obj = new BossDefeatObjective("defeat_troll_king", "TrollKing");
            obj.Initialize(DefaultContext());
            obj.UpdateProgress(ObjectiveEvent.BossDefeated("TrollKing"));
            Assert.IsTrue(obj.IsComplete);
        }

        [Test]
        public void BossDefeat_IgnoresWrongBoss()
        {
            var obj = new BossDefeatObjective("defeat_troll_king", "TrollKing");
            obj.Initialize(DefaultContext());
            obj.UpdateProgress(ObjectiveEvent.BossDefeated("GoblinLord"));
            Assert.IsFalse(obj.IsComplete);
        }

        // ── Reset ─────────────────────────────────────────────────────────────

        [Test]
        public void Reset_ClearsProgress()
        {
            var obj = new KillCountObjective("kill_5", 5);
            obj.Initialize(DefaultContext());
            obj.UpdateProgress(ObjectiveEvent.Kill("enemy"));
            obj.UpdateProgress(ObjectiveEvent.Kill("enemy"));
            obj.Reset();
            Assert.AreEqual(0, obj.CurrentProgress);
        }

        // ── ObjectiveFactory scaling ──────────────────────────────────────────

        [Test]
        public void Factory_ScalesRequiredCount_ForDifficulty2()
        {
            var def = new ObjectiveDefinition
            {
                ObjectiveId = "scaled_kill",
                Type = ObjectiveType.KillCount,
                BaseRequiredCount = 10
            };
            var context = new ObjectiveContext { DifficultyTier = 2 };
            var obj = ObjectiveFactory.Create(def, context);
            // DifficultyTier 2 → ×1.7 → round(17) = 17
            Assert.AreEqual(17, obj.RequiredProgress);
        }
    }
}
