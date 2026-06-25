using System;

namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Processes a raw damage value through the full damage pipeline:
    /// critical-hit roll → elemental modifier → armour/resistance → final damage.
    /// Pure C# – no Unity runtime dependency.
    /// </summary>
    public sealed class DamagePipeline
    {
        private readonly Random _rng;

        /// <summary>Global damage multiplier for debugging / balancing. Default 1.</summary>
        public float GlobalDamageScale { get; set; } = 1f;

        public DamagePipeline(int seed = 0)
        {
            _rng = seed != 0 ? new Random(seed) : new Random();
        }

        /// <summary>
        /// Runs the full pipeline and returns a <see cref="DamageData"/> snapshot.
        /// Does NOT apply the damage to the target; the caller does that.
        /// </summary>
        public DamageData Calculate(
            string actorId,
            string targetId,
            float baseDamage,
            Core.ElementType element,
            CombatStats attackerStats,
            CombatStats defenderStats)
        {
            if (attackerStats is null) throw new ArgumentNullException(nameof(attackerStats));
            if (defenderStats is null) throw new ArgumentNullException(nameof(defenderStats));

            // 1. Apply attacker's effective attack power modifier
            float damage = baseDamage * (attackerStats.EffectiveAttackPower / 100f);

            // 2. Critical hit roll
            bool isCrit = (float)_rng.NextDouble() < attackerStats.EffectiveCritChance;
            if (isCrit) damage *= attackerStats.CritMultiplier;

            // 3. Elemental modifier (placeholder – extend with resistance system)
            damage *= GetElementalMultiplier(element, defenderStats);

            // 4. Armour / magic resist flat reduction
            bool isMagical = IsMagicalElement(element);
            float reduction = isMagical ? defenderStats.MagicResist : defenderStats.Armor;
            damage = Math.Max(1f, damage - reduction); // minimum 1 damage

            // 5. Global scale (for debug / difficulty tuning)
            damage *= GlobalDamageScale;

            return new DamageData(actorId, targetId, baseDamage, damage, element, isCrit, false, StatusEffectType.None);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static float GetElementalMultiplier(Core.ElementType element, CombatStats defender)
        {
            // Extend this with per-resistance fields as the game grows
            return element switch
            {
                Core.ElementType.Fire      => 1.0f,
                Core.ElementType.Ice       => 1.0f,
                Core.ElementType.Lightning => 1.1f, // lightning penetrates slightly
                Core.ElementType.Poison    => 0.9f, // slower but DoT
                Core.ElementType.Shadow    => 1.0f,
                Core.ElementType.Holy      => 1.0f,
                _ => 1.0f
            };
        }

        private static bool IsMagicalElement(Core.ElementType element) =>
            element != Core.ElementType.None;
    }
}
