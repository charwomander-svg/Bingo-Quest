using System;
using System.Text;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Difficulty;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Progression;
using UnityEngine;

namespace BingoQuest.Demo
{
    public class DemoHudOverlay : MonoBehaviour
    {
        private Combatant player;
        private Func<int> aliveEnemyCount;
        private Func<Inventory> inventoryResolver;
        private Func<CharacterProgression> progressionResolver;
        private Func<string> regionNameResolver;
        private Func<float> regionEnemyHealthMultiplierResolver;
        private Func<float> regionEnemyAttackMultiplierResolver;
        private Func<string> saveProfileNameResolver;
        private string lastPattern = "None";
        private bool showBalanceDashboard = true;

        public void Bind(
            Combatant combatant,
            Func<int> enemyCount,
            Func<Inventory> inventory,
            Func<CharacterProgression> progression,
            Func<string> regionName = null,
            Func<float> regionEnemyHealthMultiplier = null,
            Func<float> regionEnemyAttackMultiplier = null,
            Func<string> saveProfileName = null)
        {
            player = combatant;
            aliveEnemyCount = enemyCount;
            inventoryResolver = inventory;
            progressionResolver = progression;
            regionNameResolver = regionName;
            regionEnemyHealthMultiplierResolver = regionEnemyHealthMultiplier;
            regionEnemyAttackMultiplierResolver = regionEnemyAttackMultiplier;
            saveProfileNameResolver = saveProfileName;

            if (BingoSystem.Instance != null)
                BingoSystem.Instance.OnPatternDetected += OnPatternDetected;
        }

        private void OnDestroy()
        {
            if (BingoSystem.Instance != null)
                BingoSystem.Instance.OnPatternDetected -= OnPatternDetected;
        }

        private void OnPatternDetected(BingoPattern pattern)
        {
            lastPattern = pattern.ToString();
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F3)
                showBalanceDashboard = !showBalanceDashboard;

            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(12, 12, 520, 940), GUI.skin.box);
            GUILayout.Label("Bingo Quest Playable Demo");
            GUILayout.Label("WASD move | Space attack | Q/W/E/R abilities | Shift dodge | L chest loot | B boss objective | N next region");
            GUILayout.Label("[1/2/3/4] difficulty | F3 balance dashboard | F5 save | F9 load");
            GUILayout.Label($"Region: {regionNameResolver?.Invoke() ?? "Unknown"}");
            GUILayout.Label($"Save Profile: {saveProfileNameResolver?.Invoke() ?? "No Profile"}");

            // Difficulty indicator
            var preset = DifficultyManager.Instance?.CurrentPreset;
            if (preset != null)
            {
                GUI.color = preset.PresetColor;
                GUILayout.Label($"Difficulty: {preset.PresetName}");
                GUI.color = Color.white;
            }

            if (player != null)
            {
                GUILayout.Space(8);
                GUILayout.Label($"Player HP: {player.Stats.Health}/{player.Stats.MaxHealth}");
                GUILayout.Label($"Enemies Alive: {aliveEnemyCount?.Invoke() ?? 0}");

                var actionBar = player.ActionBar;
                if (actionBar != null)
                {
                    GUILayout.Label($"Q {FormatAbility(actionBar, AbilitySlot.Primary)}");
                    GUILayout.Label($"W {FormatAbility(actionBar, AbilitySlot.Secondary)}");
                    GUILayout.Label($"E {FormatAbility(actionBar, AbilitySlot.Tertiary)}");
                    GUILayout.Label($"R {FormatAbility(actionBar, AbilitySlot.Ultimate)}");
                }
            }

            if (BingoSystem.Instance != null)
            {
                GUILayout.Space(8);
                int completed = BingoSystem.Instance.GetCompletedSquareCount();
                GUILayout.Label($"Bingo Card Progress: {completed}/25");
                GUILayout.Label($"Last Pattern: {lastPattern}");
            }

            var progression = progressionResolver?.Invoke();
            if (progression != null)
            {
                GUILayout.Space(8);
                GUILayout.Label($"Class: {progression.Class.ClassName}");
                GUILayout.Label($"Level: {progression.Level} | Skill Points: {progression.SkillPoints}");
                GUILayout.Label($"EXP to next level: {progression.GetExperienceForNextLevel()}");
            }

            var inventory = inventoryResolver?.Invoke();
            if (inventory != null)
            {
                GUILayout.Space(8);
                GUILayout.Label($"Inventory Items: {inventory.Items.Count}");
                var sb = new StringBuilder();
                foreach (var pair in inventory.Currencies)
                {
                    if (sb.Length > 0)
                        sb.Append(" | ");
                    sb.Append(pair.Key);
                    sb.Append(": ");
                    sb.Append(pair.Value);
                }

                GUILayout.Label(sb.Length == 0 ? "Currencies: None" : $"Currencies: {sb}");
            }

            if (showBalanceDashboard)
            {
                DrawBalanceDashboard();
            }

            GUILayout.EndArea();
        }

        private void DrawBalanceDashboard()
        {
            GUILayout.Space(8);
            GUILayout.Label("Balance Dashboard");

            var manager = DifficultyManager.Instance;
            var preset = manager?.CurrentPreset;
            var mode = manager != null ? manager.GetDifficultyMode() : Progression.DifficultyMode.Normal;
            float difficultyMultiplier = manager?.GetBalanceConfig()?.GetDifficultyMultiplier(mode) ?? 1f;
            float cooldownMultiplier = manager?.GetAbilityConfig()?.GetDifficultyCooldownMultiplier(mode) ?? 1f;
            float xpMultiplier = manager?.GetProgressionConfig()?.GetDifficultyXPMultiplier(mode) ?? 1f;

            float regionHp = regionEnemyHealthMultiplierResolver?.Invoke() ?? 1f;
            float regionAtk = regionEnemyAttackMultiplierResolver?.Invoke() ?? 1f;

            GUILayout.Label($"Preset: {preset?.PresetName ?? "None"} ({mode})");
            GUILayout.Label($"Enemy Difficulty Mult: {difficultyMultiplier:F2}x");
            GUILayout.Label($"Region HP/ATK Mult: {regionHp:F2}x / {regionAtk:F2}x");
            GUILayout.Label($"Effective Enemy HP/ATK: {(regionHp * difficultyMultiplier):F2}x / {(regionAtk * difficultyMultiplier):F2}x");
            GUILayout.Label($"XP Multiplier: {xpMultiplier:F2}x");
            GUILayout.Label($"Cooldown Multiplier: {cooldownMultiplier:F2}x");
        }

        private static string FormatAbility(ActionBar bar, AbilitySlot slot)
        {
            var ability = bar.GetAbility(slot);
            if (ability == null)
                return "Unbound";

            return ability.IsReady
                ? $"{ability.Definition.Name} [READY]"
                : $"{ability.Definition.Name} [{ability.RemainingCooldown:F1}s]";
        }
    }
}
