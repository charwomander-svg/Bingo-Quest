using BingoQuest.Gameplay.Balance;
using BingoQuest.Gameplay.Objectives;
using UnityEngine;

namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Character stats that drive damage calculations and combat behavior.
    /// Can be modified by equipment, buffs, and Bingo card rewards.
    /// </summary>
    public class CharacterStats
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        
        public int Attack { get; set; }
        public int Defense { get; set; }
        
        public float CritChance { get; set; }  // 0-1 (0-100%)
        public float CritDamage { get; set; } // Multiplier (e.g., 1.5x = 50% bonus)
        
        public float DodgeChance { get; set; } // 0-1
        public float AttackSpeed { get; set; } // Cooldown multiplier (0.5 = 2x speed)
        
        public int ElementalPower { get; set; } // Bonus damage for status effects
        
        public CharacterStats()
        {
            MaxHealth = 100;
            Health = MaxHealth;
            Attack = 10;
            Defense = 5;
            CritChance = 0.1f;
            CritDamage = 1.5f;
            DodgeChance = 0.0f;
            AttackSpeed = 1.0f;
            ElementalPower = 0;
        }

        public void ResetToMaxHealth() => Health = MaxHealth;
        
        public void TakeDamage(int amount)
        {
            Health = Mathf.Max(0, Health - amount);
        }

        public void Heal(int amount)
        {
            Health = Mathf.Min(MaxHealth, Health + amount);
        }

        public bool IsAlive => Health > 0;

        public override string ToString() =>
            $"HP: {Health}/{MaxHealth} | ATK: {Attack} | DEF: {Defense} | " +
            $"CRIT: {(CritChance * 100):F0}% | DODGE: {(DodgeChance * 100):F0}%";
    }

    /// <summary>
    /// Damage calculation engine with crits, defenses, and elemental modifiers.
    /// </summary>
    public class DamageCalculator
    {
        private System.Random random = new(System.Environment.TickCount);
        private BalanceConfig balanceConfig;

        public DamageCalculator(BalanceConfig config = null)
        {
            balanceConfig = config;
        }

        public DamageResult CalculateDamage(
            CharacterStats attacker,
            CharacterStats defender,
            AbilityDefinition ability,
            float difficultyModifier = 1.0f)
        {
            // Check if defender dodges
            if (random.NextSingle() < defender.DodgeChance)
                return DamageResult.Dodged();

            // Base damage from attack stat + ability scaling
            float damageScale = balanceConfig?.BaseDamageScale ?? 1.0f;
            int baseDamage = (int)(attacker.Attack * ability.DamageScale * damageScale);

            // Elemental bonus if applicable
            if (ability.ElementType != ElementType.Physical)
            {
                float elementalScale = balanceConfig?.ElementalPowerScale ?? 1.0f;
                baseDamage += (int)(attacker.ElementalPower * elementalScale);
            }

            // Check critical hit
            bool isCrit = random.NextSingle() < attacker.CritChance;
            if (isCrit)
            {
                float critMult = balanceConfig?.CritDamageMultiplier ?? 1.5f;
                baseDamage = (int)(baseDamage * critMult);
            }

            // Apply enemy defense (configurable mitigation factor)
            float mitigationFactor = balanceConfig?.DefenseMitigationFactor ?? 0.5f;
            int mitigatedDamage = Mathf.Max(1, baseDamage - (int)(defender.Defense * mitigationFactor));

            // Apply minimum damage threshold
            if (balanceConfig != null)
            {
                int minDamage = balanceConfig.CalculateMinDamage(baseDamage);
                mitigatedDamage = Mathf.Max(minDamage, mitigatedDamage);
            }

            // Apply difficulty modifier
            int finalDamage = (int)(mitigatedDamage * difficultyModifier);

            return new DamageResult
            {
                FinalDamage = finalDamage,
                BaseDamage = baseDamage,
                IsCritical = isCrit,
                WasDodged = false,
                Element = ability.ElementType
            };
        }

        public DamageResult CalculateAutoAttack(
            CharacterStats attacker,
            CharacterStats defender,
            float difficultyModifier = 1.0f)
        {
            var basicAbility = new AbilityDefinition
            {
                DamageScale = 1.0f,
                ElementType = ElementType.Physical
            };
            return CalculateDamage(attacker, defender, basicAbility, difficultyModifier);
        }
    }

    /// <summary>Result of a damage calculation.</summary>
    public struct DamageResult
    {
        public int FinalDamage { get; set; }
        public int BaseDamage { get; set; }
        public bool IsCritical { get; set; }
        public bool WasDodged { get; set; }
        public ElementType Element { get; set; }

        public static DamageResult Dodged() => new() { WasDodged = true, FinalDamage = 0 };

        public override string ToString() =>
            WasDodged ? "DODGED!" :
            IsCritical ? $"CRIT! {FinalDamage} damage ({Element})" :
            $"{FinalDamage} damage ({Element})";
    }
}
