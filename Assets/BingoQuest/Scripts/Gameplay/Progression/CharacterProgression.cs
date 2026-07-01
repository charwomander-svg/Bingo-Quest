using BingoQuest.Gameplay.Combat;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Progression
{
    /// <summary>
    /// Tracks character progression: level, skill points, unlocked abilities, and stat bonuses.
    /// </summary>
    public class CharacterProgression
    {
        // Core progression
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; } = 0;
        public int SkillPoints { get; private set; } = 0;
        public int LevelUpThreshold { get; private set; } = 100;

        // Permanent unlocks
        public ClassDefinition Class { get; private set; }
        public SkillTree SkillTree { get; private set; }
        private ProgressionConfig progressionConfig;
        
        // Bonuses from progression
        private Dictionary<string, int> statBonuses = new()
        {
            { "health", 0 },
            { "attack", 0 },
            { "defense", 0 },
        };
        private Dictionary<string, float> floatBonuses = new()
        {
            { "crit_chance", 0 },
            { "dodge_chance", 0 },
        };

        public event Action<int> OnLevelUp;
        public event Action<int> OnSkillPointsChanged;
        public event Action<string> OnAbilityUnlocked;

        public CharacterProgression(ClassDefinition classDefinition, SkillTree skillTree, ProgressionConfig config = null)
        {
            Class = classDefinition;
            SkillTree = skillTree;
            progressionConfig = config;
            
            if (progressionConfig != null)
                SkillPoints = progressionConfig.StartingSkillPoints;
            else
                SkillPoints = 2; // Fallback default
        }

        /// <summary>Apply experience and check for level-up.</summary>
        public void GainExperience(int amount, Progression.DifficultyMode difficulty = Progression.DifficultyMode.Normal)
        {
            if (amount <= 0)
                return;

            // Apply difficulty multiplier if config exists
            if (progressionConfig != null)
            {
                float multiplier = progressionConfig.GetDifficultyXPMultiplier(difficulty);
                amount = (int)(amount * multiplier);
            }

            Experience += amount;

            while (Experience >= LevelUpThreshold && Level < (progressionConfig?.MaxLevel ?? 999))
            {
                Experience -= LevelUpThreshold;
                Level++;
                
                if (progressionConfig != null)
                {
                    // Add skill points from config
                    SkillPoints += progressionConfig.SkillPointsPerLevel;
                    
                    // Check for bonus skill points
                    if (progressionConfig.BonusSkillPointInterval > 0 && 
                        Level % progressionConfig.BonusSkillPointInterval == 0)
                    {
                        SkillPoints += progressionConfig.BonusSkillPointsAmount;
                    }
                    
                    LevelUpThreshold = progressionConfig.GetNextLevelXPRequired(Level);
                }
                else
                {
                    SkillPoints += 1;
                    LevelUpThreshold = (int)(100 * Mathf.Pow(1.1f, Level - 1));
                }

                OnLevelUp?.Invoke(Level);
                OnSkillPointsChanged?.Invoke(SkillPoints);

                Debug.Log($"<b>LEVEL UP!</b> Now level {Level}. Skill points available: {SkillPoints}");
            }
        }

        /// <summary>Unlock a skill node from the skill tree.</summary>
        public bool TryUnlockSkill(string nodeId)
        {
            var node = SkillTree.GetNode(nodeId);
            if (node == null)
            {
                Debug.LogError($"Skill node '{nodeId}' not found");
                return false;
            }

            // Check if already unlocked
            if (SkillTree.IsNodeUnlocked(nodeId))
            {
                Debug.LogWarning($"Skill '{nodeId}' is already unlocked");
                return false;
            }

            // Check requirements
            if (!node.CanUnlock(Level, SkillPoints, SkillTree.UnlockedNodes))
            {
                Debug.LogWarning($"Cannot unlock '{nodeId}': requirements not met");
                return false;
            }

            // Unlock and apply bonuses
            SkillTree.UnlockNode(nodeId);
            SkillPoints -= node.PointCost;

            statBonuses["health"] += node.HealthBonus;
            statBonuses["attack"] += node.AttackBonus;
            statBonuses["defense"] += node.DefenseBonus;
            floatBonuses["crit_chance"] += node.CritChanceBonus;
            floatBonuses["dodge_chance"] += node.DodgeChanceBonus;

            OnSkillPointsChanged?.Invoke(SkillPoints);

            if (!string.IsNullOrEmpty(node.UnlocksAbilityId))
                OnAbilityUnlocked?.Invoke(node.UnlocksAbilityId);

            Debug.Log($"<b>Skill Unlocked:</b> {node.DisplayName}");
            return true;
        }

        /// <summary>Apply all progression bonuses to character stats.</summary>
        public void ApplyBonusesToStats(CharacterStats stats)
        {
            stats.MaxHealth += statBonuses["health"];
            stats.Health = stats.MaxHealth; // Heal on progression
            stats.Attack += statBonuses["attack"];
            stats.Defense += statBonuses["defense"];
            stats.CritChance = Mathf.Clamp01(stats.CritChance + floatBonuses["crit_chance"]);
            stats.DodgeChance = Mathf.Clamp01(stats.DodgeChance + floatBonuses["dodge_chance"]);
        }

        public int GetStatBonus(string bonusType) =>
            statBonuses.TryGetValue(bonusType, out var bonus) ? bonus : 0;

        public float GetFloatBonus(string bonusType) =>
            floatBonuses.TryGetValue(bonusType, out var bonus) ? bonus : 0;

        public void ForceSetLevel(int level, int experience)
        {
            Level = level;
            Experience = experience;
            LevelUpThreshold = (int)(100 * Mathf.Pow(1.1f, Level - 1));
        }

        public void ForceSetSkillPoints(int points) => SkillPoints = points;

        public void RecalculateBonusesFromUnlockedSkills()
        {
            statBonuses["health"] = 0;
            statBonuses["attack"] = 0;
            statBonuses["defense"] = 0;
            floatBonuses["crit_chance"] = 0f;
            floatBonuses["dodge_chance"] = 0f;

            var unlocked = SkillTree.GetUnlockedNodes();
            for (int i = 0; i < unlocked.Count; i++)
            {
                var node = unlocked[i];
                statBonuses["health"] += node.HealthBonus;
                statBonuses["attack"] += node.AttackBonus;
                statBonuses["defense"] += node.DefenseBonus;
                floatBonuses["crit_chance"] += node.CritChanceBonus;
                floatBonuses["dodge_chance"] += node.DodgeChanceBonus;
            }
        }

        public int GetExperienceForNextLevel() => LevelUpThreshold - Experience;

        public float GetLevelProgress() => (float)Experience / LevelUpThreshold;

        public override string ToString() =>
            $"Level {Level} {Class.ClassName} | EXP: {Experience}/{LevelUpThreshold} | " +
            $"Skill Points: {SkillPoints} | Stats: +{statBonuses["attack"]} ATK, " +
            $"+{statBonuses["defense"]} DEF, +{statBonuses["health"]} HP";
    }

    /// <summary>
    /// Manages a complete character with stats, progression, and loadout.
    /// </summary>
    public class Character
    {
        public string CharacterId { get; set; }
        public string CharacterName { get; set; }

        public CharacterStats Stats { get; private set; }
        public CharacterProgression Progression { get; private set; }
        public ActionBar ActionBar { get; private set; }

        public Character(string id, string name, ClassDefinition classDefinition, SkillTree skillTree)
        {
            CharacterId = id;
            CharacterName = name;

            Stats = new CharacterStats();
            ClassStatApplier.ApplyClassStats(Stats, classDefinition);

            Progression = new CharacterProgression(classDefinition, skillTree);
            ActionBar = new ActionBar();

            // Set class passive
            if (!string.IsNullOrEmpty(classDefinition.PassiveAbilityId))
                ActionBar.PassiveAbilityId = classDefinition.PassiveAbilityId;
        }

        public override string ToString() =>
            $"{CharacterName} ({Progression.Class.ClassName}) - {Progression}";
    }
}
