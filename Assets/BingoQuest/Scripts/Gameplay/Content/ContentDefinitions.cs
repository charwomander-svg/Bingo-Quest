using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Objectives;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Content
{
    [CreateAssetMenu(menuName = "BingoQuest/Content/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string enemyId;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private string regionId;

        [SerializeField] private int baseHealth = 55;
        [SerializeField] private int baseAttack = 9;
        [SerializeField] private int baseDefense = 2;
        [SerializeField] private float baseCritChance = 0.04f;
        [SerializeField] private float baseDodgeChance = 0.03f;

        [SerializeField] private int experienceReward = 28;
        [SerializeField] private float lootDropChance = 0.03f;
        [SerializeField] private string lootMaterialId = "enemy_drop";
        [SerializeField] private string lootMaterialName = "Scrap Trophy";

        [SerializeField] private Color tint = Color.white;
        [SerializeField] private float modelScale = 1.2f;

        public string EnemyId { get => enemyId; set => enemyId = value; }
        public string DisplayName { get => displayName; set => displayName = value; }
        public string Description { get => description; set => description = value; }
        public string RegionId { get => regionId; set => regionId = value; }

        public int BaseHealth { get => baseHealth; set => baseHealth = value; }
        public int BaseAttack { get => baseAttack; set => baseAttack = value; }
        public int BaseDefense { get => baseDefense; set => baseDefense = value; }
        public float BaseCritChance { get => baseCritChance; set => baseCritChance = value; }
        public float BaseDodgeChance { get => baseDodgeChance; set => baseDodgeChance = value; }

        public int ExperienceReward { get => experienceReward; set => experienceReward = value; }
        public float LootDropChance { get => lootDropChance; set => lootDropChance = value; }
        public string LootMaterialId { get => lootMaterialId; set => lootMaterialId = value; }
        public string LootMaterialName { get => lootMaterialName; set => lootMaterialName = value; }

        public Color Tint { get => tint; set => tint = value; }
        public float ModelScale { get => modelScale; set => modelScale = value; }
    }

    [CreateAssetMenu(menuName = "BingoQuest/Content/Boss Definition", fileName = "BossDefinition")]
    public sealed class BossDefinition : ScriptableObject
    {
        [SerializeField] private string bossId;
        [SerializeField] private string displayName;
        [SerializeField] private string subtitle;
        [SerializeField] private string regionId;

        [SerializeField] private int baseHealth = 200;
        [SerializeField] private int baseAttack = 18;
        [SerializeField] private int baseDefense = 5;
        [SerializeField] private float baseCritChance = 0.12f;
        [SerializeField] private float baseDodgeChance = 0.08f;

        [SerializeField] private int experienceReward = 150;
        [SerializeField] private int lootCount = 3;
        [SerializeField] private List<string> phaseNames = new List<string> { "Phase 1", "Phase 2", "Phase 3" };

        [SerializeField] private Color tint = new Color(0.95f, 0.65f, 0.3f, 1f);
        [SerializeField] private float modelScale = 2.4f;

        public string BossId { get => bossId; set => bossId = value; }
        public string DisplayName { get => displayName; set => displayName = value; }
        public string Subtitle { get => subtitle; set => subtitle = value; }
        public string RegionId { get => regionId; set => regionId = value; }

        public int BaseHealth { get => baseHealth; set => baseHealth = value; }
        public int BaseAttack { get => baseAttack; set => baseAttack = value; }
        public int BaseDefense { get => baseDefense; set => baseDefense = value; }
        public float BaseCritChance { get => baseCritChance; set => baseCritChance = value; }
        public float BaseDodgeChance { get => baseDodgeChance; set => baseDodgeChance = value; }

        public int ExperienceReward { get => experienceReward; set => experienceReward = value; }
        public int LootCount { get => lootCount; set => lootCount = value; }
        public List<string> PhaseNames { get => phaseNames; set => phaseNames = value; }

        public Color Tint { get => tint; set => tint = value; }
        public float ModelScale { get => modelScale; set => modelScale = value; }
    }

    [CreateAssetMenu(menuName = "BingoQuest/Content/Ability Pool", fileName = "AbilityPool")]
    public sealed class AbilityPool : ScriptableObject
    {
        [SerializeField] private string poolId;
        [SerializeField] private string classId;
        [SerializeField] private List<AbilityDefinitionData> abilities = new List<AbilityDefinitionData>();

        public string PoolId { get => poolId; set => poolId = value; }
        public string ClassId { get => classId; set => classId = value; }
        public List<AbilityDefinitionData> Abilities { get => abilities; set => abilities = value; }
    }

    [System.Serializable]
    public sealed class AbilityDefinitionData
    {
        [SerializeField] private string abilityId;
        [SerializeField] private string abilityName;
        [SerializeField] private string description;
        [SerializeField] private float cooldown = 1.5f;
        [SerializeField] private float damageScale = 1.0f;
        [SerializeField] private ElementType elementType = ElementType.Physical;
        [SerializeField] private AbilitySlot preferredSlot = AbilitySlot.Primary;
        [SerializeField] private bool appliesStatusEffect;
        [SerializeField] private StatusEffectType statusEffectType;
        [SerializeField] private int statusEffectChance = 50;
        [SerializeField] private bool isAoe;
        [SerializeField] private float aoeRadius = 5f;

        public string AbilityId { get => abilityId; set => abilityId = value; }
        public string Name { get => abilityName; set => abilityName = value; }
        public string Description { get => description; set => description = value; }
        public float Cooldown { get => cooldown; set => cooldown = value; }
        public float DamageScale { get => damageScale; set => damageScale = value; }
        public ElementType ElementType { get => elementType; set => elementType = value; }
        public AbilitySlot PreferredSlot { get => preferredSlot; set => preferredSlot = value; }
        public bool AppliesStatusEffect { get => appliesStatusEffect; set => appliesStatusEffect = value; }
        public StatusEffectType StatusEffectType { get => statusEffectType; set => statusEffectType = value; }
        public int StatusEffectChance { get => statusEffectChance; set => statusEffectChance = value; }
        public bool IsAOE { get => isAoe; set => isAoe = value; }
        public float AOERadius { get => aoeRadius; set => aoeRadius = value; }
    }

    [CreateAssetMenu(menuName = "BingoQuest/Content/Objective Pool", fileName = "ObjectivePool")]
    public sealed class ObjectivePool : ScriptableObject
    {
        [SerializeField] private string poolId;
        [SerializeField] private string regionId;
        [SerializeField] private List<ObjectiveDefinitionData> objectives = new List<ObjectiveDefinitionData>();

        public string PoolId { get => poolId; set => poolId = value; }
        public string RegionId { get => regionId; set => regionId = value; }
        public List<ObjectiveDefinitionData> Objectives { get => objectives; set => objectives = value; }
    }

    [System.Serializable]
    public sealed class ObjectiveDefinitionData
    {
        [SerializeField] private string objectiveId;
        [SerializeField] private string displayName;
        [SerializeField] private ObjectiveType type;
        [SerializeField] private int requiredProgress = 1;
        [SerializeField] private ElementType element = ElementType.Physical;
        [SerializeField] private int minimumItemRarity;
        [SerializeField] private string targetSourceId = "ANY";

        public string ObjectiveId { get => objectiveId; set => objectiveId = value; }
        public string DisplayName { get => displayName; set => displayName = value; }
        public ObjectiveType Type { get => type; set => type = value; }
        public int RequiredProgress { get => requiredProgress; set => requiredProgress = value; }
        public ElementType Element { get => element; set => element = value; }
        public int MinimumItemRarity { get => minimumItemRarity; set => minimumItemRarity = value; }
        public string TargetSourceId { get => targetSourceId; set => targetSourceId = value; }
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
