using System.Collections.Generic;
using BingoQuest.Core;
using BingoQuest.Gameplay.Combat;
using NUnit.Framework;

namespace BingoQuest.Tests.EditMode
{
    /// <summary>NUnit EditMode tests for <see cref="CombatEventBus"/>.</summary>
    [TestFixture]
    public class CombatEventBusTests
    {
        [Test]
        public void Publish_RaisesCombatEvent()
        {
            var bus = new CombatEventBus();
            CombatEvent? received = null;
            bus.OnCombatEvent += evt => received = evt;

            var killEvt = CombatEvent.EnemyKilled("player", "goblin_01", ElementType.Fire, true);
            bus.Publish(killEvt);

            Assert.IsNotNull(received);
            Assert.AreEqual(CombatEventCategory.EnemyKilled, received!.Value.Category);
        }

        [Test]
        public void Publish_EnemyKilled_TranslatesObjectiveEvent()
        {
            var bus = new CombatEventBus();
            ObjectiveEvent? objEvt = null;
            bus.OnObjectiveEvent += evt => objEvt = evt;

            bus.Publish(CombatEvent.EnemyKilled("player", "goblin", ElementType.Ice, false));

            Assert.IsNotNull(objEvt);
            Assert.AreEqual(ObjectiveEventType.EnemyKilled, objEvt!.Value.Type);
            Assert.AreEqual(ElementType.Ice, objEvt.Value.Element);
        }

        [Test]
        public void Publish_CritDamage_TranslatesCritObjectiveEvent()
        {
            var bus = new CombatEventBus();
            ObjectiveEvent? objEvt = null;
            bus.OnObjectiveEvent += evt => objEvt = evt;

            bus.Publish(CombatEvent.DamageDealt("player", "goblin", 100f, ElementType.Fire, isCritical: true));

            Assert.IsNotNull(objEvt);
            Assert.AreEqual(ObjectiveEventType.CriticalHit, objEvt!.Value.Type);
            Assert.IsTrue(objEvt.Value.IsCritical);
        }

        [Test]
        public void Publish_NonCritDamage_DoesNotEmitObjectiveEvent()
        {
            var bus = new CombatEventBus();
            var received = new List<ObjectiveEvent>();
            bus.OnObjectiveEvent += evt => received.Add(evt);

            bus.Publish(CombatEvent.DamageDealt("player", "goblin", 50f, ElementType.None, isCritical: false));

            Assert.AreEqual(0, received.Count, "Non-crit damage should not generate an objective event.");
        }

        [Test]
        public void Publish_DodgePerformed_TranslatesDodgeObjectiveEvent()
        {
            var bus = new CombatEventBus();
            ObjectiveEvent? objEvt = null;
            bus.OnObjectiveEvent += evt => objEvt = evt;

            bus.Publish(CombatEvent.DodgePerformed("player"));

            Assert.IsNotNull(objEvt);
            Assert.AreEqual(ObjectiveEventType.DodgeSuccess, objEvt!.Value.Type);
        }

        [Test]
        public void Publish_StatusApplied_TranslatesStatusObjectiveEvent()
        {
            var bus = new CombatEventBus();
            ObjectiveEvent? objEvt = null;
            bus.OnObjectiveEvent += evt => objEvt = evt;

            bus.Publish(CombatEvent.StatusApplied("player", "goblin", StatusEffectType.Burn));

            Assert.IsNotNull(objEvt);
            Assert.AreEqual(ObjectiveEventType.StatusApplied, objEvt!.Value.Type);
            Assert.AreEqual("Burn", objEvt.Value.SourceId);
        }

        [Test]
        public void Publish_BossDefeated_TranslatesBossObjectiveEvent()
        {
            var bus = new CombatEventBus();
            ObjectiveEvent? objEvt = null;
            bus.OnObjectiveEvent += evt => objEvt = evt;

            bus.Publish(CombatEvent.BossDefeated("player", "TrollKing"));

            Assert.IsNotNull(objEvt);
            Assert.AreEqual(ObjectiveEventType.BossDefeated, objEvt!.Value.Type);
            Assert.AreEqual("TrollKing", objEvt.Value.SourceId);
        }
    }
}
