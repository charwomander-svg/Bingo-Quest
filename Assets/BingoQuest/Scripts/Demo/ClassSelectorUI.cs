using UnityEngine;

namespace BingoQuest.Demo
{
    /// <summary>
    /// Full-screen IMGUI class selector shown at game start (before difficulty selector).
    /// Supports mouse click and 1-5 keyboard shortcuts.
    /// </summary>
    public sealed class ClassSelectorUI : MonoBehaviour
    {
        public bool HasSelected { get; private set; }
        public string SelectedClassId { get; private set; }

        private int hoveredIndex = 0;
        private bool stylesInitialized;

        private static readonly GUIStyle TitleStyle    = new GUIStyle();
        private static readonly GUIStyle SubtitleStyle = new GUIStyle();

        private struct ClassEntry
        {
            public string Id;
            public string Name;
            public string Role;
            public string Description;
            public Color Color;
            public string Stats;
        }

        private static readonly ClassEntry[] Classes =
        {
            new ClassEntry
            {
                Id = "warrior", Name = "Warrior", Role = "Tank / DPS",
                Description = "Stalwart frontline fighter. Shield Bash to stun, Thunder Cleave to devastate. Highest base defense.",
                Color = new Color(0.95f, 0.45f, 0.15f),
                Stats = "HP ●●●●  ATK ●●●  DEF ●●●●●"
            },
            new ClassEntry
            {
                Id = "rogue", Name = "Rogue", Role = "DPS / Evasion",
                Description = "Agile assassin. High crit chance, Smoke Bomb, lethal Venom Blade combo. Low defense.",
                Color = new Color(0.65f, 0.25f, 0.85f),
                Stats = "HP ●●●   ATK ●●●●  DEF ●●"
            },
            new ClassEntry
            {
                Id = "cleric", Name = "Cleric", Role = "Support / DPS",
                Description = "Holy guardian. Healing Prayer for sustain, Consecration burns foes. Moderate all-round stats.",
                Color = new Color(0.90f, 0.78f, 0.20f),
                Stats = "HP ●●●●  ATK ●●●  DEF ●●●●"
            },
            new ClassEntry
            {
                Id = "mage", Name = "Mage", Role = "AoE / Elemental",
                Description = "Arcane powerhouse. Massive area spells with Fire and Frost. Very fragile.",
                Color = new Color(0.20f, 0.55f, 0.95f),
                Stats = "HP ●●    ATK ●●●●●  DEF ●"
            },
            new ClassEntry
            {
                Id = "ranger", Name = "Ranger", Role = "DPS / Precision",
                Description = "Methodical hunter. Consistent damage, nature status effects. Balanced stats.",
                Color = new Color(0.25f, 0.75f, 0.35f),
                Stats = "HP ●●●   ATK ●●●●  DEF ●●●"
            },
        };

        private void Update()
        {
            if (HasSelected) return;

            for (int i = 0; i < Classes.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    Select(i);
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                Select(hoveredIndex);

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                hoveredIndex = Mathf.Max(0, hoveredIndex - 1);

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                hoveredIndex = Mathf.Min(Classes.Length - 1, hoveredIndex + 1);
        }

        private void OnGUI()
        {
            if (HasSelected) return;

            InitStyles();

            // Dark overlay
            GUI.color = new Color(0f, 0f, 0f, 0.90f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;

            GUI.Label(new Rect(cx - 300f, cy - 200f, 600f, 56f), "BINGO QUEST", TitleStyle);
            GUI.Label(new Rect(cx - 300f, cy - 148f, 600f, 34f), "Choose Your Class", SubtitleStyle);

            float cardW = Mathf.Min(168f, (Screen.width - 100f) / Classes.Length);
            float cardH = 200f;
            float totalW = cardW * Classes.Length + 10f * (Classes.Length - 1);
            float startX = cx - totalW * 0.5f;
            float cardY = cy - 80f;

            for (int i = 0; i < Classes.Length; i++)
            {
                var cls = Classes[i];
                bool isHovered = (i == hoveredIndex);
                var rect = new Rect(startX + i * (cardW + 10f), cardY, cardW, cardH);

                // Card background
                var bg = new Color(cls.Color.r * 0.15f, cls.Color.g * 0.15f, cls.Color.b * 0.15f, 0.96f);
                GUI.color = isHovered ? new Color(bg.r * 1.7f, bg.g * 1.7f, bg.b * 1.7f, 1f) : bg;
                GUI.DrawTexture(rect, Texture2D.whiteTexture);

                // Border
                if (isHovered)
                {
                    GUI.color = cls.Color;
                    GUI.DrawTexture(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, 3), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(rect.x - 2, rect.yMax - 1, rect.width + 4, 3), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(rect.x - 2, rect.y - 2, 3, rect.height + 4), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(rect.xMax - 1, rect.y - 2, 3, rect.height + 4), Texture2D.whiteTexture);
                }
                GUI.color = Color.white;

                // [N] key badge
                var badgeStyle = new GUIStyle(GUI.skin.label)
                { fontSize = 12, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = cls.Color } };
                GUI.Label(new Rect(rect.x, rect.y + 6f, rect.width, 18f), $"[{i + 1}]", badgeStyle);

                // Class name
                var nameStyle = new GUIStyle(GUI.skin.label)
                { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = isHovered ? Color.white : new Color(0.85f, 0.85f, 0.85f) } };
                GUI.Label(new Rect(rect.x, rect.y + 26f, rect.width, 26f), cls.Name, nameStyle);

                // Role tag
                var roleStyle = new GUIStyle(GUI.skin.label)
                { fontSize = 10, alignment = TextAnchor.MiddleCenter, normal = { textColor = cls.Color } };
                GUI.Label(new Rect(rect.x, rect.y + 52f, rect.width, 16f), cls.Role, roleStyle);

                // Stat dots
                var statStyle = new GUIStyle(GUI.skin.label)
                { fontSize = 9, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = new Color(0.80f, 0.80f, 0.80f) } };
                GUI.Label(new Rect(rect.x + 4f, rect.y + 70f, rect.width - 8f, 32f), cls.Stats, statStyle);

                // Description
                var descStyle = new GUIStyle(GUI.skin.label)
                { fontSize = 10, wordWrap = true, alignment = TextAnchor.UpperCenter, normal = { textColor = new Color(0.70f, 0.70f, 0.70f) } };
                GUI.Label(new Rect(rect.x + 6f, rect.y + 104f, rect.width - 12f, 84f), cls.Description, descStyle);

                // Click target
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    Select(i);

                if (rect.Contains(Event.current.mousePosition))
                    hoveredIndex = i;
            }

            var hintStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 12, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.50f, 0.50f, 0.50f) } };
            GUI.Label(new Rect(cx - 350f, cardY + cardH + 16f, 700f, 24f),
                "← → navigate  •  [1][2][3][4][5] quick pick  •  Enter / Click to confirm",
                hintStyle);
        }

        private void Select(int index)
        {
            if (index < 0 || index >= Classes.Length) return;
            SelectedClassId = Classes[index].Id;
            HasSelected = true;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;
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
