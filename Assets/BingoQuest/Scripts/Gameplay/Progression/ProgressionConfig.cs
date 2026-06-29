using UnityEngine;

namespace BingoQuest.Gameplay.Progression
{
    /// <summary>
    /// Centralized progression configuration for XP curves, level scaling, and skill point distribution.
    /// Exposed as ScriptableObject for rapid designer iteration.
    /// </summary>
    [CreateAssetMenu(menuName = "BingoQuest/Progression Config", fileName = "ProgressionConfig")]
    public class ProgressionConfig : ScriptableObject
    {
        [Header("Experience Curve")]
        [SerializeField] private int baseXPThreshold = 100;
        [SerializeField][Range(1.05f, 1.25f)] private float xpScalingPerLevel = 1.1f;
        [SerializeField] private int maxLevel = 50;
        [SerializeField][Range(0.5f, 2f)] private float bonusXPMultiplier = 1.0f;

        [Header("Skill Points")]
        [SerializeField] private int startingSkillPoints = 2;
        [SerializeField] private int skillPointsPerLevel = 1;
        [SerializeField] private int bonusSkillPointInterval = 10; // Every N levels
        [SerializeField] private int bonusSkillPointsAmount = 1;

        [Header("Stat Progression")]
        [SerializeField] private int healthPerLevel = 5;
        [SerializeField] private int attackPerLevel = 1;
        [SerializeField] private int defensePerLevel = 1;
        [SerializeField][Range(0f, 0.1f)] private float critChancePerLevel = 0.005f;
        [SerializeField][Range(0f, 0.1f)] private float dodgeChancePerLevel = 0.003f;

        [Header("Leveling Speed (Difficulty Modifiers)")]
        [SerializeField][Range(0.5f, 2f)] private float easyXPMultiplier = 1.2f;
        [SerializeField][Range(0.5f, 2f)] private float normalXPMultiplier = 1.0f;
        [SerializeField][Range(0.5f, 2f)] private float hardXPMultiplier = 0.8f;
        [SerializeField][Range(0.5f, 2f)] private float nightmareXPMultiplier = 0.6f;

        [Header("Milestone Rewards")]
        [SerializeField] private int levelUpHealthHeal = 0; // Restore this much health on level up
        [SerializeField] private bool resetAbilityCooldowsOnLevelUp = false;

        public int BaseXPThreshold => baseXPThreshold;
        public float XPScalingPerLevel => xpScalingPerLevel;
        public int MaxLevel => maxLevel;
        public float BonusXPMultiplier => bonusXPMultiplier;

        public int StartingSkillPoints => startingSkillPoints;
        public int SkillPointsPerLevel => skillPointsPerLevel;
        public int BonusSkillPointInterval => bonusSkillPointInterval;
        public int BonusSkillPointsAmount => bonusSkillPointsAmount;

        public int HealthPerLevel => healthPerLevel;
        public int AttackPerLevel => attackPerLevel;
        public int DefensePerLevel => defensePerLevel;
        public float CritChancePerLevel => critChancePerLevel;
        public float DodgeChancePerLevel => dodgeChancePerLevel;

        public int LevelUpHealthHeal => levelUpHealthHeal;
        public bool ResetAbilityCooldowsOnLevelUp => resetAbilityCooldowsOnLevelUp;

        /// <summary>Get XP multiplier for a difficulty mode.</summary>
        public float GetDifficultyXPMultiplier(Progression.DifficultyMode mode) => mode switch
        {
            Progression.DifficultyMode.Easy => easyXPMultiplier,
            Progression.DifficultyMode.Normal => normalXPMultiplier,
            Progression.DifficultyMode.Hard => hardXPMultiplier,
            Progression.DifficultyMode.Nightmare => nightmareXPMultiplier,
            _ => normalXPMultiplier
        };

        /// <summary>Calculate XP threshold for reaching a given level.</summary>
        public int CalculateXPThreshold(int level)
        {
            if (level <= 1)
                return 0;
            if (level > maxLevel)
                level = maxLevel;

            return (int)(baseXPThreshold * Mathf.Pow(xpScalingPerLevel, level - 1));
        }

        /// <summary>Calculate total cumulative XP needed to reach a level.</summary>
        public int CalculateTotalXPForLevel(int level)
        {
            if (level <= 1)
                return 0;

            int total = 0;
            for (int i = 2; i <= level && i <= maxLevel; i++)
            {
                total += CalculateXPThreshold(i);
            }
            return total;
        }

        /// <summary>Calculate skill points available at a given level.</summary>
        public int CalculateSkillPointsForLevel(int level)
        {
            int points = startingSkillPoints;
            
            for (int i = 1; i <= level && i <= maxLevel; i++)
            {
                points += skillPointsPerLevel;
                
                // Bonus skill points every N levels
                if (bonusSkillPointInterval > 0 && i > 0 && i % bonusSkillPointInterval == 0)
                    points += bonusSkillPointsAmount;
            }
            
            return points;
        }

        /// <summary>Calculate stat bonuses from leveling alone (not from skills).</summary>
        public (int health, int attack, int defense, float crit, float dodge) CalculateLevelBonuses(int level)
        {
            level = Mathf.Min(level, maxLevel);
            return (
                healthPerLevel * level,
                attackPerLevel * level,
                defensePerLevel * level,
                critChancePerLevel * level,
                dodgeChancePerLevel * level
            );
        }

        /// <summary>Get next level up XP requirement from current level.</summary>
        public int GetNextLevelXPRequired(int currentLevel) => CalculateXPThreshold(currentLevel + 1);

        /// <summary>Validate that progression config is sensible.</summary>
        public bool IsValid()
        {
            if (baseXPThreshold <= 0) return false;
            if (xpScalingPerLevel < 1.0f) return false;
            if (maxLevel < 1) return false;
            if (skillPointsPerLevel < 0) return false;
            return true;
        }
    }

    public partial class Progression
    {
        public enum DifficultyMode
        {
            Easy = 0,
            Normal = 1,
            Hard = 2,
            Nightmare = 3
        }
    }
}
