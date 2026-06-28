using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Objectives;
using NUnit.Framework;
using UnityEngine;

namespace BingoQuest.Tests.PlayMode
{
    public class CombatSystemTests
    {
        private DamageCalculator damageCalculator;
        private CharacterStats attacker;
        private CharacterStats defender;

        [SetUp]
        public void Setup()
        {
            damageCalculator = new DamageCalculator();
            attacker = new CharacterStats { Attack = 10 };
            defender = new CharacterStats { Defense = 5 };
        }

        [Test]
        public void DamageCalculator_CalculatesBaseDamage()
        {
            var ability = new AbilityDefinition { DamageScale = 1.0f };
            var result = damageCalculator.CalculateDamage(attacker, defender, ability);

            // 10 attack * 1.0 scale = 10 base, minus half defense (2.5) = ~7-8 min damage
            Assert.Greater(result.FinalDamage, 0);
            Assert.Less(result.FinalDamage, 20);
        }

        [Test]
        public void DamageCalculator_CriticalHitScales()
        {
            attacker.CritChance = 1.0f; // Always crit
            attacker.CritDamage = 2.0f; // 2x damage on crit

            var ability = new AbilityDefinition { DamageScale = 1.0f };
            var result = damageCalculator.CalculateDamage(attacker, defender, ability);

            Assert.IsTrue(result.IsCritical);
            Assert.Greater(result.FinalDamage, result.BaseDamage); // Scaled up by crit multiplier
        }

        [Test]
        public void DamageCalculator_DefenseReducesDamage()
        {
            var ability = new AbilityDefinition { DamageScale = 1.0f };

            defender.Defense = 0;
            var noDef = damageCalculator.CalculateDamage(attacker, defender, ability);

            defender.Defense = 20;
            var withDef = damageCalculator.CalculateDamage(attacker, defender, ability);

            Assert.Greater(noDef.FinalDamage, withDef.FinalDamage);
        }

        [Test]
        public void DamageCalculator_ElementalPowerBoosts()
        {
            attacker.ElementalPower = 10;
            var ability = new AbilityDefinition
            {
                DamageScale = 1.0f,
                ElementType = ElementType.Fire
            };

            var result = damageCalculator.CalculateDamage(attacker, defender, ability);
            Assert.Greater(result.FinalDamage, 0);
        }

        [Test]
        public void Ability_ManagesCooldowns()
        {
            var abilityDef = new AbilityDefinition { Cooldown = 1.0f };
            var ability = new Ability(abilityDef);

            Assert.IsTrue(ability.IsReady);
            Assert.IsTrue(ability.TryExecute()); // Execute
            Assert.IsFalse(ability.IsReady);    // Now on cooldown

            ability.ReduceCooldown(0.5f);
            Assert.IsFalse(ability.IsReady);    // Still cooling down

            ability.ReduceCooldown(0.5f);
            Assert.IsTrue(ability.IsReady);     // Cooldown expired
        }

        [Test]
        public void Ability_CooldownPercentProgresses()
        {
            var abilityDef = new AbilityDefinition { Cooldown = 1.0f };
            var ability = new Ability(abilityDef);

            ability.TryExecute();
            float percent1 = ability.CooldownPercent;

            ability.ReduceCooldown(0.5f);
            float percent2 = ability.CooldownPercent;

            Assert.Less(percent1, percent2);
            Assert.Greater(percent2, 0.4f);
        }

        [Test]
        public void CharacterStats_TakeDamage()
        {
            var stats = new CharacterStats { Health = 100, MaxHealth = 100 };
            stats.TakeDamage(25);

            Assert.AreEqual(75, stats.Health);
            Assert.IsTrue(stats.IsAlive);
        }

        [Test]
        public void CharacterStats_CantGoNegative()
        {
            var stats = new CharacterStats { Health = 20 };
            stats.TakeDamage(100);

            Assert.AreEqual(0, stats.Health);
            Assert.IsFalse(stats.IsAlive);
        }

        [Test]
        public void CharacterStats_Heal()
        {
            var stats = new CharacterStats { Health = 50, MaxHealth = 100 };
            stats.Heal(30);

            Assert.AreEqual(80, stats.Health);
            Assert.Less(stats.Health, stats.MaxHealth);
        }

        [Test]
        public void StatusEffectManager_AppliesEffects()
        {
            var manager = new StatusEffectManager();
            manager.ApplyEffect(StatusEffectType.Burn, 3.0f, 5.0f);

            Assert.IsTrue(manager.HasEffect(StatusEffectType.Burn));
            Assert.AreEqual(1, manager.ActiveEffects[StatusEffectType.Burn].StackCount);
        }

        [Test]
        public void StatusEffectManager_StacksEffects()
        {
            var manager = new StatusEffectManager();
            manager.ApplyEffect(StatusEffectType.Poison, 2.0f, 3.0f);
            manager.ApplyEffect(StatusEffectType.Poison, 2.0f, 3.0f);

            Assert.AreEqual(2, manager.ActiveEffects[StatusEffectType.Poison].StackCount);
        }

        [Test]
        public void StatusEffectManager_TicksDamageOverTime()
        {
            var manager = new StatusEffectManager();
            manager.ApplyEffect(StatusEffectType.Burn, 3.0f, 10.0f); // 10 DPS

            float damage1 = manager.TickAndGetDamage(1.0f); // 1 second
            float damage2 = manager.TickAndGetDamage(1.0f); // 1 second

            Assert.AreEqual(10, damage1, 0.1f);
            Assert.AreEqual(10, damage2, 0.1f);
        }

        [Test]
        public void StatusEffectManager_RemovesExpiredEffects()
        {
            var manager = new StatusEffectManager();
            manager.ApplyEffect(StatusEffectType.Freeze, 1.0f);

            manager.TickAndGetDamage(0.5f);
            Assert.IsTrue(manager.HasEffect(StatusEffectType.Freeze));

            manager.TickAndGetDamage(0.6f);
            Assert.IsFalse(manager.HasEffect(StatusEffectType.Freeze));
        }
    }

    public class ActionBarTests
    {
        [Test]
        public void ActionBar_StoresAbilities()
        {
            var actionBar = new ActionBar();
            var ability = new Ability(new AbilityDefinition { Cooldown = 1.0f });

            actionBar.SetAbility(AbilitySlot.Primary, ability);
            Assert.AreEqual(ability, actionBar.GetAbility(AbilitySlot.Primary));
        }

        [Test]
        public void ActionBar_ReducesCooldownsForAll()
        {
            var actionBar = new ActionBar();
            for (int i = 0; i < 4; i++)
            {
                var ability = new Ability(new AbilityDefinition { Cooldown = 2.0f });
                ability.TryExecute();
                actionBar.SetAbility((AbilitySlot)i, ability);
            }

            actionBar.ReduceAllCooldowns(1.0f);

            for (int i = 0; i < 4; i++)
            {
                var ability = actionBar.GetAbility((AbilitySlot)i);
                Assert.Less(ability.RemainingCooldown, 1.0f);
            }
        }
    }
}
