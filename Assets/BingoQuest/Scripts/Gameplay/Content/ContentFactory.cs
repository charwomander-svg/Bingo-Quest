using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Objectives;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Content
{
    public static class ContentFactory
    {
        public static Ability CreateAbilityFromData(AbilityDefinitionData data)
        {
            return CreateAbilityFromData(data, data != null ? data.PreferredSlot : AbilitySlot.Primary, null);
        }

        public static Ability CreateAbilityFromData(AbilityDefinitionData data, AbilitySlot slot, AbilityConfig config = null)
        {
            if (data == null)
                return null;

            var definition = new AbilityDefinition
            {
                Id = data.AbilityId,
                Name = data.Name,
                Description = data.Description,
                Cooldown = data.Cooldown,
                DamageScale = data.DamageScale,
                ElementType = data.ElementType,
                AppliesStatusEffect = data.AppliesStatusEffect,
                StatusEffectType = data.StatusEffectType,
                StatusEffectChance = data.StatusEffectChance,
                IsAOE = data.IsAOE,
                AOERadius = data.AOERadius
            };

            return new Ability(definition, slot, config);
        }

        public static List<IObjective> CreateObjectivesFromPool(ObjectivePool pool, int desiredCount = 25)
        {
            var results = new List<IObjective>();
            if (pool == null || pool.Objectives == null || pool.Objectives.Count == 0 || desiredCount <= 0)
                return results;

            for (int i = 0; i < desiredCount; i++)
            {
                var template = pool.Objectives[i % pool.Objectives.Count];
                var objectiveId = template != null && !string.IsNullOrWhiteSpace(template.ObjectiveId)
                    ? $"{template.ObjectiveId}_{i + 1:00}"
                    : $"objective_{i + 1:00}";
                var objective = CreateObjectiveFromData(template, objectiveId);
                if (objective != null)
                    results.Add(objective);
            }

            return results;
        }

        public static IObjective CreateObjectiveFromData(ObjectiveDefinitionData data, string objectiveIdOverride = null)
        {
            if (data == null)
                return null;

            string objectiveId = !string.IsNullOrWhiteSpace(objectiveIdOverride)
                ? objectiveIdOverride
                : data.ObjectiveId;
            if (string.IsNullOrWhiteSpace(objectiveId))
                return null;

            switch (data.Type)
            {
                case ObjectiveType.Kill:
                    return new KillEnemiesObjective(objectiveId, data.RequiredProgress);
                case ObjectiveType.Damage:
                    return new DealDamageObjective(objectiveId, data.RequiredProgress);
                case ObjectiveType.CriticalHit:
                    return new CriticalHitsObjective(objectiveId, data.RequiredProgress);
                case ObjectiveType.UseAbility:
                    return new UseAbilitiesObjective(objectiveId, data.RequiredProgress);
                case ObjectiveType.Dodge:
                    return new DodgeActionsObjective(objectiveId, data.RequiredProgress);
                case ObjectiveType.ApplyStatusEffect:
                    return new ApplyStatusEffectsObjective(objectiveId, data.RequiredProgress, data.Element);
                case ObjectiveType.LootItem:
                    return new LootItemsObjective(objectiveId, data.RequiredProgress, data.MinimumItemRarity);
                case ObjectiveType.OpenChest:
                    return new OpenChestsObjective(objectiveId, data.RequiredProgress);
                case ObjectiveType.DefeatBoss:
                    return new DefeatBossObjective(objectiveId, string.IsNullOrWhiteSpace(data.TargetSourceId) ? "ANY" : data.TargetSourceId);
                default:
                    Debug.LogWarning($"Unsupported objective type '{data.Type}' for '{data.ObjectiveId}'.");
                    return null;
            }
        }

        public static CharacterStats CreateStatsFromEnemy(EnemyDefinition def)
        {
            if (def == null)
                return null;

            return new CharacterStats
            {
                MaxHealth = def.BaseHealth,
                Health = def.BaseHealth,
                Attack = def.BaseAttack,
                Defense = def.BaseDefense,
                CritChance = def.BaseCritChance,
                DodgeChance = def.BaseDodgeChance
            };
        }

        public static CharacterStats CreateStatsFromBoss(BossDefinition def)
        {
            if (def == null)
                return null;

            return new CharacterStats
            {
                MaxHealth = def.BaseHealth,
                Health = def.BaseHealth,
                Attack = def.BaseAttack,
                Defense = def.BaseDefense,
                CritChance = def.BaseCritChance,
                DodgeChance = def.BaseDodgeChance
            };
        }
    }

    public sealed class ContentLibrary
    {
        private readonly Dictionary<string, EnemyDefinition> _enemies = new();
        private readonly Dictionary<string, BossDefinition> _bosses = new();
        private readonly Dictionary<string, AbilityPool> _abilityPools = new();
        private readonly Dictionary<string, ObjectivePool> _objectivePools = new();

        public void RegisterEnemy(EnemyDefinition def)
        {
            if (def != null && !string.IsNullOrWhiteSpace(def.EnemyId))
                _enemies[def.EnemyId] = def;
        }

        public void RegisterBoss(BossDefinition def)
        {
            if (def != null && !string.IsNullOrWhiteSpace(def.BossId))
                _bosses[def.BossId] = def;
        }

        public void RegisterAbilityPool(AbilityPool pool)
        {
            if (pool != null && !string.IsNullOrWhiteSpace(pool.PoolId))
                _abilityPools[pool.PoolId] = pool;
        }

        public void RegisterObjectivePool(ObjectivePool pool)
        {
            if (pool != null && !string.IsNullOrWhiteSpace(pool.PoolId))
                _objectivePools[pool.PoolId] = pool;
        }

        public EnemyDefinition GetEnemy(string enemyId) =>
            !string.IsNullOrEmpty(enemyId) && _enemies.TryGetValue(enemyId, out var def) ? def : null;

        public BossDefinition GetBoss(string bossId) =>
            !string.IsNullOrEmpty(bossId) && _bosses.TryGetValue(bossId, out var def) ? def : null;

        public AbilityPool GetAbilityPool(string poolId) =>
            !string.IsNullOrEmpty(poolId) && _abilityPools.TryGetValue(poolId, out var pool) ? pool : null;

        public ObjectivePool GetObjectivePool(string poolId) =>
            !string.IsNullOrEmpty(poolId) && _objectivePools.TryGetValue(poolId, out var pool) ? pool : null;

        public List<AbilityDefinitionData> GetAbilitiesFromPool(string poolId)
        {
            var pool = GetAbilityPool(poolId);
            return pool != null ? new List<AbilityDefinitionData>(pool.Abilities) : new List<AbilityDefinitionData>();
        }

        public int GetEnemyCount => _enemies.Count;
        public int GetBossCount => _bosses.Count;
        public int GetAbilityPoolCount => _abilityPools.Count;
        public int GetObjectivePoolCount => _objectivePools.Count;
    }
}
