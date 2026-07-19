using NUnit.Framework;
using BingoQuest.Platform.Save;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Progression;
using System;

namespace BingoQuest.Tests.EditMode
{
    public sealed class SaveSystemTests
    {
        private InMemorySaveBackend _backend;
        private ProfileManager _mgr;

        [SetUp]
        public void SetUp()
        {
            _backend = new InMemorySaveBackend();
            _mgr = new ProfileManager(_backend);
        }

        [Test]
        public void CreateProfile_StoresAndReturnsProfile()
        {
            var p = _mgr.CreateProfile("Hero");
            Assert.AreEqual("Hero", p.DisplayName);
            Assert.IsFalse(string.IsNullOrEmpty(p.ProfileId));
            Assert.AreEqual(1, _mgr.Profiles.Count);
        }

        [Test]
        public void SaveAndLoad_RoundTrip()
        {
            var p = _mgr.CreateProfile("Tester");
            p.Character.Level = 7;
            p.Character.Experience = 42;
            p.Statistics.EnemiesKilled = 99;
            _mgr.SaveProfile(p);

            var mgr2 = new ProfileManager(_backend);
            Assert.IsTrue(mgr2.TryLoadProfile(p.ProfileId, out var loaded));
            Assert.AreEqual(7, loaded.Character.Level);
            Assert.AreEqual(42, loaded.Character.Experience);
            Assert.AreEqual(99, loaded.Statistics.EnemiesKilled);
        }

        [Test]
        public void CorruptActiveSlot_FallsBackToBackup()
        {
            var p = _mgr.CreateProfile("Adventurer");
            p.Character.Level = 5;
            _mgr.SaveProfile(p);
            // Save again so a backup exists
            p.Character.Level = 6;
            _mgr.SaveProfile(p);

            // Corrupt the active slot
            string activeKey = $"profile_{p.ProfileId}_active";
            _backend.Write(activeKey, "CORRUPTED_DATA");

            bool recoveryFired = false;
            var mgr2 = new ProfileManager(_backend);
            mgr2.OnCorruptionDetected += _ => recoveryFired = true;
            Assert.IsTrue(mgr2.TryLoadProfile(p.ProfileId, out var loaded));
            Assert.IsTrue(recoveryFired, "Should have fired corruption event");
            Assert.AreEqual(5, loaded.Character.Level, "Should restore backup (level 5 was the backup)");
        }

        [Test]
        public void DeleteProfile_RemovesFromIndex()
        {
            var p = _mgr.CreateProfile("ToDelete");
            _mgr.DeleteProfile(p.ProfileId);
            Assert.AreEqual(0, _mgr.Profiles.Count);
            Assert.IsFalse(_backend.Exists($"profile_{p.ProfileId}_active"));
        }

        [Test]
        public void MaxSlots_EnforcedAtFour()
        {
            for (int i = 0; i < ProfileManager.MaxSlots; i++)
                _mgr.CreateProfile($"Slot{i}");

            Assert.Throws<InvalidOperationException>(() => _mgr.CreateProfile("Overflow"));
        }

        [Test]
        public void Serializer_ChecksumMismatch_ReturnsCorrupted()
        {
            var profile = new SaveProfile
            {
                ProfileId = Guid.NewGuid().ToString("N"),
                DisplayName = "Test",
                SchemaVersion = SaveProfile.CurrentSchemaVersion,
                CreatedAtUtc = DateTime.UtcNow,
                LastSavedAtUtc = DateTime.UtcNow,
            };
            string serialized = SaveSerializer.Serialize(profile);
            string tampered = serialized.Substring(0, serialized.Length - 3) + "XXX";
            var (_, corrupted) = SaveSerializer.Deserialize(tampered);
            Assert.IsTrue(corrupted);
        }

        [Test]
        public void Serializer_ValidData_RoundTrips()
        {
            var profile = new SaveProfile
            {
                ProfileId = Guid.NewGuid().ToString("N"),
                DisplayName = "RoundTrip",
                SchemaVersion = SaveProfile.CurrentSchemaVersion,
                CreatedAtUtc = DateTime.UtcNow,
                LastSavedAtUtc = DateTime.UtcNow,
            };
            profile.Character.Level = 12;
            profile.Flags["tutorial_done"] = "true";

            string serialized = SaveSerializer.Serialize(profile);
            var (loaded, corrupted) = SaveSerializer.Deserialize(serialized);

            Assert.IsFalse(corrupted);
            Assert.AreEqual("RoundTrip", loaded.DisplayName);
            Assert.AreEqual(12, loaded.Character.Level);
            Assert.AreEqual("true", loaded.Flags["tutorial_done"]);
        }

        [Test]
        public void SaveSystemBridge_CaptureAndRestoreInventory_RoundTrips()
        {
            var profile = new SaveProfile
            {
                ProfileId = Guid.NewGuid().ToString("N"),
                DisplayName = "BridgeTest",
                SchemaVersion = SaveProfile.CurrentSchemaVersion,
                CreatedAtUtc = DateTime.UtcNow,
                LastSavedAtUtc = DateTime.UtcNow,
            };

            var sourceInventory = new Inventory();
            sourceInventory.AddCurrency("fate_shards", 250);
            sourceInventory.AddCurrency("hero_medals", 12);
            sourceInventory.AddMaterial("ember_core", 4);

            sourceInventory.AddItem(new ItemInstance
            {
                Definition = new ItemDefinition
                {
                    ItemId = "ember_staff",
                    DisplayName = "Ember Staff",
                    Type = ItemType.Weapon,
                    BasePower = 10
                },
                Rarity = ItemRarity.Epic,
                ItemLevel = 6,
                RolledPower = 32
            });

            SaveSystemBridge.CaptureInventory(profile, sourceInventory);

            var targetInventory = new Inventory();
            SaveSystemBridge.RestoreInventory(profile, targetInventory);

            Assert.AreEqual(1, targetInventory.Items.Count);
            Assert.AreEqual("ember_staff", targetInventory.Items[0].Definition.ItemId);
            Assert.AreEqual(250, targetInventory.Currencies["fate_shards"]);
            Assert.AreEqual(12, targetInventory.Currencies["hero_medals"]);
            Assert.AreEqual(4, targetInventory.Materials["ember_core"]);
        }

        [Test]
        public void SaveSystemBridge_RestoreProgression_RebuildsUnlockedBonuses()
        {
            var profile = new SaveProfile
            {
                ProfileId = Guid.NewGuid().ToString("N"),
                DisplayName = "ProgressBridgeTest",
                SchemaVersion = SaveProfile.CurrentSchemaVersion,
                CreatedAtUtc = DateTime.UtcNow,
                LastSavedAtUtc = DateTime.UtcNow,
            };

            profile.Character.Level = 7;
            profile.Character.Experience = 35;
            profile.Character.SkillPoints = 2;
            profile.Character.UnlockedSkillNodeIds.Add("slash");
            profile.Character.UnlockedSkillNodeIds.Add("shield_bash");

            var progression = new CharacterProgression(BuiltInClasses.Warrior, SkillTreeFactory.CreateWarriorTree());
            SaveSystemBridge.RestoreProgression(profile, progression);

            Assert.AreEqual(7, progression.Level);
            Assert.IsTrue(progression.SkillTree.IsNodeUnlocked("slash"));
            Assert.IsTrue(progression.SkillTree.IsNodeUnlocked("shield_bash"));
            Assert.Greater(progression.GetStatBonus("attack"), 0);
            Assert.Greater(progression.GetStatBonus("defense"), 0);
        }
    }
}
