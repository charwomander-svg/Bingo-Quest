using BingoQuest.Gameplay.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Content
{
    public sealed class ContentFactory
    {
        public static Ability CreateAbilityFromData(AbilityDefinitionData data)
        {
            if (data == null)
                return null;

            var definition = new AbilityDefinition
            {
                Id = data.AbilityId,
                Name = data.Name,
                Cooldown = data.Cooldown,
                DamageScale = data.DamageScale,
                ElementType = data.ElementType,
                AppliesStatusEffect = data.AppliesStatusEffect,
                StatusEffectType = data.StatusEffectType,
                StatusEffectChance = data.StatusEffectChance
            };

            return new Ability(definition);
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
            return pool?.Abilities ?? new List<AbilityDefinitionData>();
        }

        public int GetEnemyCount => _enemies.Count;
        public int GetBossCount => _bosses.Count;
        public int GetAbilityPoolCount => _abilityPools.Count;
        public int GetObjectivePoolCount => _objectivePools.Count;
    }
}
