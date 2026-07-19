using System;
using BingoQuest.Gameplay.Difficulty;
using UnityEngine;

namespace BingoQuest.Demo
{
    /// <summary>
    /// Full-screen IMGUI difficulty selector shown before the game starts.
    /// Call SetPresets() then check HasSelected / SelectedPreset each frame.
    /// Supports mouse click and keyboard shortcuts 1-4.
    /// </summary>
    public sealed class DifficultySelectorUI : MonoBehaviour
    {
        public bool HasSelected { get; private set; }
        public DifficultyPreset SelectedPreset { get; private set; }

        private DifficultyPreset[] presets;
        private int hoveredIndex = 1; // default highlight = Normal

        private static readonly string[] Keys = { "1", "2", "3", "4" };

        private static readonly GUIStyle TitleStyle = new GUIStyle();
        private static readonly GUIStyle SubtitleStyle = new GUIStyle();
        private static readonly GUIStyle ButtonStyle = new GUIStyle();
        private static readonly GUIStyle DescStyle = new GUIStyle();
        private static readonly GUIStyle HintStyle = new GUIStyle();

        private bool stylesInitialized;

        public void SetPresets(DifficultyPreset[] allPresets)
        {
            presets = allPresets;
        }

        private void Update()
        {
            if (HasSelected || presets == null)
                return;

            for (int i = 0; i < presets.Length && i < 4; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    SelectPreset(i);
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                SelectPreset(hoveredIndex);

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                hoveredIndex = Mathf.Max(0, hoveredIndex - 1);

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                hoveredIndex = Mathf.Min(presets.Length - 1, hoveredIndex + 1);
        }

        private void OnGUI()
        {
            if (HasSelected || presets == null || presets.Length == 0)
                return;

            InitStyles();

            // Dark overlay
            var screenRect = new Rect(0, 0, Screen.width, Screen.height);
            GUI.color = new Color(0f, 0f, 0f, 0.88f);
            GUI.DrawTexture(screenRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;

            // Title
            GUI.Label(new Rect(cx - 300f, cy - 190f, 600f, 56f), "BINGO QUEST", TitleStyle);
            GUI.Label(new Rect(cx - 300f, cy - 138f, 600f, 34f), "Choose Your Challenge", SubtitleStyle);

            // Preset buttons
            float cardW = Mathf.Min(180f, (Screen.width - 80f) / presets.Length);
            float cardH = 180f;
            float totalW = cardW * presets.Length + 12f * (presets.Length - 1);
            float startX = cx - totalW * 0.5f;
            float cardY = cy - 60f;

            for (int i = 0; i < presets.Length; i++)
            {
                var preset = presets[i];
                bool isHovered = (i == hoveredIndex);
                var cardRect = new Rect(startX + i * (cardW + 12f), cardY, cardW, cardH);

                // Card background
                var bg = new Color(preset.PresetColor.r * 0.18f, preset.PresetColor.g * 0.18f, preset.PresetColor.b * 0.18f, 0.95f);
                GUI.color = isHovered ? new Color(bg.r * 1.6f, bg.g * 1.6f, bg.b * 1.6f, 1f) : bg;
                GUI.DrawTexture(cardRect, Texture2D.whiteTexture);

                // Border highlight on hover
                if (isHovered)
                {
                    GUI.color = preset.PresetColor;
                    GUI.DrawTexture(new Rect(cardRect.x - 2, cardRect.y - 2, cardRect.width + 4, 3), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(cardRect.x - 2, cardRect.yMax - 1, cardRect.width + 4, 3), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(cardRect.x - 2, cardRect.y - 2, 3, cardRect.height + 4), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(cardRect.xMax - 1, cardRect.y - 2, 3, cardRect.height + 4), Texture2D.whiteTexture);
                }

                GUI.color = Color.white;

                // Key badge
                var badgeStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = preset.PresetColor }
                };
                GUI.Label(new Rect(cardRect.x, cardRect.y + 8f, cardRect.width, 20f), $"[{Keys[i]}]", badgeStyle);

                // Name
                var nameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    normal = { textColor = isHovered ? Color.white : new Color(0.85f, 0.85f, 0.85f) }
                };
                GUI.Label(new Rect(cardRect.x, cardRect.y + 30f, cardRect.width, 28f), preset.PresetName, nameStyle);

                // Stars
                float stars = preset.DifficultyRating;
                var starStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = preset.PresetColor }
                };
                string starText = BuildStarString(stars);
                GUI.Label(new Rect(cardRect.x, cardRect.y + 60f, cardRect.width, 22f), starText, starStyle);

                // Description
                var descStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.UpperCenter,
                    wordWrap = true,
                    normal = { textColor = new Color(0.72f, 0.72f, 0.72f) }
                };
                GUI.Label(new Rect(cardRect.x + 6f, cardRect.y + 88f, cardRect.width - 12f, 72f), preset.Description, descStyle);

                // Invisible button for click
                if (GUI.Button(cardRect, GUIContent.none, GUIStyle.none))
                    SelectPreset(i);

                // Track hover via mouse position
                if (cardRect.Contains(Event.current.mousePosition))
                    hoveredIndex = i;
            }

            // Bottom hint
            var hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) }
            };
            GUI.Label(new Rect(cx - 350f, cardY + cardH + 18f, 700f, 24f),
                "← → navigate  •  [1][2][3][4] quick pick  •  Enter / Click to confirm",
                hintStyle);
        }

        private static string BuildStarString(float rating)
        {
            int filled = Mathf.RoundToInt(rating);
            int total = 5;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < total; i++)
                sb.Append(i < filled ? "★" : "☆");
            return sb.ToString();
        }

        private void SelectPreset(int index)
        {
            if (index < 0 || presets == null || index >= presets.Length)
                return;

            SelectedPreset = presets[index];
            HasSelected = true;
        }

        private void InitStyles()
        {
            if (stylesInitialized)
                return;

            TitleStyle.fontSize = 38;
            TitleStyle.fontStyle = FontStyle.Bold;
            TitleStyle.alignment = TextAnchor.MiddleCenter;
            TitleStyle.normal.textColor = Color.white;

            SubtitleStyle.fontSize = 18;
            SubtitleStyle.alignment = TextAnchor.MiddleCenter;
            SubtitleStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            stylesInitialized = true;
        }
    }
}
