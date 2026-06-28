using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Progression;
using NUnit.Framework;
using UnityEngine;

namespace BingoQuest.Tests.PlayMode
{
    public class ProgressionSystemTests
    {
        [Test]
        public void ClassStats_AppliedCorrectly()
        {
            var stats = new CharacterStats();
            ClassStatApplier.ApplyClassStats(stats, BuiltInClasses.Warrior);

            Assert.AreEqual(130, stats.MaxHealth);     // 100 * 1.3
            Assert.AreEqual(11, stats.Attack);         // 10 * 1.1
            Assert.AreEqual(7, stats.Defense);         // 5 * 1.4
        }

        [Test]
        public void ClassStats_RogueHighCrit()
        {
            var stats = new CharacterStats();
            ClassStatApplier.ApplyClassStats(stats, BuiltInClasses.Rogue);

            Assert.Greater(stats.CritChance, 0.2f);
            Assert.Greater(stats.DodgeChance, 0.1f);
        }

        [Test]
        public void SkillNode_CanUnlock_ChecksLevel()
        {
            var node = new SkillNode("test", "Test")
            {
                MinimumLevel = 10,
                PointCost = 1
            };

            bool canUnlock = node.CanUnlock(
                currentLevel: 5,
                availablePoints: 5,
                unlockedNodes: new()
            );

            Assert.IsFalse(canUnlock);
        }

        [Test]
        public void SkillNode_CanUnlock_ChecksPoints()
        {
            var node = new SkillNode("test", "Test")
            {
                MinimumLevel = 1,
                PointCost = 5
            };

            bool canUnlock = node.CanUnlock(
                currentLevel: 1,
                availablePoints: 3,
                unlockedNodes: new()
            );

            Assert.IsFalse(canUnlock);
        }

        [Test]
        public void SkillNode_CanUnlock_ChecksPrerequisites()
        {
            var node = new SkillNode("fireball", "Fireball")
            {
                MinimumLevel = 1,
                PointCost = 1,
                Prerequisites = new() { "mana_shield" }
            };

            var unlockedNodes = new Dictionary<string, bool>
            {
                { "mana_shield", false }
            };

            bool canUnlock = node.CanUnlock(1, 5, unlockedNodes);
            Assert.IsFalse(canUnlock);

            unlockedNodes["mana_shield"] = true;
            canUnlock = node.CanUnlock(1, 5, unlockedNodes);
            Assert.IsTrue(canUnlock);
        }

        [Test]
        public void SkillTree_AddsAndRetrievesNodes()
        {
            var tree = new SkillTree();
            var node = new SkillNode("slash", "Slash");
            tree.AddNode(node);

            Assert.AreEqual(node, tree.GetNode("slash"));
            Assert.IsFalse(tree.IsNodeUnlocked("slash"));
        }

        [Test]
        public void SkillTree_UnlocksNodes()
        {
            var tree = new SkillTree();
            tree.AddNode(new SkillNode("slash", "Slash"));

            tree.UnlockNode("slash");
            Assert.IsTrue(tree.IsNodeUnlocked("slash"));

            tree.LockNode("slash");
            Assert.IsFalse(tree.IsNodeUnlocked("slash"));
        }

        [Test]
        public void SkillTree_ReturnsUnlockedNodes()
        {
            var tree = new SkillTree();
            tree.AddNode(new SkillNode("slash", "Slash"));
            tree.AddNode(new SkillNode("shield", "Shield"));
            tree.AddNode(new SkillNode("whirlwind", "Whirlwind"));

            tree.UnlockNode("slash");
            tree.UnlockNode("shield");

            var unlocked = tree.GetUnlockedNodes();
            Assert.AreEqual(2, unlocked.Count);
        }

        [Test]
        public void CharacterProgression_GainsExperience()
        {
            var tree = SkillTreeFactory.CreateWarriorTree();
            var progression = new CharacterProgression(BuiltInClasses.Warrior, tree);

            progression.GainExperience(50);
            Assert.AreEqual(50, progression.Experience);
            Assert.AreEqual(1, progression.Level);

            progression.GainExperience(100);
            Assert.AreEqual(2, progression.Level);
        }

        [Test]
        public void CharacterProgression_LevelUpGrantsSkillPoints()
        {
            var tree = SkillTreeFactory.CreateWarriorTree();
            var progression = new CharacterProgression(BuiltInClasses.Warrior, tree);

            int startingPoints = progression.SkillPoints;
            progression.GainExperience(1000);  // Level up multiple times

            Assert.Greater(progression.SkillPoints, startingPoints);
        }

        [Test]
        public void CharacterProgression_UnlocksSkills()
        {
            var tree = SkillTreeFactory.CreateWarriorTree();
            var progression = new CharacterProgression(BuiltInClasses.Warrior, tree);

            bool unlocked = progression.TryUnlockSkill("slash");
            Assert.IsTrue(unlocked);
            Assert.IsTrue(tree.IsNodeUnlocked("slash"));
            Assert.AreEqual(1, progression.SkillPoints); // Spent 1 point
        }

        [Test]
        public void CharacterProgression_CannotUnlockTwice()
        {
            var tree = SkillTreeFactory.CreateWarriorTree();
            var progression = new CharacterProgression(BuiltInClasses.Warrior, tree);

            progression.TryUnlockSkill("slash");
            bool secondAttempt = progression.TryUnlockSkill("slash");

            Assert.IsFalse(secondAttempt);
        }

        [Test]
        public void CharacterProgression_ApplieBonusesToStats()
        {
            var tree = SkillTreeFactory.CreateWarriorTree();
            var progression = new CharacterProgression(BuiltInClasses.Warrior, tree);
            var stats = new CharacterStats { Health = 100, MaxHealth = 100 };

            progression.TryUnlockSkill("slash");
            progression.ApplyBonusesToStats(stats);

            // Slash grants +2 attack
            Assert.AreEqual(2, progression.GetStatBonus("attack"));
        }

        [Test]
        public void Character_CreatedWithClassAndProgression()
        {
            var tree = SkillTreeFactory.CreateWarriorTree();
            var character = new Character("p1", "Conan", BuiltInClasses.Warrior, tree);

            Assert.AreEqual("Conan", character.CharacterName);
            Assert.AreEqual("warrior", character.Progression.Class.ClassId);
            Assert.AreEqual(1, character.Progression.Level);
        }

        [Test]
        public void BuiltInClasses_HaveDifferentStats()
        {
            var stats1 = new CharacterStats();
            var stats2 = new CharacterStats();

            ClassStatApplier.ApplyClassStats(stats1, BuiltInClasses.Warrior);
            ClassStatApplier.ApplyClassStats(stats2, BuiltInClasses.Mage);

            Assert.AreNotEqual(stats1.MaxHealth, stats2.MaxHealth);
            Assert.AreNotEqual(stats1.Attack, stats2.Attack);
        }
    }

    public class SkillTreeFactoryTests
    {
        [Test]
        public void WarriorTree_HasSlashNode()
        {
            var tree = SkillTreeFactory.CreateWarriorTree();
            var node = tree.GetNode("slash");

            Assert.IsNotNull(node);
            Assert.AreEqual("Slash", node.DisplayName);
        }

        [Test]
        public void WarriorTree_WhirlwindRequiresSlashAndShield()
        {
            var tree = SkillTreeFactory.CreateWarriorTree();
            var node = tree.GetNode("whirlwind");

            Assert.Contains("slash", node.Prerequisites);
            Assert.Contains("shield_bash", node.Prerequisites);
        }

        [Test]
        public void MageTree_HasFireballNode()
        {
            var tree = SkillTreeFactory.CreateMageTree();
            var node = tree.GetNode("fireball");

            Assert.IsNotNull(node);
        }

        [Test]
        public void RangerTree_HasMultishot()
        {
            var tree = SkillTreeFactory.CreateRangerTree();
            var node = tree.GetNode("multishot");

            Assert.IsNotNull(node);
            Assert.AreEqual(3, node.PointCost);
        }
    }
}
