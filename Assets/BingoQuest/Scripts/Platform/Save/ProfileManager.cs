using System;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Platform.Save
{
    // ?????????????????????????????????????????????????????????????????????????????
    // ProfileManager - manages up to MaxSlots save profiles, each with an active
    // copy and a backup.  Corruption in the active slot falls back to the backup
    // automatically; the caller decides whether to surface this to the player.
    // ?????????????????????????????????????????????????????????????????????????????
    public sealed class ProfileManager
    {
        public const int MaxSlots = 4;
        private const string ActiveSuffix = "_active";
        private const string BackupSuffix = "_backup";
        private const string IndexKey = "profile_index";

        private readonly ISaveBackend _backend;
        private readonly List<ProfileMeta> _index = new();

        public IReadOnlyList<ProfileMeta> Profiles => _index;
        public BingoQuest.Platform.Save.SaveProfile ActiveProfile { get; private set; }

        public event Action<BingoQuest.Platform.Save.SaveProfile> OnProfileLoaded;
        public event Action<BingoQuest.Platform.Save.SaveProfile> OnProfileSaved;
        public event Action<string> OnCorruptionDetected;

        public ProfileManager(ISaveBackend backend)
        {
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));
            LoadIndex();
        }

        // ?? Create ???????????????????????????????????????????????????????????????
        public BingoQuest.Platform.Save.SaveProfile CreateProfile(string displayName)
        {
            if (_index.Count >= MaxSlots)
                throw new InvalidOperationException($"Maximum of {MaxSlots} save profiles reached.");

            var profile = new BingoQuest.Platform.Save.SaveProfile
            {
                ProfileId = Guid.NewGuid().ToString("N"),
                DisplayName = displayName,
                SchemaVersion = BingoQuest.Platform.Save.SaveProfile.CurrentSchemaVersion,
                CreatedAtUtc = DateTime.UtcNow,
                LastSavedAtUtc = DateTime.UtcNow,
            };

            _index.Add(new ProfileMeta { ProfileId = profile.ProfileId, DisplayName = displayName });
            SaveIndex();
            WriteProfile(profile);
            ActiveProfile = profile;
            return profile;
        }

        // ?? Load ?????????????????????????????????????????????????????????????????
        public bool TryLoadProfile(string profileId, out BingoQuest.Platform.Save.SaveProfile profile)
        {
            profile = null;
            string active = _backend.Read(ActiveKey(profileId));
            if (!string.IsNullOrEmpty(active))
            {
                var (p, corrupted) = SaveSerializer.Deserialize(active);
                if (!corrupted)
                {
                    ActiveProfile = p;
                    profile = p;
                    OnProfileLoaded?.Invoke(p);
                    return true;
                }
                OnCorruptionDetected?.Invoke($"Active save for {profileId} is corrupt; trying backup.");
            }

            string backup = _backend.Read(BackupKey(profileId));
            if (!string.IsNullOrEmpty(backup))
            {
                var (p, corrupted) = SaveSerializer.Deserialize(backup);
                if (!corrupted)
                {
                    Debug.LogWarning($"[ProfileManager] Loaded backup for profile {profileId}.");
                    ActiveProfile = p;
                    profile = p;
                    // Restore backup as active so next save is clean
                    _backend.Write(ActiveKey(profileId), backup);
                    OnProfileLoaded?.Invoke(p);
                    return true;
                }
                OnCorruptionDetected?.Invoke($"Backup save for {profileId} is also corrupt. Profile unrecoverable.");
            }

            return false;
        }

        // ?? Save ?????????????????????????????????????????????????????????????????
        public void SaveProfile(BingoQuest.Platform.Save.SaveProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            profile.LastSavedAtUtc = DateTime.UtcNow;
            WriteProfile(profile);
            ActiveProfile = profile;
            OnProfileSaved?.Invoke(profile);
        }

        // ?? Delete ???????????????????????????????????????????????????????????????
        public void DeleteProfile(string profileId)
        {
            _index.RemoveAll(m => m.ProfileId == profileId);
            _backend.Delete(ActiveKey(profileId));
            _backend.Delete(BackupKey(profileId));
            if (ActiveProfile?.ProfileId == profileId)
                ActiveProfile = null;
            SaveIndex();
        }

        // ?? Internal ?????????????????????????????????????????????????????????????
        private void WriteProfile(BingoQuest.Platform.Save.SaveProfile profile)
        {
            string json = SaveSerializer.Serialize(profile);
            // Promote current active to backup before overwriting
            string existing = _backend.Read(ActiveKey(profile.ProfileId));
            if (!string.IsNullOrEmpty(existing))
                _backend.Write(BackupKey(profile.ProfileId), existing);
            _backend.Write(ActiveKey(profile.ProfileId), json);
        }

        private void LoadIndex()
        {
            string raw = _backend.Read(IndexKey);
            if (string.IsNullOrEmpty(raw)) return;
            try
            {
                var wrapper = JsonUtility.FromJson<IndexWrapper>(raw);
                if (wrapper?.Profiles != null)
                    _index.AddRange(wrapper.Profiles);
            }
            catch { /* corrupted index; start fresh */ }
        }

        private void SaveIndex()
        {
            _backend.Write(IndexKey, JsonUtility.ToJson(new IndexWrapper { Profiles = _index }));
        }

        private static string ActiveKey(string id) => $"profile_{id}{ActiveSuffix}";
        private static string BackupKey(string id) => $"profile_{id}{BackupSuffix}";
    }

    public sealed class ProfileMeta
    {
        public string ProfileId { get; set; }
        public string DisplayName { get; set; }
    }

    [Serializable]
    internal sealed class IndexWrapper
    {
        public List<ProfileMeta> Profiles;
    }
}
