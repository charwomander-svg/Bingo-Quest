using System.Collections.Generic;
using System.Reflection;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Content;
using BingoQuest.Gameplay.Objectives;
using BingoQuest.Gameplay.Progression;
using NUnit.Framework;

namespace BingoQuest.Tests.EditMode
{
    public sealed class AuthoredContentCatalogTests
    {
        private AuthoredContentCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            catalog = AuthoredContentCatalog.CreateDefault();
        }

        [Test]
        public void CreateDefault_BuildsAuthoredContentSets()
        {
            Assert.That(catalog.Library.GetEnemyCount, Is.EqualTo(9));
            Assert.That(catalog.Library.GetBossCount, Is.EqualTo(3));
            Assert.That(catalog.Library.GetAbilityPoolCount, Is.EqualTo(5));
            Assert.That(catalog.Library.GetObjectivePoolCount, Is.EqualTo(3));
            Assert.That(catalog.GetRegionProfile("whispering_forest"), Is.Not.Null);
            Assert.That(catalog.GetClassDefinition("mage"), Is.Not.Null);
            Assert.That(catalog.GetClassDefinition("rogue"), Is.Not.Null);
            Assert.That(catalog.GetClassDefinition("cleric"), Is.Not.Null);
        }

        [Test]
        public void GetEnemyForRegion_ReturnsExpectedEnemy()
        {
            var enemy = catalog.GetEnemyForRegion("whispering_forest", 0);

            Assert.That(enemy, Is.Not.Null);
            Assert.That(enemy.EnemyId, Is.EqualTo("briarwolf"));
            Assert.That(enemy.DisplayName, Is.EqualTo("Briar Wolf"));
        }

        [Test]
        public void GetBossForRegion_ReturnsExpectedBoss()
        {
            var boss = catalog.GetBossForRegion("frozen_peaks");

            Assert.That(boss, Is.Not.Null);
            Assert.That(boss.BossId, Is.EqualTo("whiteout_wyrm"));
            Assert.That(boss.DisplayName, Is.EqualTo("Whiteout Wyrm"));
        }

        [Test]
        public void CreateStartingAbilities_ReturnsClassLoadout()
        {
            var abilities = catalog.CreateStartingAbilities("mage");

            Assert.That(abilities.Count, Is.EqualTo(4));
            Assert.That(abilities[0].Definition.Name, Is.EqualTo("Fireball"));
            Assert.That(abilities[1].Definition.Name, Is.EqualTo("Frostbolt"));
        }

        [Test]
        public void CreateStartingAbilities_RogueAndClericHaveLoadouts()
        {
            var rogueAbilities = catalog.CreateStartingAbilities("rogue");
            var clericAbilities = catalog.CreateStartingAbilities("cleric");

            Assert.That(rogueAbilities.Count, Is.EqualTo(4));
            Assert.That(rogueAbilities[0].Definition.Name, Is.EqualTo("Quick Stab"));
            Assert.That(rogueAbilities[3].Definition.Name, Is.EqualTo("Assassinate"));

            Assert.That(clericAbilities.Count, Is.EqualTo(4));
            Assert.That(clericAbilities[0].Definition.Name, Is.EqualTo("Smite"));
            Assert.That(clericAbilities[3].Definition.Name, Is.EqualTo("Divine Judgment"));
        }

        [Test]
        public void CreateObjectivesForContext_ReturnsFullCardSet()
        {
            var objectives = catalog.CreateObjectivesForContext(new ObjectiveContext
            {
                ZoneId = "whispering_forest",
                CardDifficulty = 1,
                PlayerClassId = "warrior",
                RunSeed = 1234
            });

            Assert.That(objectives.Count, Is.EqualTo(25));

            var ids = new HashSet<string>();
            for (int i = 0; i < objectives.Count; i++)
                ids.Add(objectives[i].ObjectiveId);

            Assert.That(ids.Count, Is.EqualTo(25));
            Assert.That(objectives[0].ObjectiveId, Is.EqualTo("forest_kill_pack_01"));
        }

        [Test]
        public void ApplyStarterSkills_UnlocksAuthorSetNodes()
        {
            var progression = new CharacterProgression(BuiltInClasses.Warrior, SkillTreeFactory.CreateWarriorTree());

            catalog.ApplyStarterSkills(progression);

            Assert.That(progression.SkillTree.IsNodeUnlocked("slash"), Is.True);
            Assert.That(progression.SkillTree.IsNodeUnlocked("shield_bash"), Is.True);
            Assert.That(progression.SkillPoints, Is.EqualTo(0));
        }

        [Test]
        public void ApplyStarterSkills_UnlocksRogueAndClericStarterNodes()
        {
            var rogueProgression = new CharacterProgression(BuiltInClasses.Rogue, catalog.CreateSkillTree("rogue"));
            var clericProgression = new CharacterProgression(BuiltInClasses.Cleric, catalog.CreateSkillTree("cleric"));

            catalog.ApplyStarterSkills(rogueProgression);
            catalog.ApplyStarterSkills(clericProgression);

            Assert.That(rogueProgression.SkillTree.IsNodeUnlocked("quick_stab"), Is.True);
            Assert.That(rogueProgression.SkillTree.IsNodeUnlocked("smoke_bomb"), Is.True);
            Assert.That(clericProgression.SkillTree.IsNodeUnlocked("smite"), Is.True);
            Assert.That(clericProgression.SkillTree.IsNodeUnlocked("healing_prayer"), Is.True);
        }

        [Test]
        public void CreateObjectiveFromData_DefeatBossUsesTargetSourceId()
        {
            var data = new ObjectiveDefinitionData
            {
                ObjectiveId = "forest_boss",
                Type = ObjectiveType.DefeatBoss,
                TargetSourceId = "thornbound_alpha"
            };

            var objective = ContentFactory.CreateObjectiveFromData(data);

            Assert.That(objective, Is.Not.Null);
            Assert.That(objective.ObjectiveId, Is.EqualTo("forest_boss"));

            var field = objective.GetType().GetField("targetBossId", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            Assert.That(field.GetValue(objective), Is.EqualTo("thornbound_alpha"));
        }
    }
}
