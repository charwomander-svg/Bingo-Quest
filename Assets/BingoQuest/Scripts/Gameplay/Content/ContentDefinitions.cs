using BingoQuest.Gameplay.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Content
{
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string EnemyId { get; set; }
        public new string name { get; set; }
        public string DisplayName { get; set; }

        public int BaseHealth { get; set; } = 55;
        public int BaseAttack { get; set; } = 9;
        public int BaseDefense { get; set; } = 2;
        public float BaseCritChance { get; set; } = 0.04f;
        public float BaseDodgeChance { get; set; } = 0.03f;

        public int ExperienceReward { get; set; } = 28;
        public float LootDropChance { get; set; } = 0.03f;
        public string LootMaterialId { get; set; } = "enemy_drop";
        public string LootMaterialName { get; set; } = "Scrap Trophy";
    }

    public sealed class BossDefinition : ScriptableObject
    {
        public string BossId { get; set; }
        public new string name { get; set; }
        public string DisplayName { get; set; }

        public int BaseHealth { get; set; } = 200;
        public int BaseAttack { get; set; } = 18;
        public int BaseDefense { get; set; } = 5;
        public float BaseCritChance { get; set; } = 0.12f;
        public float BaseDodgeChance { get; set; } = 0.08f;

        public int ExperienceReward { get; set; } = 150;
        public int LootCount { get; set; } = 3;
        public List<string> PhaseNames { get; set; } = new() { "Phase 1", "Phase 2", "Phase 3" };
    }

    public sealed class AbilityPool : ScriptableObject
    {
        public string PoolId { get; set; }
        public new string name { get; set; }
        public List<AbilityDefinitionData> Abilities { get; set; } = new();
    }

    [System.Serializable]
    public sealed class AbilityDefinitionData
    {
        public string AbilityId { get; set; }
        public string Name { get; set; }
        public float Cooldown { get; set; } = 1.5f;
        public float DamageScale { get; set; } = 1.0f;
        public ElementType ElementType { get; set; } = ElementType.Physical;
        public bool AppliesStatusEffect { get; set; }
        public StatusEffectType StatusEffectType { get; set; }
        public int StatusEffectChance { get; set; } = 50;
    }

    public sealed class ObjectivePool : ScriptableObject
    {
        public string PoolId { get; set; }
        public new string name { get; set; }
        public List<ObjectiveDefinitionData> Objectives { get; set; } = new();
    }

    [System.Serializable]
    public sealed class ObjectiveDefinitionData
    {
        public string ObjectiveId { get; set; }
        public string DisplayName { get; set; }
        public ObjectiveType Type { get; set; }
        public int RequiredProgress { get; set; } = 1;
        public ElementType Element { get; set; } = ElementType.Physical;
        public int MinimumItemRarity { get; set; } = 0;
    }

    public enum ObjectiveType
    {
        Kill,
        Damage,
        CriticalHit,
        UseAbility,
        Dodge,
        ApplyStatusEffect,
        LootItem,
        OpenChest,
        DefeatBoss
    }
}
