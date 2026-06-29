using BingoQuest.Gameplay.Balance;
using BingoQuest.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace BingoQuest.Tests.EditMode
{
    public class CombatBalanceTests
    {
        private BalanceConfig balanceConfig;
        private DamageCalculator calculator;
        private CharacterStats player;
        private CharacterStats enemy;

        [SetUp]
        public void Setup()
        {
            balanceConfig = ScriptableObject.CreateInstance<BalanceConfig>();
            calculator = new DamageCalculator(balanceConfig);
            player = new CharacterStats();
            enemy = new CharacterStats();
        }

        [Test]
        public void DamageScale_AppliesBaseDamageMultiplier()
        {
            // Default: 10 ATK * 1.0 DamageScale = base 10 damage
            var ability = new AbilityDefinition { DamageScale = 1.0f, ElementType = ElementType.Physical };
            var result = calculator.CalculateDamage(player, enemy, ability);
            
            // Base damage = 10 * 1.0 * 1.0 = 10
            // Minus defense (5 * 0.5) = 2.5, rounds to 2
            // Final = 10 - 2 = 8
            Assert.That(result.BaseDamage, Is.GreaterThan(0));
            Assert.That(result.FinalDamage, Is.GreaterThan(0));
        }

        [Test]
        public void DefenseMitigation_ReducesDamageCorrectly()
        {
            player.Attack = 20;
            enemy.Defense = 10;
            
            var ability = new AbilityDefinition { DamageScale = 1.0f, ElementType = ElementType.Physical };
            var result = calculator.CalculateDamage(player, enemy, ability);
            
            // Base damage = 20 * 1.0 * 1.0 = 20
            // Mitigation = 10 * 0.5 = 5
            // Final = 20 - 5 = 15
            Assert.That(result.FinalDamage, Is.EqualTo(15));
        }

        [Test]
        public void MinimumDamage_EnforcesDamageFloor()
        {
            player.Attack = 5;
            enemy.Defense = 20;
            
            var ability = new AbilityDefinition { DamageScale = 1.0f, ElementType = ElementType.Physical };
            var result = calculator.CalculateDamage(player, enemy, ability);
            
            // Without min damage: 5 - (20 * 0.5) = 5 - 10 = -5 → clamped to 1
            // With min damage: max(1, 5 * 0.05) = max(1, 0) = 1
            Assert.That(result.FinalDamage, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void CriticalHit_AppliesToDamage()
        {
            player.Attack = 10;
            player.CritChance = 1.0f; // Guaranteed crit
            
            var ability = new AbilityDefinition { DamageScale = 1.0f, ElementType = ElementType.Physical };
            var result = calculator.CalculateDamage(player, enemy, ability);
            
            Assert.That(result.IsCritical, Is.True);
            Assert.That(result.BaseDamage, Is.GreaterThan(10)); // Should be amplified by crit multiplier
        }

        [Test]
        public void DodgeChance_BypassesDamage()
        {
            enemy.DodgeChance = 1.0f; // Guaranteed dodge
            
            var ability = new AbilityDefinition { DamageScale = 1.0f, ElementType = ElementType.Physical };
            var result = calculator.CalculateDamage(player, enemy, ability);
            
            Assert.That(result.WasDodged, Is.True);
            Assert.That(result.FinalDamage, Is.EqualTo(0));
        }

        [Test]
        public void ElementalDamage_ScalesWithElementalPower()
        {
            player.Attack = 10;
            player.ElementalPower = 5;
            
            var ability = new AbilityDefinition { DamageScale = 1.0f, ElementType = ElementType.Fire };
            var result = calculator.CalculateDamage(player, enemy, ability);
            
            // Base damage = 10 * 1.0 = 10 + elemental power scaled
            Assert.That(result.BaseDamage, Is.GreaterThan(10));
            Assert.That(result.Element, Is.EqualTo(ElementType.Fire));
        }

        [Test]
        public void DifficultyModifier_ScalesDamage()
        {
            var ability = new AbilityDefinition { DamageScale = 1.0f, ElementType = ElementType.Physical };
            
            // Easy mode (0.75x)
            var easyResult = calculator.CalculateDamage(player, enemy, ability, difficultyModifier: 0.75f);
            
            // Normal mode (1.0x)
            var normalResult = calculator.CalculateDamage(player, enemy, ability, difficultyModifier: 1.0f);
            
            Assert.That(easyResult.FinalDamage, Is.LessThan(normalResult.FinalDamage));
        }

        [Test]
        public void EnemyHealthScaling_IncreasesByLevel()
        {
            int level1Health = balanceConfig.CalculateEnemyHealth(1);
            int level5Health = balanceConfig.CalculateEnemyHealth(5);
            int level10Health = balanceConfig.CalculateEnemyHealth(10);
            
            Assert.That(level5Health, Is.GreaterThan(level1Health));
            Assert.That(level10Health, Is.GreaterThan(level5Health));
        }

        [Test]
        public void BossHealthMultiplier_IncreasesBossHealth()
        {
            int normalHealth = balanceConfig.CalculateEnemyHealth(5, isBoss: false);
            int bossHealth = balanceConfig.CalculateEnemyHealth(5, isBoss: true);
            
            Assert.That(bossHealth, Is.GreaterThan(normalHealth));
            Assert.That(bossHealth, Is.EqualTo((int)(normalHealth * balanceConfig.BossHealthMultiplier)));
        }

        [Test]
        public void RegionMultipliers_ScaleByTier()
        {
            var (tier0Health, tier0Damage) = balanceConfig.GetRegionMultipliers(0);
            var (tier1Health, tier1Damage) = balanceConfig.GetRegionMultipliers(1);
            var (tier2Health, tier2Damage) = balanceConfig.GetRegionMultipliers(2);
            
            Assert.That(tier1Health, Is.GreaterThan(tier0Health));
            Assert.That(tier2Health, Is.GreaterThan(tier1Health));
            Assert.That(tier1Damage, Is.GreaterThan(tier0Damage));
            Assert.That(tier2Damage, Is.GreaterThan(tier1Damage));
        }

        [Test]
        public void DifficultyMode_ReturnsCorrectMultipliers()
        {
            float easy = balanceConfig.GetDifficultyMultiplier(DifficultyMode.Easy);
            float normal = balanceConfig.GetDifficultyMultiplier(DifficultyMode.Normal);
            float hard = balanceConfig.GetDifficultyMultiplier(DifficultyMode.Hard);
            float nightmare = balanceConfig.GetDifficultyMultiplier(DifficultyMode.Nightmare);
            
            Assert.That(easy, Is.LessThan(normal));
            Assert.That(hard, Is.GreaterThan(normal));
            Assert.That(nightmare, Is.GreaterThan(hard));
        }

        [Test]
        public void AutoAttack_CalculatesDamageWithoutAbility()
        {
            var result = calculator.CalculateAutoAttack(player, enemy);
            
            Assert.That(result.FinalDamage, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.Element, Is.EqualTo(ElementType.Physical));
        }
    }
}
