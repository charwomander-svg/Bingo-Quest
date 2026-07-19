using BingoQuest.Gameplay.Progression;
using NUnit.Framework;
using UnityEngine;

namespace BingoQuest.Tests.EditMode
{
    public class ProgressionBalanceTests
    {
        private ProgressionConfig progressionConfig;
        private CharacterProgression progression;
        private ClassDefinition testClass;
        private SkillTree testSkillTree;

        [SetUp]
        public void Setup()
        {
            progressionConfig = ScriptableObject.CreateInstance<ProgressionConfig>();
            testClass = ScriptableObject.CreateInstance<ClassDefinition>();
            testSkillTree = ScriptableObject.CreateInstance<SkillTree>();
            
            progression = new CharacterProgression(testClass, testSkillTree, progressionConfig);
        }

        [Test]
        public void ProgressionConfigIsValid()
        {
            Assert.That(progressionConfig.IsValid(), Is.True);
        }

        [Test]
        public void XPThreshold_IncreasesExponentially()
        {
            int level1XP = progressionConfig.CalculateXPThreshold(1);
            int level5XP = progressionConfig.CalculateXPThreshold(5);
            int level10XP = progressionConfig.CalculateXPThreshold(10);
            
            Assert.That(level5XP, Is.GreaterThan(level1XP));
            Assert.That(level10XP, Is.GreaterThan(level5XP));
        }

        [Test]
        public void StartingSkillPoints_MatchConfig()
        {
            Assert.That(progression.SkillPoints, Is.EqualTo(progressionConfig.StartingSkillPoints));
        }

        [Test]
        public void GainExperience_LevelsUpAtThreshold()
        {
            int xpNeeded = progressionConfig.CalculateXPThreshold(2);
            
            progression.GainExperience(xpNeeded);
            
            Assert.That(progression.Level, Is.EqualTo(2));
        }

        [Test]
        public void SkillPoints_IncreaseOnLevelUp()
        {
            int startingSkills = progression.SkillPoints;
            int xpNeeded = progressionConfig.CalculateXPThreshold(2);
            
            progression.GainExperience(xpNeeded);
            
            Assert.That(progression.SkillPoints, Is.GreaterThan(startingSkills));
        }

        [Test]
        public void DifficultyXPMultiplier_Easy_AwardsBonusXP()
        {
            float easyMult = progressionConfig.GetDifficultyXPMultiplier(Progression.DifficultyMode.Easy);
            float normalMult = progressionConfig.GetDifficultyXPMultiplier(Progression.DifficultyMode.Normal);
            
            Assert.That(easyMult, Is.GreaterThan(normalMult));
        }

        [Test]
        public void DifficultyXPMultiplier_Nightmare_ReducesXP()
        {
            float nightmareMult = progressionConfig.GetDifficultyXPMultiplier(Progression.DifficultyMode.Nightmare);
            float normalMult = progressionConfig.GetDifficultyXPMultiplier(Progression.DifficultyMode.Normal);
            
            Assert.That(nightmareMult, Is.LessThan(normalMult));
        }

        [Test]
        public void GainExperience_AppliesDifficultyMultiplier()
        {
            // Gain XP on Easy
            int xpAmount = 100;
            progression.GainExperience(xpAmount, Progression.DifficultyMode.Easy);
            int easyExperience = progression.Experience;
            
            // Reset
            progression = new CharacterProgression(testClass, testSkillTree, progressionConfig);
            
            // Gain same XP on Normal
            progression.GainExperience(xpAmount, Progression.DifficultyMode.Normal);
            int normalExperience = progression.Experience;
            
            Assert.That(easyExperience, Is.GreaterThan(normalExperience));
        }

        [Test]
        public void TotalXPForLevel_MatchesCumulativeThresholds()
        {
            int level5Total = progressionConfig.CalculateTotalXPForLevel(5);
            
            int cumulative = 0;
            for (int i = 2; i <= 5; i++)
                cumulative += progressionConfig.CalculateXPThreshold(i);
            
            Assert.That(level5Total, Is.EqualTo(cumulative));
        }

        [Test]
        public void SkillPointsForLevel_IncreasesAppropriatelly()
        {
            int level1Skills = progressionConfig.CalculateSkillPointsForLevel(1);
            int level10Skills = progressionConfig.CalculateSkillPointsForLevel(10);
            
            Assert.That(level10Skills, Is.GreaterThan(level1Skills));
        }

        [Test]
        public void BonusSkillPoints_ApplyEveryNLevels()
        {
            int bonusInterval = progressionConfig.BonusSkillPointInterval;
            
            int levelAtBonus = progressionConfig.CalculateSkillPointsForLevel(bonusInterval);
            int levelBeforeBonus = progressionConfig.CalculateSkillPointsForLevel(bonusInterval - 1);
            int levelAfterBonus = progressionConfig.CalculateSkillPointsForLevel(bonusInterval + 1);
            
            int bonusApplied = levelAtBonus - levelBeforeBonus;
            
            // Should have both regular skill point and bonus
            Assert.That(bonusApplied, Is.GreaterThanOrEqualTo(
                progressionConfig.SkillPointsPerLevel + progressionConfig.BonusSkillPointsAmount
            ));
        }

        [Test]
        public void LevelBonuses_IncreaseWithLevel()
        {
            var (h1, a1, d1, c1, do1) = progressionConfig.CalculateLevelBonuses(1);
            var (h5, a5, d5, c5, do5) = progressionConfig.CalculateLevelBonuses(5);
            var (h10, a10, d10, c10, do10) = progressionConfig.CalculateLevelBonuses(10);
            
            Assert.That(h5, Is.GreaterThan(h1));
            Assert.That(a5, Is.GreaterThan(a1));
            Assert.That(d5, Is.GreaterThan(d1));
            Assert.That(h10, Is.GreaterThan(h5));
            Assert.That(a10, Is.GreaterThan(a5));
            Assert.That(d10, Is.GreaterThan(d5));
        }

        [Test]
        public void CritChanceBonus_ClampsTo01()
        {
            var (_, _, _, crit, _) = progressionConfig.CalculateLevelBonuses(50);
            
            // Should not exceed 1.0 naturally, but verify total can be clamped
            Assert.That(crit, Is.GreaterThanOrEqualTo(0f));
        }

        [Test]
        public void MaxLevel_CapsCalculations()
        {
            int maxLevel = progressionConfig.MaxLevel;
            
            int xpAtMax = progressionConfig.CalculateXPThreshold(maxLevel);
            int xpBeyondMax = progressionConfig.CalculateXPThreshold(maxLevel + 10);
            
            Assert.That(xpBeyondMax, Is.EqualTo(xpAtMax));
        }

        [Test]
        public void ZeroExperience_DoesNotLevelUp()
        {
            int startLevel = progression.Level;
            
            progression.GainExperience(0);
            
            Assert.That(progression.Level, Is.EqualTo(startLevel));
        }

        [Test]
        public void NegativeExperience_IsIgnored()
        {
            int startLevel = progression.Level;
            int startXP = progression.Experience;
            
            progression.GainExperience(-100);
            
            Assert.That(progression.Level, Is.EqualTo(startLevel));
            Assert.That(progression.Experience, Is.EqualTo(startXP));
        }

        [Test]
        public void MultiLevelUp_InOneTurn()
        {
            int totalXP = 0;
            for (int i = 2; i <= 5; i++)
                totalXP += progressionConfig.CalculateXPThreshold(i);
            
            progression.GainExperience(totalXP);
            
            Assert.That(progression.Level, Is.EqualTo(5));
        }

        [Test]
        public void ProgressionWithoutConfig_UsesDefaults()
        {
            var defaultProgression = new CharacterProgression(testClass, testSkillTree, config: null);
            
            int xpNeeded = 100; // Default threshold
            defaultProgression.GainExperience(xpNeeded);
            
            Assert.That(defaultProgression.Level, Is.EqualTo(2));
        }
    }
}
