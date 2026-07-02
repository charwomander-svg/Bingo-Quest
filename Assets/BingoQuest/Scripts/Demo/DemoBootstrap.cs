using System;
using System.Collections;
using System.IO;
using System.Reflection;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Content;
using BingoQuest.Gameplay.Difficulty;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Objectives;
using BingoQuest.Gameplay.Progression;
using BingoQuest.Platform.Save;
using UnityEngine;

namespace BingoQuest.Demo
{
    public class DemoBootstrap : MonoBehaviour
    {
        private Combatant playerCombatant;
        private DemoPlayerController playerController;
        private DemoEnemySpawner enemySpawner;
        private DemoWorldDirector worldDirector;
        private Inventory inventory;
        private LootService lootService;
        private LootTable lootTable;
        private CharacterProgression progression;
        private AuthoredContentCatalog contentCatalog;
        private DifficultyPreset[] allPresets;
        private ProfileManager profileManager;
        private Renderer floorRenderer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoSpawn()
        {
            if (FindObjectOfType<DemoBootstrap>() != null)
                return;

            var go = new GameObject("DemoBootstrap");
            go.AddComponent<DemoBootstrap>();
        }

        private IEnumerator Start()
        {
            floorRenderer = EnsureCameraAndWorldGeometry();
            EnsureCoreSystems();

            allPresets = DifficultyPresetFactory.CreateAll();

            // Show difficulty selector and wait for the player to pick
            var selector = gameObject.AddComponent<DifficultySelectorUI>();
            selector.SetPresets(allPresets);
            yield return new WaitUntil(() => selector.HasSelected);
            DifficultyManager.Instance.SetPreset(selector.SelectedPreset);
            Destroy(selector);

            contentCatalog = AuthoredContentCatalog.CreateDefault();
            if (BingoSystem.Instance != null)
                BingoSystem.Instance.Initialize(contentCatalog);

            CreatePlayer();
            CreateEnemySpawner();
            CreateLootSystems();
            CreateProgression();
            CreateWorldDirector();

            var hud = gameObject.AddComponent<DemoHudOverlay>();
            hud.Bind(
                playerCombatant,
                () => enemySpawner.GetAliveEnemyCount(),
                () => inventory,
                () => progression,
                () => worldDirector != null ? worldDirector.CurrentRegionName : "Unknown",
                () => worldDirector != null ? worldDirector.CurrentEnemyHealthMultiplier : 1f,
                () => worldDirector != null ? worldDirector.CurrentEnemyAttackMultiplier : 1f,
                () => profileManager?.ActiveProfile?.DisplayName ?? "No Profile");

            gameObject.AddComponent<BingoCardOverlay>();

            InitializeSaveSystem();

            yield return null;

            ConfigurePlayerStatsAndAbilities();
            worldDirector.BeginWorldRun();
            enemySpawner.Begin();
        }

        private static Renderer EnsureCameraAndWorldGeometry()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var camGo = new GameObject("Main Camera");
                camera = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }

            camera.transform.position = new Vector3(0f, 12f, -10f);
            camera.transform.rotation = Quaternion.Euler(40f, 0f, 0f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);

            if (FindObjectOfType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.1f;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            var floor = GameObject.Find("DemoFloor");
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "DemoFloor";
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(2.8f, 1f, 2.8f);
            }

            return floor.GetComponent<Renderer>();
        }

        private static void EnsureCoreSystems()
        {
            if (ObjectiveEventBus.Instance == null)
            {
                var bus = new GameObject("ObjectiveEventBus");
                bus.AddComponent<ObjectiveEventBus>();
            }

            if (BingoSystem.Instance == null)
            {
                var bingo = new GameObject("BingoSystem");
                bingo.AddComponent<BingoSystem>();
            }
        }

        private void CreatePlayer()
        {
            var playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGo.name = "DemoPlayer";
            playerGo.transform.position = new Vector3(0f, 1f, 0f);

            playerCombatant = playerGo.AddComponent<Combatant>();
            SetCombatantPrivateFields(playerCombatant, "player_demo", true);

            playerController = playerGo.AddComponent<DemoPlayerController>();
            playerController.Initialize(
                playerCombatant,
                () => enemySpawner.GetNearestAliveEnemy(playerGo.transform.position),
                () => lootService.OpenChest(lootTable, progression.Level, 2),
                () =>
                {
                    if (worldDirector != null)
                        worldDirector.TravelToNextRegion();
                });
        }

        private void CreateEnemySpawner()
        {
            var spawnerGo = new GameObject("DemoEnemySpawner");
            enemySpawner = spawnerGo.AddComponent<DemoEnemySpawner>();
            enemySpawner.Initialize(playerCombatant, SelectEnemyForSpawn, OnEnemySpawned);
        }

        private void CreateLootSystems()
        {
            inventory = new Inventory();
            var generator = new ItemGenerator(seed: 1337);
            lootService = new LootService(generator, inventory);

            lootTable = new LootTable();
            lootTable.AddEntry(new LootTableEntry
            {
                Definition = new ItemDefinition { ItemId = "iron_sword", DisplayName = "Iron Sword", Type = ItemType.Weapon, BasePower = 8 },
                Weight = 1f,
                MinimumItemLevel = 1
            });
            lootTable.AddEntry(new LootTableEntry
            {
                Definition = new ItemDefinition { ItemId = "ember_staff", DisplayName = "Ember Staff", Type = ItemType.Weapon, BasePower = 10, Element = ElementType.Fire },
                Weight = 0.8f,
                MinimumItemLevel = 2
            });
            lootTable.AddEntry(new LootTableEntry
            {
                Definition = new ItemDefinition { ItemId = "lucky_ring", DisplayName = "Lucky Ring", Type = ItemType.Accessory, BasePower = 6 },
                Weight = 0.6f,
                MinimumItemLevel = 3
            });

            inventory.AddCurrency("fate_shards", 100);
            inventory.AddCurrency("bingo_tokens", 25);
        }

        private void CreateProgression()
        {
            var classDefinition = contentCatalog != null ? contentCatalog.GetClassDefinition("warrior") : BuiltInClasses.Warrior;
            var tree = contentCatalog != null ? contentCatalog.CreateSkillTree("warrior") : SkillTreeFactory.CreateWarriorTree();
            var progressionConfig = DifficultyManager.Instance.GetProgressionConfig();
            progression = new CharacterProgression(classDefinition, tree, progressionConfig);
            contentCatalog?.ApplyStarterSkills(progression);
        }

        private void InitializeSaveSystem()
        {
            string saveDirectory = Path.Combine(Application.persistentDataPath, "BingoQuestDemo");
            profileManager = new ProfileManager(new LocalFileSaveBackend(saveDirectory));

            SaveProfile profile;
            if (profileManager.Profiles.Count > 0)
            {
                var firstProfileId = profileManager.Profiles[0].ProfileId;
                if (!profileManager.TryLoadProfile(firstProfileId, out profile))
                    profile = profileManager.CreateProfile("Demo Hero");
            }
            else
            {
                profile = profileManager.CreateProfile("Demo Hero");
            }

            TryRestoreFromProfile(profile);
        }

        private void TryRestoreFromProfile(SaveProfile profile)
        {
            if (profile == null || progression == null || inventory == null)
                return;

            SaveSystemBridge.RestoreProgression(profile, progression);
            SaveSystemBridge.RestoreInventory(profile, inventory);
        }

        private void SaveProfileSnapshot()
        {
            if (profileManager == null || progression == null || inventory == null)
                return;

            var profile = profileManager.ActiveProfile ?? profileManager.CreateProfile("Demo Hero");
            SaveSystemBridge.CaptureCharacter(profile, progression, "player_demo", "Demo Hero");
            SaveSystemBridge.CaptureInventory(profile, inventory);
            profileManager.SaveProfile(profile);
        }

        private void ReloadProfileSnapshot()
        {
            if (profileManager == null)
                return;

            var profile = profileManager.ActiveProfile;
            if (profile == null && profileManager.Profiles.Count > 0)
            {
                profileManager.TryLoadProfile(profileManager.Profiles[0].ProfileId, out profile);
            }

            if (profile == null)
                return;

            TryRestoreFromProfile(profile);
            ConfigurePlayerStatsAndAbilities();
        }

        private void CreateWorldDirector()
        {
            var worldGo = new GameObject("DemoWorldDirector");
            worldDirector = worldGo.AddComponent<DemoWorldDirector>();
            worldDirector.Initialize(enemySpawner, progression, floorRenderer, OnRegionObjectiveContextReady);
        }

        private void ConfigurePlayerStatsAndAbilities()
        {
            if (playerCombatant == null)
                return;

            if (progression != null)
            {
                ClassStatApplier.ApplyClassStats(playerCombatant.Stats, progression.Class);
                progression.ApplyBonusesToStats(playerCombatant.Stats);
            }
            playerCombatant.Stats.ElementalPower = 6;

            var abilityConfig = DifficultyManager.Instance.GetAbilityConfig();
            var abilities = contentCatalog != null ? contentCatalog.CreateStartingAbilities("warrior", abilityConfig) : new System.Collections.Generic.List<Ability>();
            if (abilities.Count > 0)
            {
                for (int i = 0; i < abilities.Count && i < 4; i++)
                    playerCombatant.ActionBar.SetAbility((AbilitySlot)i, abilities[i]);
            }
            else
            {
                playerCombatant.ActionBar.SetAbility(AbilitySlot.Primary, new Ability(new AbilityDefinition
                {
                    Id = "slash",
                    Name = "Slash",
                    Cooldown = 0.6f,
                    DamageScale = 1.3f,
                    ElementType = ElementType.Physical
                }));
            }
        }

        private static void OnRegionObjectiveContextReady(ObjectiveContext context)
        {
            if (BingoSystem.Instance != null)
                BingoSystem.Instance.GenerateNewRun(context);
        }

        private EnemyDefinition SelectEnemyForSpawn(string regionId, int spawnIndex)
        {
            return contentCatalog != null ? contentCatalog.GetEnemyForRegion(regionId, spawnIndex) : null;
        }

        private void OnEnemySpawned(Combatant enemy, EnemyDefinition enemyDefinition)
        {
            if (enemyDefinition != null)
                SetCombatantPrivateFields(enemy, $"{enemyDefinition.EnemyId}_{UnityEngine.Random.Range(1000, 9999)}", false);
            else
                SetCombatantPrivateFields(enemy, $"enemy_{UnityEngine.Random.Range(1000, 9999)}", false);

            float hpMultiplier = worldDirector != null ? worldDirector.CurrentEnemyHealthMultiplier : 1f;
            float atkMultiplier = worldDirector != null ? worldDirector.CurrentEnemyAttackMultiplier : 1f;

            // Apply difficulty scaling on top of region scaling
            var balanceConfig = DifficultyManager.Instance.GetBalanceConfig();
            var diffMode = DifficultyManager.Instance.GetDifficultyMode();
            float diffMult = balanceConfig != null ? balanceConfig.GetDifficultyMultiplier(diffMode) : 1f;

            var stats = enemyDefinition != null
                ? ContentFactory.CreateStatsFromEnemy(enemyDefinition)
                : new CharacterStats { MaxHealth = 55, Health = 55, Attack = 9, Defense = 2, CritChance = 0.04f, DodgeChance = 0.03f };
            stats.MaxHealth = Mathf.RoundToInt(stats.MaxHealth * hpMultiplier * diffMult);
            stats.Health = stats.MaxHealth;
            stats.Attack = Mathf.RoundToInt(stats.Attack * atkMultiplier * diffMult);

            enemy.Stats.MaxHealth = stats.MaxHealth;
            enemy.Stats.Health = stats.Health;
            enemy.Stats.Attack = stats.Attack;
            enemy.Stats.Defense = stats.Defense;
            enemy.Stats.CritChance = stats.CritChance;
            enemy.Stats.DodgeChance = stats.DodgeChance;

            enemy.OnDefeated += () =>
            {
                int xpReward = enemyDefinition != null ? enemyDefinition.ExperienceReward : 28;
                progression.GainExperience(xpReward);
                var lootDefinition = contentCatalog != null ? contentCatalog.CreateEnemyLootDefinition(enemyDefinition) : null;
                lootService.DropEnemyLoot(
                    lootDefinition ?? new ItemDefinition
                    {
                        ItemId = "enemy_drop",
                        DisplayName = "Scrap Trophy",
                        Type = ItemType.Material,
                        BasePower = 3
                    },
                    progression.Level,
                    enemyDefinition != null ? enemyDefinition.LootDropChance : 0.03f);
            };
        }

        private void Update()
        {
            if (allPresets != null)
            {
                for (int i = 0; i < allPresets.Length && i < 4; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                    {
                        DifficultyManager.Instance.SetPreset(allPresets[i]);
                        break;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.F5))
                SaveProfileSnapshot();

            if (Input.GetKeyDown(KeyCode.F9))
                ReloadProfileSnapshot();
        }

        private void OnApplicationQuit()
        {
            SaveProfileSnapshot();
        }

        private static void SetCombatantPrivateFields(Combatant combatant, string id, bool isPlayer)
        {
            if (combatant == null)
                return;

            var type = typeof(Combatant);
            type.GetField("combatantId", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(combatant, id);
            type.GetField("isPlayer", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(combatant, isPlayer);
        }
    }
}
