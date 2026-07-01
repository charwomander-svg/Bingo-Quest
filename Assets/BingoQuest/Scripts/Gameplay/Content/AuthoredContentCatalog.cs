using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Objectives;
using BingoQuest.Gameplay.Progression;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Content
{
    public sealed class RegionContentProfile
    {
        public string RegionId { get; set; }
        public string ObjectivePoolId { get; set; }
        public string BossId { get; set; }
        public int BossSpawnKillRequirement { get; set; } = 8;
        public string RewardCurrencyId { get; set; } = "hero_medals";
        public int RewardCurrencyAmount { get; set; } = 5;
        public List<string> EnemyIds { get; } = new List<string>();
    }

    public sealed class ClassContentProfile
    {
        public string ClassId { get; set; }
        public string AbilityPoolId { get; set; }
        public List<string> StarterSkillNodeIds { get; } = new List<string>();
        public List<string> AutoUnlockOrder { get; } = new List<string>();
    }

    public sealed class AuthoredContentCatalog
    {
        private readonly ContentLibrary _library = new ContentLibrary();
        private readonly Dictionary<string, RegionContentProfile> _regionProfiles = new Dictionary<string, RegionContentProfile>(StringComparer.Ordinal);
        private readonly Dictionary<string, ClassContentProfile> _classProfiles = new Dictionary<string, ClassContentProfile>(StringComparer.Ordinal);
        private readonly Dictionary<string, AbilityDefinitionData> _abilitiesById = new Dictionary<string, AbilityDefinitionData>(StringComparer.Ordinal);

        public ContentLibrary Library => _library;

        public static AuthoredContentCatalog CreateDefault()
        {
            var catalog = new AuthoredContentCatalog();
            catalog.RegisterDefaultAbilityPools();
            catalog.RegisterDefaultEnemies();
            catalog.RegisterDefaultBosses();
            catalog.RegisterDefaultObjectivePools();
            catalog.RegisterDefaultProfiles();
            return catalog;
        }

        public RegionContentProfile GetRegionProfile(string regionId)
        {
            return !string.IsNullOrWhiteSpace(regionId) && _regionProfiles.TryGetValue(regionId, out var profile)
                ? profile
                : null;
        }

        public BossDefinition GetBossForRegion(string regionId)
        {
            var profile = GetRegionProfile(regionId);
            return profile == null ? null : _library.GetBoss(profile.BossId);
        }

        public EnemyDefinition GetEnemyForRegion(string regionId, int sequenceIndex)
        {
            var profile = GetRegionProfile(regionId);
            if (profile == null || profile.EnemyIds.Count == 0)
                return null;

            int safeIndex = Mathf.Abs(sequenceIndex) % profile.EnemyIds.Count;
            return _library.GetEnemy(profile.EnemyIds[safeIndex]);
        }

        public ClassDefinition GetClassDefinition(string classId)
        {
            switch (classId)
            {
                case "mage":
                    return BuiltInClasses.Mage;
                case "ranger":
                    return BuiltInClasses.Ranger;
                case "rogue":
                    return BuiltInClasses.Rogue;
                case "cleric":
                    return BuiltInClasses.Cleric;
                case "warrior":
                default:
                    return BuiltInClasses.Warrior;
            }
        }

        public SkillTree CreateSkillTree(string classId)
        {
            switch (classId)
            {
                case "mage":
                    return SkillTreeFactory.CreateMageTree();
                case "ranger":
                    return SkillTreeFactory.CreateRangerTree();
                case "rogue":
                    return SkillTreeFactory.CreateRogueTree();
                case "cleric":
                    return SkillTreeFactory.CreateClericTree();
                case "warrior":
                default:
                    return SkillTreeFactory.CreateWarriorTree();
            }
        }

        public List<Ability> CreateStartingAbilities(string classId, AbilityConfig config = null)
        {
            var result = new List<Ability>();
            if (!_classProfiles.TryGetValue(classId, out var profile))
                return result;

            var pool = _library.GetAbilityPool(profile.AbilityPoolId);
            if (pool == null)
                return result;

            for (int i = 0; i < pool.Abilities.Count; i++)
                result.Add(ContentFactory.CreateAbilityFromData(pool.Abilities[i], pool.Abilities[i].PreferredSlot, config));

            return result;
        }

        public bool TryCreateAbility(string abilityId, out Ability ability, AbilityConfig config = null)
        {
            ability = null;
            if (string.IsNullOrWhiteSpace(abilityId) || !_abilitiesById.TryGetValue(abilityId, out var data))
                return false;

            ability = ContentFactory.CreateAbilityFromData(data, data.PreferredSlot, config);
            return ability != null;
        }

        public List<IObjective> CreateObjectivesForContext(ObjectiveContext context)
        {
            var profile = GetRegionProfile(context.ZoneId);
            if (profile == null)
                return new List<IObjective>();

            var pool = _library.GetObjectivePool(profile.ObjectivePoolId);
            return ContentFactory.CreateObjectivesFromPool(pool, 25);
        }

        public int GetBossSpawnKillRequirement(string regionId)
        {
            var profile = GetRegionProfile(regionId);
            return profile != null ? Mathf.Max(1, profile.BossSpawnKillRequirement) : 8;
        }

        public void ApplyStarterSkills(CharacterProgression progression)
        {
            if (progression == null || progression.Class == null)
                return;

            if (!_classProfiles.TryGetValue(progression.Class.ClassId, out var profile))
                return;

            for (int i = 0; i < profile.StarterSkillNodeIds.Count; i++)
                progression.TryUnlockSkill(profile.StarterSkillNodeIds[i]);
        }

        public void ApplyAutoUnlocks(CharacterProgression progression, int maxUnlocks)
        {
            if (progression == null || progression.Class == null || maxUnlocks <= 0)
                return;

            if (!_classProfiles.TryGetValue(progression.Class.ClassId, out var profile))
                return;

            int applied = 0;
            for (int i = 0; i < profile.AutoUnlockOrder.Count && applied < maxUnlocks; i++)
            {
                if (progression.TryUnlockSkill(profile.AutoUnlockOrder[i]))
                    applied++;
            }
        }

        public ItemDefinition CreateEnemyLootDefinition(EnemyDefinition definition)
        {
            if (definition == null)
                return null;

            return new ItemDefinition
            {
                ItemId = definition.LootMaterialId,
                DisplayName = definition.LootMaterialName,
                Type = ItemType.Material,
                BasePower = Mathf.Max(1, definition.BaseAttack / 3)
            };
        }

        public ItemDefinition CreateBossRewardDefinition(BossDefinition definition)
        {
            if (definition == null)
                return null;

            return new ItemDefinition
            {
                ItemId = $"{definition.BossId}_relic",
                DisplayName = $"{definition.DisplayName} Relic",
                Type = ItemType.Accessory,
                BasePower = Mathf.Max(10, definition.BaseAttack)
            };
        }

        public int GetRegionRewardAmount(string regionId)
        {
            var profile = GetRegionProfile(regionId);
            return profile != null ? Mathf.Max(0, profile.RewardCurrencyAmount) : 0;
        }

        public string GetRegionRewardCurrency(string regionId)
        {
            var profile = GetRegionProfile(regionId);
            return profile != null ? profile.RewardCurrencyId : "hero_medals";
        }

        private void RegisterDefaultAbilityPools()
        {
            RegisterAbilityPool(CreateAbilityPool(
                "warrior_loadout",
                "warrior",
                new AbilityDefinitionData
                {
                    AbilityId = "ability_slash",
                    Name = "Slash",
                    Description = "Fast cleave that opens enemy defenses.",
                    Cooldown = 0.6f,
                    DamageScale = 1.3f,
                    PreferredSlot = AbilitySlot.Primary
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_shield_bash",
                    Name = "Shield Bash",
                    Description = "Heavy bash that disrupts the target.",
                    Cooldown = 1.4f,
                    DamageScale = 1.1f,
                    PreferredSlot = AbilitySlot.Secondary,
                    AppliesStatusEffect = true,
                    StatusEffectType = StatusEffectType.Stun,
                    StatusEffectChance = 55
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_whirlwind",
                    Name = "Whirlwind",
                    Description = "Spin through nearby foes.",
                    Cooldown = 2.2f,
                    DamageScale = 1.8f,
                    PreferredSlot = AbilitySlot.Tertiary,
                    IsAOE = true,
                    AOERadius = 4.5f
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_execute",
                    Name = "Execute",
                    Description = "A finishing strike for elite targets.",
                    Cooldown = 5.5f,
                    DamageScale = 3.0f,
                    PreferredSlot = AbilitySlot.Ultimate
                }));

            RegisterAbilityPool(CreateAbilityPool(
                "mage_loadout",
                "mage",
                new AbilityDefinitionData
                {
                    AbilityId = "ability_fireball",
                    Name = "Fireball",
                    Description = "Explosive fire projectile.",
                    Cooldown = 1.0f,
                    DamageScale = 1.4f,
                    ElementType = ElementType.Fire,
                    PreferredSlot = AbilitySlot.Primary,
                    AppliesStatusEffect = true,
                    StatusEffectType = StatusEffectType.Burn,
                    StatusEffectChance = 65
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_frostbolt",
                    Name = "Frostbolt",
                    Description = "Chilling lance that can freeze.",
                    Cooldown = 1.6f,
                    DamageScale = 1.5f,
                    ElementType = ElementType.Frost,
                    PreferredSlot = AbilitySlot.Secondary,
                    AppliesStatusEffect = true,
                    StatusEffectType = StatusEffectType.Freeze,
                    StatusEffectChance = 55
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_mana_shield",
                    Name = "Mana Shield",
                    Description = "Protective ward that rewards careful casting.",
                    Cooldown = 2.8f,
                    DamageScale = 0.9f,
                    ElementType = ElementType.Holy,
                    PreferredSlot = AbilitySlot.Tertiary
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_meteor_storm",
                    Name = "Meteor Storm",
                    Description = "Rain fire over a large area.",
                    Cooldown = 6.0f,
                    DamageScale = 3.2f,
                    ElementType = ElementType.Fire,
                    PreferredSlot = AbilitySlot.Ultimate,
                    IsAOE = true,
                    AOERadius = 6.5f
                }));

            RegisterAbilityPool(CreateAbilityPool(
                "ranger_loadout",
                "ranger",
                new AbilityDefinitionData
                {
                    AbilityId = "ability_basic_shot",
                    Name = "Basic Shot",
                    Description = "Reliable ranged pressure.",
                    Cooldown = 0.5f,
                    DamageScale = 1.15f,
                    PreferredSlot = AbilitySlot.Primary
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_aimed_shot",
                    Name = "Aimed Shot",
                    Description = "Precise high-crit arrow.",
                    Cooldown = 1.8f,
                    DamageScale = 1.8f,
                    PreferredSlot = AbilitySlot.Secondary
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_multishot",
                    Name = "Multishot",
                    Description = "Loose a spread of arrows.",
                    Cooldown = 2.4f,
                    DamageScale = 1.6f,
                    PreferredSlot = AbilitySlot.Tertiary,
                    IsAOE = true,
                    AOERadius = 5.5f
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_hawkeye_barrage",
                    Name = "Hawkeye Barrage",
                    Description = "Rain arrows on priority targets.",
                    Cooldown = 5.8f,
                    DamageScale = 2.8f,
                    PreferredSlot = AbilitySlot.Ultimate
                }));

            RegisterAbilityPool(CreateAbilityPool(
                "rogue_loadout",
                "rogue",
                new AbilityDefinitionData
                {
                    AbilityId = "ability_quick_stab",
                    Name = "Quick Stab",
                    Description = "Rapid dagger combo with high crit pressure.",
                    Cooldown = 0.45f,
                    DamageScale = 1.25f,
                    PreferredSlot = AbilitySlot.Primary
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_smoke_bomb",
                    Name = "Smoke Bomb",
                    Description = "Disorient enemies and create an opening.",
                    Cooldown = 2.0f,
                    DamageScale = 0.75f,
                    PreferredSlot = AbilitySlot.Secondary,
                    AppliesStatusEffect = true,
                    StatusEffectType = StatusEffectType.Curse,
                    StatusEffectChance = 60,
                    IsAOE = true,
                    AOERadius = 4.0f
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_venom_blade",
                    Name = "Venom Blade",
                    Description = "Toxic strike that applies stacking poison.",
                    Cooldown = 1.8f,
                    DamageScale = 1.65f,
                    ElementType = ElementType.Poison,
                    PreferredSlot = AbilitySlot.Tertiary,
                    AppliesStatusEffect = true,
                    StatusEffectType = StatusEffectType.Poison,
                    StatusEffectChance = 70
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_assassinate",
                    Name = "Assassinate",
                    Description = "High-damage execution strike from the shadows.",
                    Cooldown = 5.4f,
                    DamageScale = 3.1f,
                    PreferredSlot = AbilitySlot.Ultimate
                }));

            RegisterAbilityPool(CreateAbilityPool(
                "cleric_loadout",
                "cleric",
                new AbilityDefinitionData
                {
                    AbilityId = "ability_smite",
                    Name = "Smite",
                    Description = "Focused holy damage against a single target.",
                    Cooldown = 0.85f,
                    DamageScale = 1.3f,
                    ElementType = ElementType.Holy,
                    PreferredSlot = AbilitySlot.Primary
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_healing_prayer",
                    Name = "Healing Prayer",
                    Description = "Restorative pulse that keeps the run alive.",
                    Cooldown = 2.6f,
                    DamageScale = 0.85f,
                    ElementType = ElementType.Holy,
                    PreferredSlot = AbilitySlot.Secondary
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_consecration",
                    Name = "Consecration",
                    Description = "Blessed ground that harms enemies over time.",
                    Cooldown = 3.3f,
                    DamageScale = 1.5f,
                    ElementType = ElementType.Holy,
                    PreferredSlot = AbilitySlot.Tertiary,
                    AppliesStatusEffect = true,
                    StatusEffectType = StatusEffectType.Burn,
                    StatusEffectChance = 45,
                    IsAOE = true,
                    AOERadius = 5.0f
                },
                new AbilityDefinitionData
                {
                    AbilityId = "ability_divine_judgment",
                    Name = "Divine Judgment",
                    Description = "Call down holy wrath in a large burst.",
                    Cooldown = 6.1f,
                    DamageScale = 3.0f,
                    ElementType = ElementType.Holy,
                    PreferredSlot = AbilitySlot.Ultimate,
                    IsAOE = true,
                    AOERadius = 6.0f
                }));
        }

        private void RegisterDefaultEnemies()
        {
            _library.RegisterEnemy(CreateEnemy("briarwolf", "Briar Wolf", "whispering_forest", 58, 10, 2, 35, "thorn_pelt", "Thorn Pelt", new Color(0.22f, 0.42f, 0.18f, 1f)));
            _library.RegisterEnemy(CreateEnemy("sapling_brute", "Sapling Brute", "whispering_forest", 85, 12, 4, 42, "living_bark", "Living Bark", new Color(0.35f, 0.27f, 0.15f, 1f)));
            _library.RegisterEnemy(CreateEnemy("moss_archer", "Moss Archer", "whispering_forest", 48, 14, 1, 38, "spore_arrow", "Spore Arrow", new Color(0.27f, 0.48f, 0.24f, 1f)));

            _library.RegisterEnemy(CreateEnemy("slag_beetle", "Slag Beetle", "forgotten_mines", 70, 12, 5, 48, "molten_shell", "Molten Shell", new Color(0.46f, 0.28f, 0.16f, 1f)));
            _library.RegisterEnemy(CreateEnemy("ember_miner", "Ember Miner", "forgotten_mines", 62, 15, 3, 44, "ember_core", "Ember Core", new Color(0.62f, 0.31f, 0.12f, 1f)));
            _library.RegisterEnemy(CreateEnemy("tunnel_warden", "Tunnel Warden", "forgotten_mines", 96, 13, 6, 55, "warden_plate", "Warden Plate", new Color(0.33f, 0.31f, 0.28f, 1f)));

            _library.RegisterEnemy(CreateEnemy("ice_bat", "Ice Bat", "frozen_peaks", 55, 16, 1, 50, "rime_wing", "Rime Wing", new Color(0.67f, 0.82f, 0.95f, 1f)));
            _library.RegisterEnemy(CreateEnemy("avalanche_ram", "Avalanche Ram", "frozen_peaks", 104, 17, 5, 60, "glacier_horn", "Glacier Horn", new Color(0.76f, 0.82f, 0.88f, 1f)));
            _library.RegisterEnemy(CreateEnemy("frost_sentinel", "Frost Sentinel", "frozen_peaks", 88, 18, 6, 58, "sentinel_shard", "Sentinel Shard", new Color(0.58f, 0.74f, 0.92f, 1f)));
        }

        private void RegisterDefaultBosses()
        {
            _library.RegisterBoss(CreateBoss("thornbound_alpha", "Thornbound Alpha", "Ancient Beast of the Canopy", "whispering_forest", 280, 22, 6, 180, 3, new Color(0.24f, 0.52f, 0.18f, 1f)));
            _library.RegisterBoss(CreateBoss("furnace_overseer", "Furnace Overseer", "Molten Tyrant of the Deep Roads", "forgotten_mines", 360, 26, 8, 230, 4, new Color(0.72f, 0.34f, 0.12f, 1f)));
            _library.RegisterBoss(CreateBoss("whiteout_wyrm", "Whiteout Wyrm", "Storm Herald of the Peaks", "frozen_peaks", 460, 31, 9, 320, 4, new Color(0.78f, 0.88f, 0.98f, 1f)));
        }

        private void RegisterDefaultObjectivePools()
        {
            _library.RegisterObjectivePool(CreateObjectivePool(
                "forest_objectives",
                "whispering_forest",
                new ObjectiveDefinitionData { ObjectiveId = "forest_kill_pack", DisplayName = "Cull the Pack", Type = ObjectiveType.Kill, RequiredProgress = 8 },
                new ObjectiveDefinitionData { ObjectiveId = "forest_fire_clear", DisplayName = "Burn the Brush", Type = ObjectiveType.ApplyStatusEffect, RequiredProgress = 3, Element = ElementType.Fire },
                new ObjectiveDefinitionData { ObjectiveId = "forest_precision", DisplayName = "Predator Precision", Type = ObjectiveType.CriticalHit, RequiredProgress = 4 },
                new ObjectiveDefinitionData { ObjectiveId = "forest_loot", DisplayName = "Gather Wild Trophies", Type = ObjectiveType.LootItem, RequiredProgress = 4 },
                new ObjectiveDefinitionData { ObjectiveId = "forest_chests", DisplayName = "Find Forest Caches", Type = ObjectiveType.OpenChest, RequiredProgress = 2 },
                new ObjectiveDefinitionData { ObjectiveId = "forest_boss", DisplayName = "Silence the Alpha", Type = ObjectiveType.DefeatBoss, TargetSourceId = "thornbound_alpha" }));

            _library.RegisterObjectivePool(CreateObjectivePool(
                "mines_objectives",
                "forgotten_mines",
                new ObjectiveDefinitionData { ObjectiveId = "mines_kill_line", DisplayName = "Break the Dig Line", Type = ObjectiveType.Kill, RequiredProgress = 10 },
                new ObjectiveDefinitionData { ObjectiveId = "mines_lightning", DisplayName = "Overload Their Gear", Type = ObjectiveType.ApplyStatusEffect, RequiredProgress = 4, Element = ElementType.Lightning },
                new ObjectiveDefinitionData { ObjectiveId = "mines_damage", DisplayName = "Shatter the Front", Type = ObjectiveType.Damage, RequiredProgress = 180 },
                new ObjectiveDefinitionData { ObjectiveId = "mines_dodge", DisplayName = "Slip the Crossfire", Type = ObjectiveType.Dodge, RequiredProgress = 4 },
                new ObjectiveDefinitionData { ObjectiveId = "mines_rares", DisplayName = "Scavenge Rare Components", Type = ObjectiveType.LootItem, RequiredProgress = 2, MinimumItemRarity = 2 },
                new ObjectiveDefinitionData { ObjectiveId = "mines_boss", DisplayName = "Cool the Furnace Overseer", Type = ObjectiveType.DefeatBoss, TargetSourceId = "furnace_overseer" }));

            _library.RegisterObjectivePool(CreateObjectivePool(
                "peaks_objectives",
                "frozen_peaks",
                new ObjectiveDefinitionData { ObjectiveId = "peaks_kill_wave", DisplayName = "Weather the Storm", Type = ObjectiveType.Kill, RequiredProgress = 12 },
                new ObjectiveDefinitionData { ObjectiveId = "peaks_freeze", DisplayName = "Turn the Gale Aside", Type = ObjectiveType.ApplyStatusEffect, RequiredProgress = 3, Element = ElementType.Frost },
                new ObjectiveDefinitionData { ObjectiveId = "peaks_abilities", DisplayName = "Ride the Tempest", Type = ObjectiveType.UseAbility, RequiredProgress = 10 },
                new ObjectiveDefinitionData { ObjectiveId = "peaks_damage", DisplayName = "Break the Whiteout", Type = ObjectiveType.Damage, RequiredProgress = 260 },
                new ObjectiveDefinitionData { ObjectiveId = "peaks_chests", DisplayName = "Crack the Ice Caches", Type = ObjectiveType.OpenChest, RequiredProgress = 3 },
                new ObjectiveDefinitionData { ObjectiveId = "peaks_boss", DisplayName = "Defeat the Whiteout Wyrm", Type = ObjectiveType.DefeatBoss, TargetSourceId = "whiteout_wyrm" }));
        }

        private void RegisterDefaultProfiles()
        {
            var warrior = new ClassContentProfile
            {
                ClassId = "warrior",
                AbilityPoolId = "warrior_loadout"
            };
            warrior.StarterSkillNodeIds.Add("slash");
            warrior.StarterSkillNodeIds.Add("shield_bash");
            warrior.AutoUnlockOrder.Add("defensive_stance");
            warrior.AutoUnlockOrder.Add("whirlwind");
            warrior.AutoUnlockOrder.Add("execute");
            _classProfiles[warrior.ClassId] = warrior;

            var mage = new ClassContentProfile
            {
                ClassId = "mage",
                AbilityPoolId = "mage_loadout"
            };
            mage.StarterSkillNodeIds.Add("fireball");
            mage.StarterSkillNodeIds.Add("frostbolt");
            mage.AutoUnlockOrder.Add("mana_shield");
            mage.AutoUnlockOrder.Add("meteor_storm");
            _classProfiles[mage.ClassId] = mage;

            var ranger = new ClassContentProfile
            {
                ClassId = "ranger",
                AbilityPoolId = "ranger_loadout"
            };
            ranger.StarterSkillNodeIds.Add("basic_shot");
            ranger.AutoUnlockOrder.Add("aimed_shot");
            ranger.AutoUnlockOrder.Add("multishot");
            _classProfiles[ranger.ClassId] = ranger;

            var rogue = new ClassContentProfile
            {
                ClassId = "rogue",
                AbilityPoolId = "rogue_loadout"
            };
            rogue.StarterSkillNodeIds.Add("quick_stab");
            rogue.StarterSkillNodeIds.Add("smoke_bomb");
            rogue.AutoUnlockOrder.Add("shadowstep");
            rogue.AutoUnlockOrder.Add("venom_blade");
            rogue.AutoUnlockOrder.Add("assassinate");
            _classProfiles[rogue.ClassId] = rogue;

            var cleric = new ClassContentProfile
            {
                ClassId = "cleric",
                AbilityPoolId = "cleric_loadout"
            };
            cleric.StarterSkillNodeIds.Add("smite");
            cleric.StarterSkillNodeIds.Add("healing_prayer");
            cleric.AutoUnlockOrder.Add("consecration");
            cleric.AutoUnlockOrder.Add("guardian_aegis");
            cleric.AutoUnlockOrder.Add("divine_judgment");
            _classProfiles[cleric.ClassId] = cleric;

            RegisterRegionProfile(new RegionContentProfile
            {
                RegionId = "whispering_forest",
                ObjectivePoolId = "forest_objectives",
                BossId = "thornbound_alpha",
                BossSpawnKillRequirement = 8,
                RewardCurrencyAmount = 6,
                EnemyIds = { "briarwolf", "sapling_brute", "moss_archer" }
            });

            RegisterRegionProfile(new RegionContentProfile
            {
                RegionId = "forgotten_mines",
                ObjectivePoolId = "mines_objectives",
                BossId = "furnace_overseer",
                BossSpawnKillRequirement = 10,
                RewardCurrencyAmount = 8,
                EnemyIds = { "slag_beetle", "ember_miner", "tunnel_warden" }
            });

            RegisterRegionProfile(new RegionContentProfile
            {
                RegionId = "frozen_peaks",
                ObjectivePoolId = "peaks_objectives",
                BossId = "whiteout_wyrm",
                BossSpawnKillRequirement = 12,
                RewardCurrencyAmount = 10,
                EnemyIds = { "ice_bat", "avalanche_ram", "frost_sentinel" }
            });
        }

        private void RegisterAbilityPool(AbilityPool pool)
        {
            _library.RegisterAbilityPool(pool);
            for (int i = 0; i < pool.Abilities.Count; i++)
            {
                var ability = pool.Abilities[i];
                if (!string.IsNullOrWhiteSpace(ability.AbilityId))
                    _abilitiesById[ability.AbilityId] = ability;
            }
        }

        private void RegisterRegionProfile(RegionContentProfile profile)
        {
            if (profile != null && !string.IsNullOrWhiteSpace(profile.RegionId))
                _regionProfiles[profile.RegionId] = profile;
        }

        private static AbilityPool CreateAbilityPool(string poolId, string classId, params AbilityDefinitionData[] abilities)
        {
            var pool = ScriptableObject.CreateInstance<AbilityPool>();
            pool.PoolId = poolId;
            pool.ClassId = classId;
            pool.Abilities = new List<AbilityDefinitionData>(abilities);
            pool.name = poolId;
            return pool;
        }

        private static ObjectivePool CreateObjectivePool(string poolId, string regionId, params ObjectiveDefinitionData[] objectives)
        {
            var pool = ScriptableObject.CreateInstance<ObjectivePool>();
            pool.PoolId = poolId;
            pool.RegionId = regionId;
            pool.Objectives = new List<ObjectiveDefinitionData>(objectives);
            pool.name = poolId;
            return pool;
        }

        private static EnemyDefinition CreateEnemy(string id, string displayName, string regionId, int hp, int attack, int defense, int xp, string materialId, string materialName, Color tint)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.EnemyId = id;
            enemy.DisplayName = displayName;
            enemy.RegionId = regionId;
            enemy.Description = $"{displayName} patrols {regionId.Replace("_", " ")}.";
            enemy.BaseHealth = hp;
            enemy.BaseAttack = attack;
            enemy.BaseDefense = defense;
            enemy.ExperienceReward = xp;
            enemy.LootMaterialId = materialId;
            enemy.LootMaterialName = materialName;
            enemy.Tint = tint;
            enemy.name = id;
            return enemy;
        }

        private static BossDefinition CreateBoss(string id, string displayName, string subtitle, string regionId, int hp, int attack, int defense, int xp, int lootCount, Color tint)
        {
            var boss = ScriptableObject.CreateInstance<BossDefinition>();
            boss.BossId = id;
            boss.DisplayName = displayName;
            boss.Subtitle = subtitle;
            boss.RegionId = regionId;
            boss.BaseHealth = hp;
            boss.BaseAttack = attack;
            boss.BaseDefense = defense;
            boss.ExperienceReward = xp;
            boss.LootCount = lootCount;
            boss.Tint = tint;
            boss.PhaseNames = new List<string> { "Opening", "Enrage", "Final Stand" };
            boss.name = id;
            return boss;
        }
    }
}
