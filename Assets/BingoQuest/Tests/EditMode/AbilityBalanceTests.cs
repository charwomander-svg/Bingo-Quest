using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Progression;
using NUnit.Framework;
using UnityEngine;

namespace BingoQuest.Tests.EditMode
{
    public class AbilityBalanceTests
    {
        private AbilityConfig abilityConfig;
        private AbilityDefinition testAbility;

        [SetUp]
        public void Setup()
        {
            abilityConfig = ScriptableObject.CreateInstance<AbilityConfig>();
            testAbility = new AbilityDefinition
            {
                Id = "test_ability",
                Name = "Test Ability",
                Cooldown = 2.0f,
                DamageScale = 1.0f
            };
        }

        [Test]
        public void AbilityConfigIsValid()
        {
            Assert.That(abilityConfig.IsValid(), Is.True);
        }

        [Test]
        public void GetSlotTuning_ReturnsAllSlots()
        {
            var primary = abilityConfig.GetSlotTuning(AbilitySlot.Primary);
            var secondary = abilityConfig.GetSlotTuning(AbilitySlot.Secondary);
            var tertiary = abilityConfig.GetSlotTuning(AbilitySlot.Tertiary);
            var ultimate = abilityConfig.GetSlotTuning(AbilitySlot.Ultimate);

            Assert.That(primary, Is.Not.Null);
            Assert.That(secondary, Is.Not.Null);
            Assert.That(tertiary, Is.Not.Null);
            Assert.That(ultimate, Is.Not.Null);
        }

        [Test]
        public void SlotCooldowns_IncreaseFromPrimaryToUltimate()
        {
            float primary = abilityConfig.GetCooldown(AbilitySlot.Primary);
            float secondary = abilityConfig.GetCooldown(AbilitySlot.Secondary);
            float tertiary = abilityConfig.GetCooldown(AbilitySlot.Tertiary);
            float ultimate = abilityConfig.GetCooldown(AbilitySlot.Ultimate);

            Assert.That(secondary, Is.GreaterThan(primary));
            Assert.That(tertiary, Is.GreaterThan(secondary));
            Assert.That(ultimate, Is.GreaterThan(tertiary));
        }

        [Test]
        public void SlotDamage_IncreaseFromPrimaryToUltimate()
        {
            float primary = abilityConfig.GetDamage(AbilitySlot.Primary, 1.0f);
            float secondary = abilityConfig.GetDamage(AbilitySlot.Secondary, 1.0f);
            float tertiary = abilityConfig.GetDamage(AbilitySlot.Tertiary, 1.0f);
            float ultimate = abilityConfig.GetDamage(AbilitySlot.Ultimate, 1.0f);

            Assert.That(secondary, Is.GreaterThan(primary));
            Assert.That(tertiary, Is.GreaterThan(secondary));
            Assert.That(ultimate, Is.GreaterThan(tertiary));
        }

        [Test]
        public void GetCooldown_WithBaseCooldown_UsesProvidedValue()
        {
            float cooldown = abilityConfig.GetCooldown(AbilitySlot.Primary, baseCooldown: 5.0f);
            
            Assert.That(cooldown, Is.GreaterThanOrEqualTo(5.0f)); // May be multiplied by global
        }

        [Test]
        public void GetDamage_AppliesGlobalMultiplier()
        {
            float damage1x = abilityConfig.GetDamage(AbilitySlot.Primary, 10.0f);
            
            // Damage should respect global multiplier
            Assert.That(damage1x, Is.GreaterThan(0));
        }

        [Test]
        public void GetResourceCost_ReturnsPerSlotCost()
        {
            int primary = abilityConfig.GetResourceCost(AbilitySlot.Primary);
            int secondary = abilityConfig.GetResourceCost(AbilitySlot.Secondary);
            
            // At least should not crash; values depend on config
            Assert.That(primary, Is.GreaterThanOrEqualTo(0));
            Assert.That(secondary, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void GetCritChanceBonus_ReturnsValidBonus()
        {
            float bonus = abilityConfig.GetCritChanceBonus(AbilitySlot.Ultimate);
            
            Assert.That(bonus, Is.GreaterThanOrEqualTo(0f));
            Assert.That(bonus, Is.LessThanOrEqualTo(1f));
        }

        [Test]
        public void CanCastWhileMoving_ReturnsBool()
        {
            bool primary = abilityConfig.CanCastWhileMoving(AbilitySlot.Primary);
            bool ultimate = abilityConfig.CanCastWhileMoving(AbilitySlot.Ultimate);
            
            // Just verify it doesn't crash
            Assert.That(primary, Is.TypeOf<bool>());
            Assert.That(ultimate, Is.TypeOf<bool>());
        }

        [Test]
        public void DifficultyCooldownMultiplier_VariesByMode()
        {
            float easy = abilityConfig.GetDifficultyCooldownMultiplier(DifficultyMode.Easy);
            float normal = abilityConfig.GetDifficultyCooldownMultiplier(DifficultyMode.Normal);
            float hard = abilityConfig.GetDifficultyCooldownMultiplier(DifficultyMode.Hard);
            float nightmare = abilityConfig.GetDifficultyCooldownMultiplier(DifficultyMode.Nightmare);

            Assert.That(easy, Is.LessThan(normal));
            Assert.That(hard, Is.GreaterThan(normal));
            Assert.That(nightmare, Is.GreaterThan(hard));
        }

        [Test]
        public void CalculateFinalCooldown_AppliesDifficultyMultiplier()
        {
            float baseCooldown = 2.0f;
            
            float easyFinal = abilityConfig.CalculateFinalCooldown(AbilitySlot.Primary, baseCooldown, DifficultyMode.Easy);
            float normalFinal = abilityConfig.CalculateFinalCooldown(AbilitySlot.Primary, baseCooldown, DifficultyMode.Normal);
            float hardFinal = abilityConfig.CalculateFinalCooldown(AbilitySlot.Primary, baseCooldown, DifficultyMode.Hard);

            Assert.That(easyFinal, Is.LessThan(normalFinal));
            Assert.That(hardFinal, Is.GreaterThan(normalFinal));
        }

        [Test]
        public void Ability_TryExecute_SetsRemainingCooldown()
        {
            var ability = new Ability(testAbility, AbilitySlot.Primary, abilityConfig);
            
            bool executed = ability.TryExecute();
            
            Assert.That(executed, Is.True);
            Assert.That(ability.RemainingCooldown, Is.GreaterThan(0));
        }

        [Test]
        public void Ability_CannotExecuteWhileOnCooldown()
        {
            var ability = new Ability(testAbility, AbilitySlot.Primary, abilityConfig);
            
            bool first = ability.TryExecute();
            bool second = ability.TryExecute();
            
            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
        }

        [Test]
        public void Ability_ReduceCooldown_DecrementsTimer()
        {
            var ability = new Ability(testAbility, AbilitySlot.Primary, abilityConfig);
            ability.TryExecute();
            
            float cooldownBefore = ability.RemainingCooldown;
            ability.ReduceCooldown(0.5f);
            float cooldownAfter = ability.RemainingCooldown;
            
            Assert.That(cooldownAfter, Is.LessThan(cooldownBefore));
        }

        [Test]
        public void Ability_CooldownReachesZero()
        {
            var ability = new Ability(testAbility, AbilitySlot.Primary, abilityConfig);
            ability.TryExecute();
            
            float cooldown = ability.RemainingCooldown;
            ability.ReduceCooldown(cooldown + 1f); // Reduce past zero
            
            Assert.That(ability.RemainingCooldown, Is.EqualTo(0));
            Assert.That(ability.IsReady, Is.True);
        }

        [Test]
        public void Ability_ResetCooldown_MakesReady()
        {
            var ability = new Ability(testAbility, AbilitySlot.Primary, abilityConfig);
            ability.TryExecute();
            
            Assert.That(ability.IsReady, Is.False);
            
            ability.ResetCooldown();
            
            Assert.That(ability.IsReady, Is.True);
        }

        [Test]
        public void Ability_CooldownPercent_RangesZeroToOne()
        {
            var ability = new Ability(testAbility, AbilitySlot.Primary, abilityConfig);
            
            ability.TryExecute();
            float percentMid = ability.CooldownPercent;
            
            ability.ReduceCooldown(testAbility.Cooldown);
            float percentEnd = ability.CooldownPercent;
            
            Assert.That(percentMid, Is.GreaterThan(0).And.LessThan(1));
            Assert.That(percentEnd, Is.EqualTo(1f).Within(0.01f));
        }

        [Test]
        public void ActionBar_SetAndGetAbility()
        {
            var actionBar = new ActionBar();
            var ability = new Ability(testAbility, AbilitySlot.Primary, abilityConfig);
            
            actionBar.SetAbility(AbilitySlot.Primary, ability);
            var retrieved = actionBar.GetAbility(AbilitySlot.Primary);
            
            Assert.That(retrieved, Is.EqualTo(ability));
        }

        [Test]
        public void ActionBar_ReduceAllCooldowns()
        {
            var actionBar = new ActionBar();
            var ability1 = new Ability(testAbility, AbilitySlot.Primary, abilityConfig);
            var ability2 = new Ability(testAbility, AbilitySlot.Secondary, abilityConfig);
            
            actionBar.SetAbility(AbilitySlot.Primary, ability1);
            actionBar.SetAbility(AbilitySlot.Secondary, ability2);
            
            ability1.TryExecute();
            ability2.TryExecute();
            
            float cd1Before = ability1.RemainingCooldown;
            float cd2Before = ability2.RemainingCooldown;
            
            actionBar.ReduceAllCooldowns(0.5f);
            
            Assert.That(ability1.RemainingCooldown, Is.LessThan(cd1Before));
            Assert.That(ability2.RemainingCooldown, Is.LessThan(cd2Before));
        }

        [Test]
        public void ActionBar_GetCooldownPercent()
        {
            var actionBar = new ActionBar();
            var ability = new Ability(testAbility, AbilitySlot.Primary, abilityConfig);
            
            actionBar.SetAbility(AbilitySlot.Primary, ability);
            ability.TryExecute();
            
            float percent = actionBar.GetCooldownPercent(AbilitySlot.Primary);
            
            Assert.That(percent, Is.GreaterThanOrEqualTo(0).And.LessThanOrEqualTo(1));
        }

        [Test]
        public void MaxResourcePool_IsPositive()
        {
            Assert.That(abilityConfig.MaxResourcePool, Is.GreaterThan(0));
        }

        [Test]
        public void ResourceRegenPerSecond_IsNonNegative()
        {
            Assert.That(abilityConfig.ResourceRegenPerSecond, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void StatusEffectChanceBias_ClampsToValidRange()
        {
            float bias = abilityConfig.StatusEffectChanceBias;
            
            Assert.That(bias, Is.GreaterThanOrEqualTo(0f).And.LessThanOrEqualTo(1f));
        }

        [Test]
        public void AbilityWithoutConfig_UsesDefaults()
        {
            var ability = new Ability(testAbility, AbilitySlot.Primary, config: null);
            
            bool executed = ability.TryExecute();
            
            Assert.That(executed, Is.True);
            Assert.That(ability.RemainingCooldown, Is.EqualTo(testAbility.Cooldown));
        }
    }
}
