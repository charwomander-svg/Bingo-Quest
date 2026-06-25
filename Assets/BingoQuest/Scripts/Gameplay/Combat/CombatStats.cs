using System;

namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Mutable stat block for a combat entity (player or enemy).
    /// All base stats are set at creation; modifiers are applied separately.
    /// </summary>
    public sealed class CombatStats
    {
        // ── Base stats ────────────────────────────────────────────────────────

        public float MaxHealth { get; set; }
        public float CurrentHealth { get; private set; }

        public float AttackPower { get; set; }
        public float AbilityPower { get; set; }
        public float Armor { get; set; }
        public float MagicResist { get; set; }

        public float CritChance { get; set; }   // 0–1 range
        public float CritMultiplier { get; set; } = 1.5f; // default 1.5×

        public float DodgeChance { get; set; }   // 0–1 range
        public float MoveSpeed { get; set; }

        // ── Bingo card bonuses (applied when patterns complete) ───────────────

        public float BonusDamageMultiplier { get; set; } = 1f;
        public float BonusDodgeChance { get; set; }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public bool IsAlive => CurrentHealth > 0f;

        public CombatStats(float maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        /// <summary>Applies damage, clamped to [0, MaxHealth].</summary>
        /// <returns>Actual damage dealt after armour reduction.</returns>
        public float ApplyDamage(float rawDamage, bool isMagical = false)
        {
            float reduction = isMagical ? MagicResist : Armor;
            float effective = Math.Max(0f, rawDamage - reduction);
            CurrentHealth = Math.Max(0f, CurrentHealth - effective);
            return effective;
        }

        /// <summary>Restores health, clamped to MaxHealth.</summary>
        public void Heal(float amount)
        {
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + Math.Max(0f, amount));
        }

        /// <summary>Resets health to max (used at run start or respawn).</summary>
        public void FullRestore() => CurrentHealth = MaxHealth;

        /// <summary>Effective crit chance including card bonuses, clamped to [0, 0.95].</summary>
        public float EffectiveCritChance => Math.Min(0.95f, CritChance);

        /// <summary>Effective dodge chance including card bonuses, clamped to [0, 0.75].</summary>
        public float EffectiveDodgeChance => Math.Min(0.75f, DodgeChance + BonusDodgeChance);

        /// <summary>Effective attack power including bingo multiplier.</summary>
        public float EffectiveAttackPower => AttackPower * BonusDamageMultiplier;
    }
}
