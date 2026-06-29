using NUnit.Framework;
using BingoQuest.Gameplay.Content;
using BingoQuest.Gameplay.Combat;

namespace BingoQuest.Tests.EditMode
{
    public sealed class ContentSystemTests
    {
        private ContentLibrary _library;

        [SetUp]
        public void SetUp()
        {
            _library = new ContentLibrary();
        }

        [Test]
        public void RegisterEnemy_StoresAndRetrieves()
        {
            var enemy = new EnemyDefinition { EnemyId = "goblin", DisplayName = "Goblin", BaseHealth = 30 };
            _library.RegisterEnemy(enemy);
            
            var retrieved = _library.GetEnemy("goblin");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("Goblin", retrieved.DisplayName);
            Assert.AreEqual(30, retrieved.BaseHealth);
        }

        [Test]
        public void RegisterBoss_StoresAndRetrieves()
        {
            var boss = new BossDefinition { BossId = "dragon", DisplayName = "Dragon", BaseHealth = 400 };
            _library.RegisterBoss(boss);
            
            var retrieved = _library.GetBoss("dragon");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("Dragon", retrieved.DisplayName);
            Assert.AreEqual(400, retrieved.BaseHealth);
        }

        [Test]
        public void RegisterAbilityPool_StoresAndRetrieves()
        {
            var pool = new AbilityPool { PoolId = "warrior_abilities", name = "Warrior Abilities" };
            pool.Abilities.Add(new AbilityDefinitionData { AbilityId = "slash", Name = "Slash", DamageScale = 1.5f });
            _library.RegisterAbilityPool(pool);
            
            var retrieved = _library.GetAbilityPool("warrior_abilities");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(1, retrieved.Abilities.Count);
            Assert.AreEqual("slash", retrieved.Abilities[0].AbilityId);
        }

        [Test]
        public void RegisterObjectivePool_StoresAndRetrieves()
        {
            var pool = new ObjectivePool { PoolId = "tier1_objectives", name = "Tier 1" };
            pool.Objectives.Add(new ObjectiveDefinitionData { ObjectiveId = "kill_5", Type = ObjectiveType.Kill, RequiredProgress = 5 });
            _library.RegisterObjectivePool(pool);
            
            var retrieved = _library.GetObjectivePool("tier1_objectives");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(1, retrieved.Objectives.Count);
            Assert.AreEqual(5, retrieved.Objectives[0].RequiredProgress);
        }

        [Test]
        public void CreateStatsFromEnemy_BuildsCorrectStats()
        {
            var enemy = new EnemyDefinition
            {
                EnemyId = "test_enemy",
                BaseHealth = 100,
                BaseAttack = 15,
                BaseDefense = 5,
                BaseCritChance = 0.2f,
                BaseDodgeChance = 0.1f
            };

            var stats = ContentFactory.CreateStatsFromEnemy(enemy);
            Assert.AreEqual(100, stats.MaxHealth);
            Assert.AreEqual(15, stats.Attack);
            Assert.AreEqual(5, stats.Defense);
            Assert.AreEqual(0.2f, stats.CritChance);
            Assert.AreEqual(0.1f, stats.DodgeChance);
        }

        [Test]
        public void CreateAbilityFromData_BuildsCorrectAbility()
        {
            var data = new AbilityDefinitionData
            {
                AbilityId = "fireball",
                Name = "Fireball",
                Cooldown = 2.0f,
                DamageScale = 1.8f,
                ElementType = ElementType.Fire
            };

            var ability = ContentFactory.CreateAbilityFromData(data);
            Assert.IsNotNull(ability);
            Assert.AreEqual("Fireball", ability.Definition.Name);
            Assert.AreEqual(2.0f, ability.Definition.Cooldown);
            Assert.AreEqual(1.8f, ability.Definition.DamageScale);
        }

        [Test]
        public void GetAbilitiesFromPool_ReturnsAllAbilities()
        {
            var pool = new AbilityPool { PoolId = "test_pool" };
            pool.Abilities.Add(new AbilityDefinitionData { AbilityId = "a1", Name = "Ability 1" });
            pool.Abilities.Add(new AbilityDefinitionData { AbilityId = "a2", Name = "Ability 2" });
            pool.Abilities.Add(new AbilityDefinitionData { AbilityId = "a3", Name = "Ability 3" });
            _library.RegisterAbilityPool(pool);

            var abilities = _library.GetAbilitiesFromPool("test_pool");
            Assert.AreEqual(3, abilities.Count);
        }

        [Test]
        public void NullEnemyId_NotRegistered()
        {
            var enemy = new EnemyDefinition { EnemyId = null, DisplayName = "NoID" };
            _library.RegisterEnemy(enemy);
            
            Assert.IsNull(_library.GetEnemy(null));
        }
    }
}
