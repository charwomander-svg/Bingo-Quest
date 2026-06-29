using System;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Progression
{
    /// <summary>
    /// Skill tree node representing a learnable ability or stat boost.
    /// </summary>
    public class SkillNode
    {
        public string NodeId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        // Skill acquisition
        public int PointCost { get; set; } = 1;
        public int MinimumLevel { get; set; } = 1;
        public bool IsLocked { get; set; } = true;

        // Prerequisites (other nodes that must be unlocked first)
        public List<string> Prerequisites { get; set; } = new();

        // Stat rewards when unlocked
        public int HealthBonus { get; set; }
        public int AttackBonus { get; set; }
        public int DefenseBonus { get; set; }
        public float CritChanceBonus { get; set; }
        public float DodgeChanceBonus { get; set; }

        // Ability reward ID (e.g., "fireball", "shield_bash")
        public string UnlocksAbilityId { get; set; }

        public SkillNode() { }

        public SkillNode(string id, string name)
        {
            NodeId = id;
            DisplayName = name;
        }

        public bool CanUnlock(int currentLevel, int availablePoints, Dictionary<string, bool> unlockedNodes)
        {
            // Check level requirement
            if (currentLevel < MinimumLevel)
                return false;

            // Check point cost
            if (availablePoints < PointCost)
                return false;

            // Check all prerequisites are unlocked
            foreach (var prereq in Prerequisites)
            {
                if (!unlockedNodes.TryGetValue(prereq, out bool unlocked) || !unlocked)
                    return false;
            }

            return true;
        }

        public override string ToString() => $"{DisplayName} (Costs {PointCost} point)";
    }

    /// <summary>
    /// Skill tree for a character class.
    /// Defines the progression path with nodes and unlock requirements.
    /// </summary>
    public class SkillTree
    {
        private Dictionary<string, SkillNode> nodes = new();
        private Dictionary<string, bool> unlockedNodes = new();

        public IReadOnlyDictionary<string, SkillNode> Nodes => nodes;
        public IReadOnlyDictionary<string, bool> UnlockedNodes => unlockedNodes;

        public void AddNode(SkillNode node)
        {
            nodes[node.NodeId] = node;
            unlockedNodes[node.NodeId] = false;
        }

        public SkillNode GetNode(string nodeId) =>
            nodes.TryGetValue(nodeId, out var node) ? node : null;

        public bool IsNodeUnlocked(string nodeId) =>
            unlockedNodes.TryGetValue(nodeId, out var unlocked) && unlocked;

        public void UnlockNode(string nodeId)
        {
            if (nodes.ContainsKey(nodeId))
                unlockedNodes[nodeId] = true;
        }

        public void LockNode(string nodeId)
        {
            if (nodes.ContainsKey(nodeId))
                unlockedNodes[nodeId] = false;
        }

        public void Reset()
        {
            foreach (var key in unlockedNodes.Keys)
                unlockedNodes[key] = false;
        }

        public List<SkillNode> GetUnlockedNodes()
        {
            var result = new List<SkillNode>();
            foreach (var (id, unlocked) in unlockedNodes)
            {
                if (unlocked)
                    result.Add(nodes[id]);
            }
            return result;
        }

        public List<string> GetUnlockedNodeIds()
        {
            var result = new List<string>();
            foreach (var (id, unlocked) in unlockedNodes)
                if (unlocked) result.Add(id);
            return result;
        }

        public void ForceUnlock(string nodeId)
        {
            if (!nodes.ContainsKey(nodeId))
                nodes[nodeId] = new SkillNode(nodeId, nodeId);
            unlockedNodes[nodeId] = true;
        }

        public override string ToString()
        {
            int unlockedCount = 0;
            foreach (var unlocked in unlockedNodes.Values)
                if (unlocked) unlockedCount++;

            return $"SkillTree: {unlockedCount}/{nodes.Count} nodes unlocked";
        }
    }

    /// <summary>
    /// Factory for creating class-specific skill trees.
    /// </summary>
    public static class SkillTreeFactory
    {
        public static SkillTree CreateWarriorTree()
        {
            var tree = new SkillTree();

            // Tier 1: Basic attacks
            tree.AddNode(new SkillNode("slash", "Slash")
            {
                DisplayName = "Slash",
                Description = "Basic sword attack",
                PointCost = 1,
                MinimumLevel = 1,
                AttackBonus = 2,
                UnlocksAbilityId = "ability_slash"
            });

            tree.AddNode(new SkillNode("shield_bash", "Shield Bash")
            {
                DisplayName = "Shield Bash",
                Description = "Bash with shield, can stun",
                PointCost = 1,
                MinimumLevel = 1,
                Prerequisites = new() { "slash" },
                DefenseBonus = 3,
                UnlocksAbilityId = "ability_shield_bash"
            });

            // Tier 2: Intermediate
            tree.AddNode(new SkillNode("whirlwind", "Whirlwind")
            {
                DisplayName = "Whirlwind",
                Description = "Spin attack hitting all enemies",
                PointCost = 2,
                MinimumLevel = 5,
                Prerequisites = new() { "slash", "shield_bash" },
                AttackBonus = 5,
                UnlocksAbilityId = "ability_whirlwind"
            });

            tree.AddNode(new SkillNode("defensive_stance", "Defensive Stance")
            {
                DisplayName = "Defensive Stance",
                Description = "+20% defense while active",
                PointCost = 1,
                MinimumLevel = 3,
                Prerequisites = new() { "shield_bash" },
                DefenseBonus = 4,
                UnlocksAbilityId = "ability_defensive_stance"
            });

            // Tier 3: Advanced
            tree.AddNode(new SkillNode("execute", "Execute")
            {
                DisplayName = "Execute",
                Description = "Massive attack with high crit chance",
                PointCost = 3,
                MinimumLevel = 10,
                Prerequisites = new() { "whirlwind" },
                AttackBonus = 10,
                CritChanceBonus = 0.25f,
                UnlocksAbilityId = "ability_execute"
            });

            return tree;
        }

        public static SkillTree CreateMageTree()
        {
            var tree = new SkillTree();

            // Tier 1: Basic spells
            tree.AddNode(new SkillNode("fireball", "Fireball")
            {
                DisplayName = "Fireball",
                Description = "Hurl a ball of fire",
                PointCost = 1,
                MinimumLevel = 1,
                AttackBonus = 3,
                UnlocksAbilityId = "ability_fireball"
            });

            tree.AddNode(new SkillNode("frostbolt", "Frostbolt")
            {
                DisplayName = "Frostbolt",
                Description = "Freeze enemies with ice",
                PointCost = 1,
                MinimumLevel = 1,
                Prerequisites = new() { "fireball" },
                AttackBonus = 2,
                UnlocksAbilityId = "ability_frostbolt"
            });

            // Tier 2: Intermediate
            tree.AddNode(new SkillNode("mana_shield", "Mana Shield")
            {
                DisplayName = "Mana Shield",
                Description = "Absorb damage with mana",
                PointCost = 2,
                MinimumLevel = 5,
                Prerequisites = new() { "fireball", "frostbolt" },
                DefenseBonus = 5,
                UnlocksAbilityId = "ability_mana_shield"
            });

            // Tier 3: Ultimate
            tree.AddNode(new SkillNode("meteor_storm", "Meteor Storm")
            {
                DisplayName = "Meteor Storm",
                Description = "Summon meteors in an area",
                PointCost = 4,
                MinimumLevel = 15,
                Prerequisites = new() { "fireball", "mana_shield" },
                AttackBonus = 12,
                UnlocksAbilityId = "ability_meteor_storm"
            });

            return tree;
        }

        public static SkillTree CreateRangerTree()
        {
            var tree = new SkillTree();

            tree.AddNode(new SkillNode("basic_shot", "Basic Shot")
            {
                DisplayName = "Basic Shot",
                Description = "Quick arrow shot",
                PointCost = 1,
                MinimumLevel = 1,
                AttackBonus = 2,
                CritChanceBonus = 0.05f,
                UnlocksAbilityId = "ability_basic_shot"
            });

            tree.AddNode(new SkillNode("aimed_shot", "Aimed Shot")
            {
                DisplayName = "Aimed Shot",
                Description = "Carefully aimed attack with high crit",
                PointCost = 2,
                MinimumLevel = 5,
                Prerequisites = new() { "basic_shot" },
                AttackBonus = 4,
                CritChanceBonus = 0.15f,
                UnlocksAbilityId = "ability_aimed_shot"
            });

            tree.AddNode(new SkillNode("multishot", "Multishot")
            {
                DisplayName = "Multishot",
                Description = "Fire multiple arrows",
                PointCost = 3,
                MinimumLevel = 10,
                Prerequisites = new() { "aimed_shot" },
                AttackBonus = 6,
                UnlocksAbilityId = "ability_multishot"
            });

            return tree;
        }
    }
}
