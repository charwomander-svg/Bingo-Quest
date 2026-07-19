using BingoQuest.Gameplay.Balance;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Progression;
using UnityEngine;

namespace BingoQuest.Gameplay.Difficulty
{
    /// <summary>
    /// Bundled difficulty preset containing all balance configurations.
    /// Designers select a preset and all subsystems tune accordingly.
    /// </summary>
    [CreateAssetMenu(menuName = "BingoQuest/Difficulty Preset", fileName = "DifficultyPreset")]
    public class DifficultyPreset : ScriptableObject
    {
        [System.Serializable]
        public class PresetInfo
        {
            [SerializeField] public string PresetName = "Normal";
            [SerializeField] public string Description = "Balanced experience";
            [SerializeField][Range(0f, 5f)] public float DifficultyRating = 2.0f; // 1 = easiest, 5 = hardest
            [SerializeField] public Color PresetColor = Color.white;
        }

        [SerializeField] private PresetInfo presetInfo = new();

        [Header("Combat & Difficulty")]
        [SerializeField] private BalanceConfig balanceConfig;

        [Header("Progression")]
        [SerializeField] private ProgressionConfig progressionConfig;

        [Header("Loot")]
        [SerializeField] private LootConfig lootConfig;

        [Header("Abilities")]
        [SerializeField] private AbilityConfig abilityConfig;

        public string PresetName => presetInfo.PresetName;
        public string Description => presetInfo.Description;
        public float DifficultyRating => presetInfo.DifficultyRating;
        public Color PresetColor => presetInfo.PresetColor;

        public BalanceConfig BalanceConfig => balanceConfig;
        public ProgressionConfig ProgressionConfig => progressionConfig;
        public LootConfig LootConfig => lootConfig;
        public AbilityConfig AbilityConfig => abilityConfig;

        /// <summary>Check if preset is complete (all configs assigned).</summary>
        public bool IsComplete()
        {
            return balanceConfig != null &&
                   progressionConfig != null &&
                   lootConfig != null &&
                   abilityConfig != null;
        }

        /// <summary>Get list of missing configs for debugging.</summary>
        public string GetMissingConfigs()
        {
            var missing = new System.Collections.Generic.List<string>();
            if (balanceConfig == null) missing.Add("BalanceConfig");
            if (progressionConfig == null) missing.Add("ProgressionConfig");
            if (lootConfig == null) missing.Add("LootConfig");
            if (abilityConfig == null) missing.Add("AbilityConfig");
            return missing.Count == 0 ? "None" : string.Join(", ", missing);
        }

        public override string ToString()
        {
            return $"[{presetInfo.PresetName}] ({presetInfo.DifficultyRating}/5.0)\n" +
                   $"Description: {presetInfo.Description}\n" +
                   $"Balance: {(balanceConfig ? "✓" : "✗")} " +
                   $"Progression: {(progressionConfig ? "✓" : "✗")} " +
                   $"Loot: {(lootConfig ? "✓" : "✗")} " +
                   $"Ability: {(abilityConfig ? "✓" : "✗")}";
        }
    }

    /// <summary>
    /// Manages current difficulty preset and applies it to all systems.
    /// </summary>
    public class DifficultyManager
    {
        private DifficultyPreset currentPreset;
        private static DifficultyManager instance;

        public static DifficultyManager Instance
        {
            get
            {
                instance ??= new DifficultyManager();
                return instance;
            }
        }

        public DifficultyPreset CurrentPreset => currentPreset;
        public event System.Action<DifficultyPreset> OnPresetChanged;

        public DifficultyManager() { }

        public DifficultyManager(DifficultyPreset preset)
        {
            SetPreset(preset);
        }

        /// <summary>Load and apply a difficulty preset.</summary>
        public bool SetPreset(DifficultyPreset preset)
        {
            if (preset == null)
            {
                Debug.LogError("Cannot set null DifficultyPreset");
                return false;
            }

            if (!preset.IsComplete())
            {
                Debug.LogWarning($"Preset '{preset.PresetName}' is incomplete. Missing: {preset.GetMissingConfigs()}");
                return false;
            }

            currentPreset = preset;
            OnPresetChanged?.Invoke(currentPreset);

            Debug.Log($"<b>Difficulty preset loaded:</b> {preset.PresetName} ({preset.DifficultyRating}/5.0)");
            return true;
        }

        /// <summary>Get current balance config, or null if not set.</summary>
        public BalanceConfig GetBalanceConfig() => currentPreset?.BalanceConfig;

        /// <summary>Get current progression config, or null if not set.</summary>
        public ProgressionConfig GetProgressionConfig() => currentPreset?.ProgressionConfig;

        /// <summary>Get current loot config, or null if not set.</summary>
        public LootConfig GetLootConfig() => currentPreset?.LootConfig;

        /// <summary>Get current ability config, or null if not set.</summary>
        public AbilityConfig GetAbilityConfig() => currentPreset?.AbilityConfig;

        /// <summary>Get current difficulty mode from preset rating.</summary>
        public BingoQuest.Gameplay.Progression.DifficultyMode GetDifficultyMode()
        {
            if (currentPreset == null)
                return BingoQuest.Gameplay.Progression.DifficultyMode.Normal;

            float rating = currentPreset.DifficultyRating;
            if (rating <= 1.5f) return BingoQuest.Gameplay.Progression.DifficultyMode.Easy;
            if (rating <= 2.5f) return BingoQuest.Gameplay.Progression.DifficultyMode.Normal;
            if (rating <= 3.5f) return BingoQuest.Gameplay.Progression.DifficultyMode.Hard;
            return BingoQuest.Gameplay.Progression.DifficultyMode.Nightmare;
        }

        /// <summary>Reset to null state (useful for testing).</summary>
        public void Clear()
        {
            currentPreset = null;
        }
    }
}
