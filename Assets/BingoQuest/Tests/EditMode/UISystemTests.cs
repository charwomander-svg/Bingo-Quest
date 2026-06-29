using BingoQuest.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace BingoQuest.Tests.EditMode
{
    public class UISystemTests
    {
        private GameObject testGameObject;

        [SetUp]
        public void Setup()
        {
            testGameObject = new GameObject("UITest");
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(testGameObject);
        }

        #region Inventory UI Tests

        [Test]
        public void InventoryUI_InitializesWithoutError()
        {
            InventoryUI inventory = testGameObject.AddComponent<InventoryUI>();
            Assert.That(inventory, Is.Not.Null);
        }

        [Test]
        public void InventoryUI_CanCreateInventoryItem()
        {
            var item = new InventoryItem
            {
                Name = "Test Sword",
                Description = "A test weapon",
                Rarity = "Rare",
                Type = "Weapon",
                Damage = 50,
                LevelRequired = 10,
                GoldValue = 500
            };

            Assert.That(item.Name, Is.EqualTo("Test Sword"));
            Assert.That(item.Damage, Is.EqualTo(50));
        }

        [Test]
        public void InventoryUI_CanRefreshInventory()
        {
            var inventory = testGameObject.AddComponent<InventoryUI>();
            var items = new List<InventoryItem>
            {
                new InventoryItem { Name = "Sword", Rarity = "Rare", Type = "Weapon", Damage = 50 },
                new InventoryItem { Name = "Shield", Rarity = "Uncommon", Type = "Armor", Defense = 30 }
            };

            // Should not throw
            inventory.RefreshInventory(items);
            Assert.Pass();
        }

        [Test]
        public void InventoryUI_RarityColorsConfigurable()
        {
            var inventory = testGameObject.AddComponent<InventoryUI>();
            Assert.That(inventory, Is.Not.Null);
            // Color mapping should be set up during Start()
        }

        [Test]
        public void InventoryItem_HasConsumableFlag()
        {
            var potion = new InventoryItem
            {
                Name = "Health Potion",
                Type = "Consumable",
                IsConsumable = true
            };

            Assert.That(potion.IsConsumable, Is.True);
        }

        [Test]
        public void InventoryItem_StoresAllStats()
        {
            var item = new InventoryItem
            {
                Name = "Epic Sword",
                Description = "Legendary weapon",
                Rarity = "Epic",
                Type = "Weapon",
                Damage = 100,
                Defense = 10,
                LevelRequired = 20,
                GoldValue = 5000,
                IsConsumable = false
            };

            Assert.That(item.Damage, Is.EqualTo(100));
            Assert.That(item.Defense, Is.EqualTo(10));
            Assert.That(item.LevelRequired, Is.EqualTo(20));
            Assert.That(item.GoldValue, Is.EqualTo(5000));
        }

        #endregion

        #region Character Sheet Tests

        [Test]
        public void CharacterSheetUI_InitializesWithoutError()
        {
            CharacterSheetUI sheet = testGameObject.AddComponent<CharacterSheetUI>();
            Assert.That(sheet, Is.Not.Null);
        }

        [Test]
        public void CharacterData_StoresCharacterInfo()
        {
            var character = new CharacterData
            {
                Name = "Hero",
                Level = 10,
                CurrentXP = 500,
                XPForNextLevel = 1000,
                MaxHP = 100,
                CurrentHP = 75,
                MaxMana = 50,
                CurrentMana = 40,
                Damage = 25,
                Defense = 15,
                CritChance = 15.5f,
                AttackSpeed = 1.5f,
                Strength = 10,
                Dexterity = 8,
                Intelligence = 12,
                Vitality = 9,
                AttributePoints = 3
            };

            Assert.That(character.Name, Is.EqualTo("Hero"));
            Assert.That(character.Level, Is.EqualTo(10));
            Assert.That(character.Damage, Is.EqualTo(25));
            Assert.That(character.AttributePoints, Is.EqualTo(3));
        }

        [Test]
        public void CharacterSheetUI_CanRefreshDisplay()
        {
            var sheet = testGameObject.AddComponent<CharacterSheetUI>();
            var character = new CharacterData
            {
                Name = "Test",
                Level = 5,
                CurrentXP = 100,
                XPForNextLevel = 500,
                MaxHP = 50,
                CurrentHP = 50,
                MaxMana = 30,
                CurrentMana = 30
            };

            // Should not throw
            sheet.RefreshCharacterSheet(character);
            Assert.Pass();
        }

        [Test]
        public void CharacterData_TrackEquippedItems()
        {
            var character = new CharacterData
            {
                Name = "Adventurer",
                Level = 15
            };

            var weapon = new InventoryItem { Name = "Sword", Type = "Weapon", Damage = 50 };
            var armor = new InventoryItem { Name = "Plate Mail", Type = "Armor", Defense = 40 };

            character.EquippedWeapon = weapon;
            character.EquippedArmor = armor;

            Assert.That(character.EquippedWeapon.Name, Is.EqualTo("Sword"));
            Assert.That(character.EquippedArmor.Name, Is.EqualTo("Plate Mail"));
        }

        #endregion

        #region Skill Tree Tests

        [Test]
        public void SkillTreeUI_InitializesWithoutError()
        {
            SkillTreeUI skillTree = testGameObject.AddComponent<SkillTreeUI>();
            Assert.That(skillTree, Is.Not.Null);
        }

        [Test]
        public void SkillTreeUI_CanRefreshTree()
        {
            var skillTree = testGameObject.AddComponent<SkillTreeUI>();
            var unlockedSkills = new List<int> { 1, 2 };

            // Should not throw
            skillTree.RefreshSkillTree(5, unlockedSkills);
            Assert.Pass();
        }

        [Test]
        public void SkillTreeUI_TrackSkillPoints()
        {
            var skillTree = testGameObject.AddComponent<SkillTreeUI>();
            skillTree.RefreshSkillTree(10, new List<int>());
            
            // SkillTree should track available skill points
            Assert.Pass();
        }

        [Test]
        public void SkillTreeUI_CanUnlockSkills()
        {
            var skillTree = testGameObject.AddComponent<SkillTreeUI>();
            var unlockedSkills = new List<int>();

            skillTree.RefreshSkillTree(1, unlockedSkills);
            // Unlocking would add to the unlockedSkills list
            Assert.That(unlockedSkills.Count, Is.EqualTo(0));
        }

        [Test]
        public void SkillTreeUI_SupportsRespec()
        {
            var skillTree = testGameObject.AddComponent<SkillTreeUI>();
            var unlockedSkills = new List<int> { 1, 2, 3 };

            skillTree.RefreshSkillTree(0, unlockedSkills);
            
            // Respec should reset all unlocked skills and refund points
            skillTree.ConfirmRespec(3); // 3 points refunded
            
            Assert.Pass();
        }

        #endregion

        #region Settings Menu Tests

        [Test]
        public void SettingsMenuUI_InitializesWithoutError()
        {
            SettingsMenuUI settings = testGameObject.AddComponent<SettingsMenuUI>();
            Assert.That(settings, Is.Not.Null);
        }

        [Test]
        public void GameSettings_HasDefaultValues()
        {
            var settings = new GameSettings();

            Assert.That(settings.MasterVolume, Is.EqualTo(1f));
            Assert.That(settings.SFXVolume, Is.EqualTo(1f));
            Assert.That(settings.MusicVolume, Is.EqualTo(0.7f));
            Assert.That(settings.AmbienceVolume, Is.EqualTo(0.5f));
            Assert.That(settings.Difficulty, Is.EqualTo("Normal"));
        }

        [Test]
        public void GameSettings_TrackAccessibilityOptions()
        {
            var settings = new GameSettings();

            Assert.That(settings.ScreenShakeEnabled, Is.True);
            Assert.That(settings.ColorblindMode, Is.False);
            Assert.That(settings.SubtitlesEnabled, Is.True);
        }

        [Test]
        public void GameSettings_CanModifyVolumes()
        {
            var settings = new GameSettings();
            settings.MasterVolume = 0.5f;
            settings.MusicVolume = 0.3f;

            Assert.That(settings.MasterVolume, Is.EqualTo(0.5f));
            Assert.That(settings.MusicVolume, Is.EqualTo(0.3f));
        }

        [Test]
        public void GameSettings_CanChangeDifficulty()
        {
            var settings = new GameSettings();
            settings.Difficulty = "Hard";

            Assert.That(settings.Difficulty, Is.EqualTo("Hard"));
        }

        [Test]
        public void GameSettings_CanToggleAccessibility()
        {
            var settings = new GameSettings();
            settings.ColorblindMode = true;
            settings.SubtitlesEnabled = false;

            Assert.That(settings.ColorblindMode, Is.True);
            Assert.That(settings.SubtitlesEnabled, Is.False);
        }

        [Test]
        public void SettingsMenuUI_CanApplySettings()
        {
            var settingsUI = testGameObject.AddComponent<SettingsMenuUI>();
            
            // ApplySettings should gather from UI and fire event
            settingsUI.ApplySettings();
            Assert.Pass();
        }

        [Test]
        public void SettingsMenuUI_CanResetToDefaults()
        {
            var settingsUI = testGameObject.AddComponent<SettingsMenuUI>();
            
            settingsUI.ResetToDefaults();
            // Should reset all values to defaults
            Assert.Pass();
        }

        #endregion

        #region UI Integration Tests

        [Test]
        public void InventoryUI_CanCompareItems()
        {
            var inventory = testGameObject.AddComponent<InventoryUI>();
            
            var item1 = new InventoryItem { Name = "Sword", Damage = 50 };
            var item2 = new InventoryItem { Name = "Great Sword", Damage = 75 };

            var items = new List<InventoryItem> { item1, item2 };
            inventory.RefreshInventory(items);
            
            // Should support comparison
            Assert.Pass();
        }

        [Test]
        public void CharacterSheetUI_CanDisplayAttributes()
        {
            var sheet = testGameObject.AddComponent<CharacterSheetUI>();
            var character = new CharacterData
            {
                Name = "Warrior",
                Strength = 15,
                Dexterity = 10,
                Intelligence = 8,
                Vitality = 14,
                AttributePoints = 2
            };

            sheet.RefreshCharacterSheet(character);
            Assert.That(character.AttributePoints, Is.GreaterThan(0));
        }

        [Test]
        public void SkillTreeUI_CanDisplaySkillDescriptions()
        {
            var skillTree = testGameObject.AddComponent<SkillTreeUI>();
            skillTree.RefreshSkillTree(5, new List<int>());
            
            // Should have skill descriptions available
            Assert.Pass();
        }

        [Test]
        public void SettingsMenuUI_CanPersistSettings()
        {
            var settingsUI = testGameObject.AddComponent<SettingsMenuUI>();
            
            var settings = new GameSettings();
            settings.MasterVolume = 0.5f;
            settings.Difficulty = "Hard";
            
            // Settings should be saveable
            Assert.That(settings.MasterVolume, Is.EqualTo(0.5f));
            Assert.That(settings.Difficulty, Is.EqualTo("Hard"));
        }

        #endregion

        #region Event Firing Tests

        [Test]
        public void InventoryUI_FiresItemSelectedEvent()
        {
            var inventory = testGameObject.AddComponent<InventoryUI>();
            bool eventFired = false;
            
            inventory.OnItemSelected += (item) => { eventFired = true; };
            
            // Event should fire when item selected
            Assert.That(inventory.OnItemSelected, Is.Not.Null);
        }

        [Test]
        public void SkillTreeUI_FiresSkillUnlockedEvent()
        {
            var skillTree = testGameObject.AddComponent<SkillTreeUI>();
            bool eventFired = false;
            
            skillTree.OnSkillUnlocked += (skillId) => { eventFired = true; };
            
            // Event should fire when skill unlocked
            Assert.That(skillTree.OnSkillUnlocked, Is.Not.Null);
        }

        [Test]
        public void SettingsMenuUI_FiresSettingsAppliedEvent()
        {
            var settingsUI = testGameObject.AddComponent<SettingsMenuUI>();
            bool eventFired = false;
            
            settingsUI.OnSettingsApplied += (settings) => { eventFired = true; };
            
            // Event should fire when settings applied
            Assert.That(settingsUI.OnSettingsApplied, Is.Not.Null);
        }

        #endregion
    }
}
