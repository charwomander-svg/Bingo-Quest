using System;
using BingoQuest.Core;

namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Central event bus for all combat-layer events.
    /// Translates <see cref="CombatEvent"/> records into <see cref="ObjectiveEvent"/> records
    /// so the objective system never depends directly on the combat layer.
    /// </summary>
    public sealed class CombatEventBus
    {
        // ── Raw combat subscribers (other combat-layer systems) ────────────────
        public event Action<CombatEvent>? OnCombatEvent;

        // ── Translated objective events (consumed by BingoCard) ────────────────
        public event Action<ObjectiveEvent>? OnObjectiveEvent;

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Publishes a combat event to all subscribers and, if applicable,
        /// translates it to the objective layer.
        /// </summary>
        public void Publish(in CombatEvent evt)
        {
            OnCombatEvent?.Invoke(evt);

            var objEvt = Translate(evt);
            if (objEvt.Type != ObjectiveEventType.None)
                OnObjectiveEvent?.Invoke(objEvt);
        }

        // ── Translation ────────────────────────────────────────────────────────

        private static ObjectiveEvent Translate(in CombatEvent evt) =>
            evt.Category switch
            {
                CombatEventCategory.EnemyKilled =>
                    new ObjectiveEvent(
                        ObjectiveEventType.EnemyKilled,
                        evt.TargetId,
                        1,
                        evt.Element,
                        evt.IsCritical,
                        false,
                        false),

                CombatEventCategory.DamageDealt when evt.IsCritical =>
                    new ObjectiveEvent(
                        ObjectiveEventType.CriticalHit,
                        evt.ActorId,
                        1,
                        evt.Element,
                        true,
                        false,
                        false),

                CombatEventCategory.StatusApplied =>
                    new ObjectiveEvent(
                        ObjectiveEventType.StatusApplied,
                        evt.StatusEffect.ToString(),
                        1,
                        evt.Element,
                        false,
                        false,
                        false),

                CombatEventCategory.DodgePerformed =>
                    new ObjectiveEvent(
                        ObjectiveEventType.DodgeSuccess,
                        evt.ActorId,
                        1,
                        ElementType.None,
                        false,
                        false,
                        false),

                CombatEventCategory.AbilityUsed =>
                    new ObjectiveEvent(
                        ObjectiveEventType.AbilityUsed,
                        evt.TargetId, // TargetId holds abilityId for this category
                        1,
                        ElementType.None,
                        false,
                        false,
                        false),

                CombatEventCategory.BossDefeated =>
                    new ObjectiveEvent(
                        ObjectiveEventType.BossDefeated,
                        evt.TargetId,
                        1,
                        ElementType.None,
                        false,
                        false,
                        false),

                _ => default // ObjectiveEventType.None – not forwarded
            };
    }
}
