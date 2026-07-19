using BingoQuest.Gameplay.Difficulty;
using BingoQuest.Gameplay.Balance;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Progression;
using NUnit.Framework;
using UnityEngine;

namespace BingoQuest.Tests.EditMode
{
    public class DifficultyPresetTests
    {
        private DifficultyPreset completePreset;
        private DifficultyPreset incompletePreset;
        private DifficultyManager manager;

        [SetUp]
        public void Setup()
        {
            // Create a complete preset with all configs
            completePreset = ScriptableObject.CreateInstance<DifficultyPreset>();
            var balanceConfig = ScriptableObject.CreateInstance<BalanceConfig>();
            var progressionConfig = ScriptableObject.CreateInstance<ProgressionConfig>();
            var lootConfig = ScriptableObject.CreateInstance<LootConfig>();
            var abilityConfig = ScriptableObject.CreateInstance<AbilityConfig>();

            // Use reflection to set private fields
            typeof(DifficultyPreset).GetField("balanceConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(completePreset, balanceConfig);
            typeof(DifficultyPreset).GetField("progressionConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(completePreset, progressionConfig);
            typeof(DifficultyPreset).GetField("lootConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(completePreset, lootConfig);
            typeof(DifficultyPreset).GetField("abilityConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(completePreset, abilityConfig);

            // Create an incomplete preset (missing loot config)
            incompletePreset = ScriptableObject.CreateInstance<DifficultyPreset>();
            typeof(DifficultyPreset).GetField("balanceConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(incompletePreset, balanceConfig);
            typeof(DifficultyPreset).GetField("progressionConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(incompletePreset, progressionConfig);
            typeof(DifficultyPreset).GetField("abilityConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(incompletePreset, abilityConfig);

            manager = new DifficultyManager();
        }

        [TearDown]
        public void Teardown()
        {
            manager.Clear();
        }

        [Test]
        public void CompletePreset_IsValid()
        {
            Assert.That(completePreset.IsComplete(), Is.True);
        }

        [Test]
        public void IncompletePreset_IsNotValid()
        {
            Assert.That(incompletePreset.IsComplete(), Is.False);
        }

        [Test]
        public void IncompletePreset_ReportsMissingConfigs()
        {
            string missing = incompletePreset.GetMissingConfigs();
            
            Assert.That(missing, Does.Contain("LootConfig"));
        }

        [Test]
        public void PresetName_IsAccessible()
        {
            Assert.That(completePreset.PresetName, Is.Not.Null);
        }

        [Test]
        public void DifficultyRating_IsInValidRange()
        {
            Assert.That(completePreset.DifficultyRating, Is.GreaterThanOrEqualTo(0f).And.LessThanOrEqualTo(5f));
        }

        [Test]
        public void PresetColor_IsValid()
        {
            var color = completePreset.PresetColor;
            
            Assert.That(color.r, Is.GreaterThanOrEqualTo(0f).And.LessThanOrEqualTo(1f));
            Assert.That(color.g, Is.GreaterThanOrEqualTo(0f).And.LessThanOrEqualTo(1f));
            Assert.That(color.b, Is.GreaterThanOrEqualTo(0f).And.LessThanOrEqualTo(1f));
        }

        [Test]
        public void BalanceConfig_IsReturned()
        {
            Assert.That(completePreset.BalanceConfig, Is.Not.Null);
        }

        [Test]
        public void ProgressionConfig_IsReturned()
        {
            Assert.That(completePreset.ProgressionConfig, Is.Not.Null);
        }

        [Test]
        public void LootConfig_IsReturned()
        {
            Assert.That(completePreset.LootConfig, Is.Not.Null);
        }

        [Test]
        public void AbilityConfig_IsReturned()
        {
            Assert.That(completePreset.AbilityConfig, Is.Not.Null);
        }

        [Test]
        public void DifficultyManager_StartsWithNoPreset()
        {
            Assert.That(manager.CurrentPreset, Is.Null);
        }

        [Test]
        public void DifficultyManager_AcceptsCompletePreset()
        {
            bool success = manager.SetPreset(completePreset);
            
            Assert.That(success, Is.True);
            Assert.That(manager.CurrentPreset, Is.EqualTo(completePreset));
        }

        [Test]
        public void DifficultyManager_RejectsIncompletePreset()
        {
            bool success = manager.SetPreset(incompletePreset);
            
            Assert.That(success, Is.False);
        }

        [Test]
        public void DifficultyManager_RejectsNullPreset()
        {
            bool success = manager.SetPreset(null);
            
            Assert.That(success, Is.False);
        }

        [Test]
        public void DifficultyManager_GetBalanceConfig_ReturnsConfigAfterSet()
        {
            manager.SetPreset(completePreset);
            
            var config = manager.GetBalanceConfig();
            
            Assert.That(config, Is.Not.Null);
            Assert.That(config, Is.EqualTo(completePreset.BalanceConfig));
        }

        [Test]
        public void DifficultyManager_GetProgressionConfig_ReturnsConfigAfterSet()
        {
            manager.SetPreset(completePreset);
            
            var config = manager.GetProgressionConfig();
            
            Assert.That(config, Is.Not.Null);
            Assert.That(config, Is.EqualTo(completePreset.ProgressionConfig));
        }

        [Test]
        public void DifficultyManager_GetLootConfig_ReturnsConfigAfterSet()
        {
            manager.SetPreset(completePreset);
            
            var config = manager.GetLootConfig();
            
            Assert.That(config, Is.Not.Null);
            Assert.That(config, Is.EqualTo(completePreset.LootConfig));
        }

        [Test]
        public void DifficultyManager_GetAbilityConfig_ReturnsConfigAfterSet()
        {
            manager.SetPreset(completePreset);
            
            var config = manager.GetAbilityConfig();
            
            Assert.That(config, Is.Not.Null);
            Assert.That(config, Is.EqualTo(completePreset.AbilityConfig));
        }

        [Test]
        public void DifficultyManager_GetDifficultyMode_ReturnsEasyForLowRating()
        {
            // Set preset with low difficulty rating
            typeof(DifficultyPreset).GetField("presetInfo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(completePreset, new DifficultyPreset.PresetInfo { DifficultyRating = 1.0f });
            
            manager.SetPreset(completePreset);
            var mode = manager.GetDifficultyMode();
            
            Assert.That(mode, Is.EqualTo(Progression.DifficultyMode.Easy));
        }

        [Test]
        public void DifficultyManager_GetDifficultyMode_ReturnsNormalForMediumRating()
        {
            // Set preset with medium difficulty rating
            typeof(DifficultyPreset).GetField("presetInfo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(completePreset, new DifficultyPreset.PresetInfo { DifficultyRating = 2.0f });
            
            manager.SetPreset(completePreset);
            var mode = manager.GetDifficultyMode();
            
            Assert.That(mode, Is.EqualTo(Progression.DifficultyMode.Normal));
        }

        [Test]
        public void DifficultyManager_GetDifficultyMode_ReturnsHardForHighRating()
        {
            // Set preset with high difficulty rating
            typeof(DifficultyPreset).GetField("presetInfo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(completePreset, new DifficultyPreset.PresetInfo { DifficultyRating = 3.0f });
            
            manager.SetPreset(completePreset);
            var mode = manager.GetDifficultyMode();
            
            Assert.That(mode, Is.EqualTo(Progression.DifficultyMode.Hard));
        }

        [Test]
        public void DifficultyManager_GetDifficultyMode_ReturnsNightmareForMaxRating()
        {
            // Set preset with max difficulty rating
            typeof(DifficultyPreset).GetField("presetInfo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(completePreset, new DifficultyPreset.PresetInfo { DifficultyRating = 5.0f });
            
            manager.SetPreset(completePreset);
            var mode = manager.GetDifficultyMode();
            
            Assert.That(mode, Is.EqualTo(Progression.DifficultyMode.Nightmare));
        }

        [Test]
        public void DifficultyManager_FiresOnPresetChangedEvent()
        {
            DifficultyPreset changedPreset = null;
            manager.OnPresetChanged += (preset) => { changedPreset = preset; };
            
            manager.SetPreset(completePreset);
            
            Assert.That(changedPreset, Is.EqualTo(completePreset));
        }

        [Test]
        public void DifficultyManager_Clear_ResetsPreset()
        {
            manager.SetPreset(completePreset);
            manager.Clear();
            
            Assert.That(manager.CurrentPreset, Is.Null);
        }

        [Test]
        public void DifficultyManager_Singleton_ReturnsSameInstance()
        {
            var instance1 = DifficultyManager.Instance;
            var instance2 = DifficultyManager.Instance;
            
            Assert.That(instance1, Is.EqualTo(instance2));
        }

        [Test]
        public void PresetToString_ContainsPresetInfo()
        {
            string str = completePreset.ToString();
            
            Assert.That(str, Does.Contain("✓")); // Should show checks for assigned configs
        }
    }
}
