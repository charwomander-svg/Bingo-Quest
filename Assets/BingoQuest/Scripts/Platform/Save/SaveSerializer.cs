using System;
using System.Text;
using UnityEngine;

namespace BingoQuest.Platform.Save
{
    // ?????????????????????????????????????????????????????????????????????????????
    // Thin wrapper around Unity's JsonUtility with corruption detection via SHA-1
    // checksum and migration hooks keyed on SaveProfile.SchemaVersion.
    // ?????????????????????????????????????????????????????????????????????????????
    public static class SaveSerializer
    {
        private const char ChecksumSep = '|';

        public static string Serialize(SaveProfile profile)
        {
            var wrapper = new SaveWrapper { Version = profile.SchemaVersion, Payload = profile };
            string json = JsonUtility.ToJson(wrapper, prettyPrint: false);
            string checksum = ComputeChecksum(json);
            return checksum + ChecksumSep + json;
        }

        public static (SaveProfile profile, bool corrupted) Deserialize(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return (null, true);

            int sep = raw.IndexOf(ChecksumSep);
            if (sep < 0)
                return (null, true);

            string storedChecksum = raw.Substring(0, sep);
            string json = raw.Substring(sep + 1);

            if (ComputeChecksum(json) != storedChecksum)
                return (null, true);

            try
            {
                var wrapper = JsonUtility.FromJson<SaveWrapper>(json);
                var profile = wrapper?.Payload;
                if (profile == null)
                    return (null, true);

                profile = Migrate(profile, wrapper.Version);
                return (profile, false);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveSerializer] Deserialization error: {ex.Message}");
                return (null, true);
            }
        }

        // ?? Migration ????????????????????????????????????????????????????????????
        private static SaveProfile Migrate(SaveProfile profile, int fromVersion)
        {
            // As the schema advances, add migration steps here:
            // if (fromVersion < 2) { ... }
            // if (fromVersion < 3) { ... }
            profile.SchemaVersion = SaveProfile.CurrentSchemaVersion;
            return profile;
        }

        // ?? Checksum ?????????????????????????????????????????????????????????????
        private static string ComputeChecksum(string input)
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            byte[] bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    // Needed because JsonUtility requires a concrete class wrapper to embed version
    [Serializable]
    internal sealed class SaveWrapper
    {
        public int Version;
        public SaveProfile Payload;
    }
}
