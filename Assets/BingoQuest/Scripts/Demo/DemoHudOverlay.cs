using System;
using System.Text;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Combat;
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
        private string lastPattern = "None";

        public void Bind(Combatant combatant, Func<int> enemyCount, Func<Inventory> inventory, Func<CharacterProgression> progression)
        {
            player = combatant;
            aliveEnemyCount = enemyCount;
            inventoryResolver = inventory;
            progressionResolver = progression;

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
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(12, 12, 520, 760), GUI.skin.box);
            GUILayout.Label("Bingo Quest Playable Demo");
            GUILayout.Label("WASD move | Space attack | Q/W/E/R abilities | Shift dodge | L chest loot | B boss objective");

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

            GUILayout.EndArea();
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
