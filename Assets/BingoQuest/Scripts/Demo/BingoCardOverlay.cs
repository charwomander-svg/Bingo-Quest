using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Objectives;
using UnityEngine;

namespace BingoQuest.Demo
{
    /// <summary>
    /// IMGUI full-screen Bingo card overlay. Press C to toggle.
    /// Shows all 25 squares with short labels, progress bars, completion state,
    /// and highlights completed rows/columns/diagonals.
    /// </summary>
    public sealed class BingoCardOverlay : MonoBehaviour
    {
        public bool IsVisible { get; set; } = false;

        // Flash state for newly completed squares
        private readonly Dictionary<string, float> _completionFlashTimers = new();
        private readonly HashSet<string> _knownComplete = new();

        // Color palette
        private static readonly Color ColComplete    = new Color(0.25f, 0.85f, 0.35f, 1f);
        private static readonly Color ColInProgress  = new Color(0.90f, 0.80f, 0.20f, 1f);
        private static readonly Color ColEmpty       = new Color(0.55f, 0.58f, 0.65f, 1f);
        private static readonly Color ColFlash       = new Color(1.00f, 1.00f, 0.30f, 1f);
        private static readonly Color ColPatternLine = new Color(0.30f, 0.70f, 1.00f, 0.22f);
        private static readonly Color ColBg          = new Color(0.06f, 0.07f, 0.10f, 0.94f);

        private const float FlashDuration = 1.8f;
        private const float CardLeft      = 60f;
        private const float CardTop       = 60f;
        private const float CellSize      = 128f;
        private const float CellPad       = 6f;
        private const int   GridSize      = BingoCard.GRID_SIZE;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
                IsVisible = !IsVisible;

            // Tick flash timers
            var toRemove = new List<string>();
            foreach (var key in _completionFlashTimers.Keys)
            {
                _completionFlashTimers[key] -= Time.deltaTime;
                if (_completionFlashTimers[key] <= 0f)
                    toRemove.Add(key);
            }
            foreach (var k in toRemove)
                _completionFlashTimers.Remove(k);
        }

        private void OnGUI()
        {
            if (!IsVisible)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.55f);
                GUI.Label(new Rect(Screen.width - 120f, Screen.height - 28f, 114f, 22f), "[C] Bingo Card");
                GUI.color = Color.white;
                return;
            }

            var system = BingoSystem.Instance;
            var card   = system?.GetCard();

            float totalW = GridSize * (CellSize + CellPad) - CellPad + 24f;
            float totalH = GridSize * (CellSize + CellPad) - CellPad + 72f;
            float left   = CardLeft;
            float top    = CardTop;

            // Background panel
            GUI.color = ColBg;
            GUI.DrawTexture(new Rect(left - 12f, top - 12f, totalW, totalH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Title row
            GUI.Label(new Rect(left, top, totalW - 24f, 26f), card != null
                ? $"Bingo Card  —  {card.CompletedSquareCount} / {GridSize * GridSize} squares"
                : "Bingo Card  —  (no active run)");
            GUI.Label(new Rect(left + totalW - 140f, top, 130f, 22f), "[C] close");

            top += 32f;

            if (card == null || system == null)
                return;

            // Detect flash triggers for newly completed squares
            for (int i = 0; i < card.Squares.Length; i++)
            {
                var sq = card.Squares[i];
                if (sq.IsCompleted && !_knownComplete.Contains(sq.ObjectiveId))
                {
                    _knownComplete.Add(sq.ObjectiveId);
                    _completionFlashTimers[sq.ObjectiveId] = FlashDuration;
                }
            }

            // Draw pattern highlight bands first (behind cells)
            DrawPatternHighlights(left, top, card, system);

            // Draw cells
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    float cx = left + col * (CellSize + CellPad);
                    float cy = top  + row * (CellSize + CellPad);
                    DrawCell(new Rect(cx, cy, CellSize, CellSize), row, col, card, system);
                }
            }
        }

        private void DrawPatternHighlights(float left, float top, BingoCard card, BingoSystem system)
        {
            // Highlight complete rows
            for (int row = 0; row < GridSize; row++)
            {
                bool full = true;
                for (int col = 0; col < GridSize; col++)
                    if (!card.GetSquare(row, col).IsCompleted) { full = false; break; }

                if (!full)
                    continue;

                float hy = top + row * (CellSize + CellPad) - 3f;
                GUI.color = ColPatternLine;
                GUI.DrawTexture(new Rect(left - 3f, hy, GridSize * (CellSize + CellPad) - CellPad + 6f, CellSize + 6f), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // Highlight complete columns
            for (int col = 0; col < GridSize; col++)
            {
                bool full = true;
                for (int row = 0; row < GridSize; row++)
                    if (!card.GetSquare(row, col).IsCompleted) { full = false; break; }

                if (!full)
                    continue;

                float hx = left + col * (CellSize + CellPad) - 3f;
                GUI.color = ColPatternLine;
                GUI.DrawTexture(new Rect(hx, top - 3f, CellSize + 6f, GridSize * (CellSize + CellPad) - CellPad + 6f), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
        }

        private void DrawCell(Rect cell, int row, int col, BingoCard card, BingoSystem system)
        {
            var square    = card.GetSquare(row, col);
            var objective = system.GetObjective(square.ObjectiveId);

            bool complete   = square.IsCompleted;
            bool flashing   = _completionFlashTimers.TryGetValue(square.ObjectiveId, out float flashT);
            float flashFrac = flashing ? (flashT / FlashDuration) : 0f;

            // Cell background
            Color bg = complete
                ? Color.Lerp(ColComplete, ColFlash, flashing ? flashFrac * 0.6f : 0f)
                : (objective != null && objective.CurrentProgress > 0 ? ColInProgress : ColEmpty);
            bg.a = 0.82f;

            GUI.color = bg;
            GUI.DrawTexture(cell, Texture2D.whiteTexture);

            // Thin border
            GUI.color = complete ? new Color(0.2f, 1f, 0.3f, 0.9f) : new Color(0f, 0f, 0f, 0.5f);
            GUI.DrawTexture(new Rect(cell.x,              cell.y,              cell.width, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cell.x,              cell.yMax - 2f,      cell.width, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cell.x,              cell.y,              2f, cell.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cell.xMax - 2f,      cell.y,              2f, cell.height), Texture2D.whiteTexture);

            GUI.color = Color.white;

            // Grid coordinate label (tiny, top-left corner)
            var coordStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                normal   = { textColor = new Color(1f, 1f, 1f, 0.35f) },
                alignment = TextAnchor.UpperLeft
            };
            GUI.Label(new Rect(cell.x + 4f, cell.y + 2f, 28f, 14f), $"{row},{col}", coordStyle);

            // Completion check mark (large)
            if (complete)
            {
                var checkStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize  = 28,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal    = { textColor = Color.white }
                };
                GUI.Label(new Rect(cell.x, cell.y, cell.width, 30f), "✓", checkStyle);
            }

            // Objective display name — derived from objective ID (strip suffix, capitalise)
            string label = DeriveLabel(square.ObjectiveId, objective);
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 10,
                wordWrap  = true,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = complete ? FontStyle.Bold : FontStyle.Normal,
                normal    = { textColor = complete ? Color.white : new Color(0.9f, 0.9f, 0.9f, 1f) }
            };

            float labelTop = complete ? cell.y + 28f : cell.y + 6f;
            GUI.Label(new Rect(cell.x + 4f, labelTop, cell.width - 8f, 48f), label, labelStyle);

            // Progress bar (only when in progress, not yet complete)
            if (!complete && objective != null && objective.RequiredProgress > 1)
            {
                float pct = Mathf.Clamp01((float)objective.CurrentProgress / objective.RequiredProgress);
                float barW = cell.width - 12f;
                float barY = cell.yMax - 22f;

                // Bar background
                GUI.color = new Color(0f, 0f, 0f, 0.55f);
                GUI.DrawTexture(new Rect(cell.x + 6f, barY, barW, 8f), Texture2D.whiteTexture);

                // Bar fill
                GUI.color = pct >= 0.75f ? new Color(0.3f, 0.9f, 0.3f, 0.9f) :
                            pct >= 0.40f ? new Color(0.9f, 0.8f, 0.1f, 0.9f) :
                                           new Color(0.5f, 0.6f, 0.9f, 0.9f);
                if (pct > 0f)
                    GUI.DrawTexture(new Rect(cell.x + 6f, barY, barW * pct, 8f), Texture2D.whiteTexture);

                GUI.color = Color.white;

                // Progress text
                var progressStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize  = 9,
                    alignment = TextAnchor.LowerCenter,
                    normal    = { textColor = new Color(0.9f, 0.9f, 0.9f, 0.85f) }
                };
                GUI.Label(new Rect(cell.x + 4f, barY - 12f, cell.width - 8f, 14f),
                    $"{objective.CurrentProgress}/{objective.RequiredProgress}", progressStyle);
            }
            else if (complete && objective != null)
            {
                var doneStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize  = 9,
                    alignment = TextAnchor.LowerCenter,
                    normal    = { textColor = new Color(0.8f, 1f, 0.8f, 0.85f) }
                };
                GUI.Label(new Rect(cell.x + 4f, cell.yMax - 20f, cell.width - 8f, 18f), "DONE", doneStyle);
            }
        }

        private static string DeriveLabel(string objectiveId, IObjective objective)
        {
            if (string.IsNullOrWhiteSpace(objectiveId))
                return "???";

            // Strip trailing _01 / _25 suffix added by ContentFactory
            string id = objectiveId;
            int underscorePos = id.LastIndexOf('_');
            if (underscorePos > 0 && underscorePos < id.Length - 1)
            {
                string suffix = id.Substring(underscorePos + 1);
                if (suffix.Length == 2 && char.IsDigit(suffix[0]) && char.IsDigit(suffix[1]))
                    id = id.Substring(0, underscorePos);
            }

            // Replace underscores with spaces, title-case each word
            string[] words = id.Split('_');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0)
                    continue;
                words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1);
            }

            string label = string.Join(" ", words);

            // Append required progress hint if available
            if (objective != null && objective.RequiredProgress > 1)
                label += $"\n×{objective.RequiredProgress}";

            return label;
        }
    }
}
