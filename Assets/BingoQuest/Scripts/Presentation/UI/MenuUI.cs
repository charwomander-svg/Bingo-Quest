using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace BingoQuest.Presentation.UI
{
    /// <summary>
    /// Inventory UI displaying items, rarity colors, and comparison features.
    /// Supports drag-and-drop, item inspection, and filtering by type.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [System.Serializable]
        public class RarityColor
        {
            public string rarityName;
            public Color color;
        }

        [SerializeField] private GridLayoutGroup inventoryGrid;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private Image itemIconPreview;
        [SerializeField] private TextMeshProUGUI itemStatsText;
        [SerializeField] private Image rarityColorIndicator;
        [SerializeField] private List<RarityColor> rarityColors = new();
        [SerializeField] private Button sortByNameButton;
        [SerializeField] private Button sortByRarityButton;
        [SerializeField] private Button filterAllButton;
        [SerializeField] private Button filterWeaponsButton;
        [SerializeField] private Button filterArmorButton;
        [SerializeField] private Button filterAccessoriesButton;

        private Dictionary<string, Color> rarityColorMap = new();
        private List<GameObject> itemSlots = new();
        private GameObject selectedSlot;
        private List<InventoryItem> currentItems = new();
        private string currentFilter = "All";

        public event System.Action<InventoryItem> OnItemSelected;
        public event System.Action<InventoryItem> OnItemEquipped;
        public event System.Action<InventoryItem> OnItemConsumed;

        private void Start()
        {
            // Build rarity color map
            foreach (var rarity in rarityColors)
            {
                rarityColorMap[rarity.rarityName] = rarity.color;
            }

            // Wire button clicks
            if (sortByNameButton) sortByNameButton.onClick.AddListener(() => SortInventory("Name"));
            if (sortByRarityButton) sortByRarityButton.onClick.AddListener(() => SortInventory("Rarity"));
            if (filterAllButton) filterAllButton.onClick.AddListener(() => FilterInventory("All"));
            if (filterWeaponsButton) filterWeaponsButton.onClick.AddListener(() => FilterInventory("Weapon"));
            if (filterArmorButton) filterArmorButton.onClick.AddListener(() => FilterInventory("Armor"));
            if (filterAccessoriesButton) filterAccessoriesButton.onClick.AddListener(() => FilterInventory("Accessory"));
        }

        /// <summary>Display inventory items in grid.</summary>
        public void RefreshInventory(List<InventoryItem> items)
        {
            currentItems = new List<InventoryItem>(items);
            ApplyFilter();
        }

        /// <summary>Select an inventory item for inspection.</summary>
        public void SelectItem(InventoryItem item, GameObject slot)
        {
            if (selectedSlot)
                selectedSlot.GetComponent<Image>().color = Color.white;

            selectedSlot = slot;
            slot.GetComponent<Image>().color = new Color(0.7f, 0.7f, 1f); // Highlight

            // Update detail panel
            itemNameText.text = item.Name;
            itemDescriptionText.text = item.Description;
            itemIconPreview.sprite = item.Icon;
            itemStatsText.text = FormatItemStats(item);

            // Set rarity color
            if (rarityColorMap.TryGetValue(item.Rarity, out Color rarityColor))
                rarityColorIndicator.color = rarityColor;

            OnItemSelected?.Invoke(item);
        }

        /// <summary>Equip selected item.</summary>
        public void EquipSelectedItem()
        {
            if (selectedSlot && currentItems.Count > 0)
            {
                var item = GetItemFromSlot(selectedSlot);
                if (item != null)
                    OnItemEquipped?.Invoke(item);
            }
        }

        /// <summary>Consume selected item (food, potions, etc).</summary>
        public void ConsumeSelectedItem()
        {
            if (selectedSlot && currentItems.Count > 0)
            {
                var item = GetItemFromSlot(selectedSlot);
                if (item != null && item.IsConsumable)
                    OnItemConsumed?.Invoke(item);
            }
        }

        /// <summary>Display item comparison (selected vs equipped).</summary>
        public void CompareItem(InventoryItem equipped)
        {
            if (selectedSlot && currentItems.Count > 0)
            {
                var selectedItem = GetItemFromSlot(selectedSlot);
                if (selectedItem == null) return;

                string comparison = $"<b>{selectedItem.Name}</b> vs <b>{equipped.Name}</b>\n\n";
                comparison += $"Damage: {selectedItem.Damage} → {equipped.Damage}\n";
                comparison += $"Defense: {selectedItem.Defense} → {equipped.Defense}\n";
                comparison += $"Level Req: {selectedItem.LevelRequired} vs {equipped.LevelRequired}";

                itemStatsText.text = comparison;
            }
        }

        private void ApplyFilter()
        {
            List<InventoryItem> filtered = new();

            foreach (var item in currentItems)
            {
                if (currentFilter == "All" || item.Type == currentFilter)
                    filtered.Add(item);
            }

            DisplayItems(filtered);
        }

        private void SortInventory(string sortBy)
        {
            if (sortBy == "Name")
                currentItems.Sort((a, b) => a.Name.CompareTo(b.Name));
            else if (sortBy == "Rarity")
                currentItems.Sort((a, b) => GetRarityValue(b.Rarity).CompareTo(GetRarityValue(a.Rarity)));

            ApplyFilter();
        }

        private void FilterInventory(string filterType)
        {
            currentFilter = filterType;
            ApplyFilter();
        }

        private void DisplayItems(List<InventoryItem> items)
        {
            // Clear existing slots
            foreach (var slot in itemSlots)
                Destroy(slot);
            itemSlots.Clear();

            // Create slots for filtered items
            foreach (var item in items)
            {
                GameObject slot = Instantiate(itemSlotPrefab, inventoryGrid.transform);
                var image = slot.GetComponent<Image>();
                image.sprite = item.Icon;
                image.color = rarityColorMap.TryGetValue(item.Rarity, out Color c) ? c : Color.white;

                var button = slot.GetComponent<Button>();
                button.onClick.AddListener(() => SelectItem(item, slot));

                // Store item reference (simplified)
                slot.name = item.Name;
                itemSlots.Add(slot);
            }
        }

        private InventoryItem GetItemFromSlot(GameObject slot)
        {
            foreach (var item in currentItems)
            {
                if (item.Name == slot.name)
                    return item;
            }
            return null;
        }

        private string FormatItemStats(InventoryItem item)
        {
            string stats = $"<b>Rarity:</b> {item.Rarity}\n";
            stats += $"<b>Type:</b> {item.Type}\n";
            stats += $"<b>Level Req:</b> {item.LevelRequired}\n";
            if (item.Damage > 0) stats += $"<b>Damage:</b> +{item.Damage}\n";
            if (item.Defense > 0) stats += $"<b>Defense:</b> +{item.Defense}\n";
            stats += $"<b>Value:</b> {item.GoldValue}g";
            return stats;
        }

        private int GetRarityValue(string rarity)
        {
            return rarity switch
            {
                "Common" => 1,
                "Uncommon" => 2,
                "Rare" => 3,
                "Epic" => 4,
                "Legendary" => 5,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Simplified inventory item data structure.
    /// </summary>
    public class InventoryItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Sprite Icon { get; set; }
        public string Rarity { get; set; } // Common, Uncommon, Rare, Epic, Legendary
        public string Type { get; set; } // Weapon, Armor, Accessory, Consumable
        public int Damage { get; set; }
        public int Defense { get; set; }
        public int LevelRequired { get; set; }
        public int GoldValue { get; set; }
        public bool IsConsumable { get; set; }
    }

    /// <summary>
    /// Character sheet UI showing character stats, equipped items, and progression.
    /// </summary>
    public class CharacterSheetUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image characterPortrait;
        [SerializeField] private Slider experienceSlider;
        [SerializeField] private TextMeshProUGUI experienceText;

        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI defenseText;
        [SerializeField] private TextMeshProUGUI critChanceText;
        [SerializeField] private TextMeshProUGUI attackSpeedText;

        [SerializeField] private Image equippedWeapon;
        [SerializeField] private Image equippedArmor;
        [SerializeField] private Image equippedAccessory;
        [SerializeField] private TextMeshProUGUI equippedStatsText;

        [SerializeField] private VerticalLayoutGroup attributesGroup;
        [SerializeField] private TextMeshProUGUI attributesText;

        public event System.Action<int> OnAttributePointSpent;

        /// <summary>Update character sheet display.</summary>
        public void RefreshCharacterSheet(CharacterData character)
        {
            characterNameText.text = character.Name;
            levelText.text = $"Level {character.Level}";
            characterPortrait.sprite = character.PortraitIcon;

            // Experience bar
            float expPercent = (float)character.CurrentXP / character.XPForNextLevel;
            experienceSlider.value = expPercent;
            experienceText.text = $"{character.CurrentXP}/{character.XPForNextLevel} XP";

            // Core stats
            hpText.text = $"{character.CurrentHP}/{character.MaxHP}";
            manaText.text = $"{character.CurrentMana}/{character.MaxMana}";
            damageText.text = $"{character.Damage}";
            defenseText.text = $"{character.Defense}";
            critChanceText.text = $"{character.CritChance:F1}%";
            attackSpeedText.text = $"{character.AttackSpeed:F2}x";

            // Equipped items
            if (character.EquippedWeapon != null)
                equippedWeapon.sprite = character.EquippedWeapon.Icon;
            if (character.EquippedArmor != null)
                equippedArmor.sprite = character.EquippedArmor.Icon;
            if (character.EquippedAccessory != null)
                equippedAccessory.sprite = character.EquippedAccessory.Icon;

            // Attributes
            UpdateAttributesDisplay(character);
        }

        private void UpdateAttributesDisplay(CharacterData character)
        {
            attributesText.text = $"<b>Attributes</b>\n" +
                $"Strength: {character.Strength}\n" +
                $"Dexterity: {character.Dexterity}\n" +
                $"Intelligence: {character.Intelligence}\n" +
                $"Vitality: {character.Vitality}\n" +
                $"\n<b>Available Points:</b> {character.AttributePoints}";
        }

        /// <summary>Display character comparison with another character.</summary>
        public void CompareCharacter(CharacterData other)
        {
            string comparison = "<b>Comparison</b>\n";
            comparison += "--- Your Stats ---\n";
            comparison += $"Damage: {other.Damage}\n";
            comparison += $"Defense: {other.Defense}\n";
            comparison += $"HP: {other.MaxHP}\n";
            comparison += $"Crit: {other.CritChance:F1}%\n";

            attributesText.text = comparison;
        }
    }

    /// <summary>
    /// Simplified character data structure for UI display.
    /// </summary>
    public class CharacterData
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public Sprite PortraitIcon { get; set; }
        public int CurrentXP { get; set; }
        public int XPForNextLevel { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int CurrentMana { get; set; }
        public int MaxMana { get; set; }
        public int Damage { get; set; }
        public int Defense { get; set; }
        public float CritChance { get; set; }
        public float AttackSpeed { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Intelligence { get; set; }
        public int Vitality { get; set; }
        public int AttributePoints { get; set; }
        public InventoryItem EquippedWeapon { get; set; }
        public InventoryItem EquippedArmor { get; set; }
        public InventoryItem EquippedAccessory { get; set; }
    }

    /// <summary>
    /// Skill tree UI with node-based skill selection and respec system.
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        [System.Serializable]
        public class SkillNodeUI
        {
            public Button nodeButton;
            public Image nodeIcon;
            public TextMeshProUGUI nodeNameText;
            public TextMeshProUGUI nodeDescriptionText;
            public bool isUnlocked;
            public int skillId;
        }

        [SerializeField] private List<SkillNodeUI> skillNodes = new();
        [SerializeField] private TextMeshProUGUI selectedSkillNameText;
        [SerializeField] private TextMeshProUGUI selectedSkillDescriptionText;
        [SerializeField] private TextMeshProUGUI skillPointsText;
        [SerializeField] private Button unlockSkillButton;
        [SerializeField] private Button respecButton;
        [SerializeField] private TextMeshProUGUI respecCostText;

        private int availableSkillPoints;
        private SkillNodeUI selectedNode;
        private int respecCost = 1000; // Gold cost to respec

        public event System.Action<int> OnSkillUnlocked;
        public event System.Action OnRespecRequested;

        private void Start()
        {
            // Wire up skill node buttons
            for (int i = 0; i < skillNodes.Count; i++)
            {
                int index = i;
                skillNodes[i].nodeButton.onClick.AddListener(() => SelectSkillNode(skillNodes[index]));
            }

            if (unlockSkillButton) unlockSkillButton.onClick.AddListener(UnlockSelectedSkill);
            if (respecButton) respecButton.onClick.AddListener(RequestRespec);
        }

        /// <summary>Refresh skill tree display with current state.</summary>
        public void RefreshSkillTree(int skillPoints, List<int> unlockedSkills)
        {
            availableSkillPoints = skillPoints;
            skillPointsText.text = $"Skill Points: {availableSkillPoints}";

            // Update node states
            foreach (var node in skillNodes)
            {
                node.isUnlocked = unlockedSkills.Contains(node.skillId);
                node.nodeButton.image.color = node.isUnlocked ? new Color(0.5f, 1f, 0.5f) : Color.gray;
                node.nodeNameText.text = node.skillId.ToString(); // Placeholder
            }
        }

        /// <summary>Select a skill node to view/unlock.</summary>
        public void SelectSkillNode(SkillNodeUI node)
        {
            selectedNode = node;
            selectedSkillNameText.text = $"Skill {node.skillId}";
            selectedSkillDescriptionText.text = GetSkillDescription(node.skillId);

            // Enable/disable unlock button based on state
            if (unlockSkillButton)
                unlockSkillButton.interactable = !node.isUnlocked && availableSkillPoints > 0;
        }

        /// <summary>Unlock selected skill if conditions met.</summary>
        public void UnlockSelectedSkill()
        {
            if (selectedNode != null && !selectedNode.isUnlocked && availableSkillPoints > 0)
            {
                selectedNode.isUnlocked = true;
                availableSkillPoints--;
                skillPointsText.text = $"Skill Points: {availableSkillPoints}";
                selectedNode.nodeButton.image.color = new Color(0.5f, 1f, 0.5f);

                OnSkillUnlocked?.Invoke(selectedNode.skillId);
            }
        }

        /// <summary>Request full skill tree respec (expensive).</summary>
        public void RequestRespec()
        {
            OnRespecRequested?.Invoke();
        }

        /// <summary>Confirm respec and refund skill points.</summary>
        public void ConfirmRespec(int refundedPoints)
        {
            availableSkillPoints = refundedPoints;
            skillPointsText.text = $"Skill Points: {availableSkillPoints}";

            // Reset all nodes
            foreach (var node in skillNodes)
            {
                node.isUnlocked = false;
                node.nodeButton.image.color = Color.gray;
            }
        }

        private string GetSkillDescription(int skillId)
        {
            return skillId switch
            {
                1 => "Fireball - Deals damage in area\n+50% fire damage",
                2 => "Ice Storm - Freezes enemies\n+30% freeze chance",
                3 => "Lightning Strike - Single target nuke\n+40% crit chance",
                4 => "Whirlwind - AOE melee attack\n+25% all damage",
                5 => "Shield Wall - Block incoming damage\n+50% defense",
                _ => "Unknown skill"
            };
        }
    }

    /// <summary>
    /// Settings menu with volume, difficulty, accessibility controls.
    /// </summary>
    public class SettingsMenuUI : MonoBehaviour
    {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider ambienceVolumeSlider;

        [SerializeField] private Dropdown difficultyDropdown;
        [SerializeField] private Toggle screenShakeToggle;
        [SerializeField] private Toggle colorblindModeToggle;
        [SerializeField] private Toggle subtitlesToggle;

        [SerializeField] private Button resetToDefaultsButton;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;

        private GameSettings currentSettings;

        public event System.Action<GameSettings> OnSettingsApplied;
        public event System.Action OnSettingsReset;

        private void Start()
        {
            // Load current settings
            currentSettings = LoadSettings();
            RefreshUI();

            // Wire buttons
            if (resetToDefaultsButton) resetToDefaultsButton.onClick.AddListener(ResetToDefaults);
            if (applyButton) applyButton.onClick.AddListener(ApplySettings);
            if (closeButton) closeButton.onClick.AddListener(() => gameObject.SetActive(false));

            // Wire sliders
            if (masterVolumeSlider) masterVolumeSlider.onValueChanged.AddListener(_ => UpdatePreview());
            if (sfxVolumeSlider) sfxVolumeSlider.onValueChanged.AddListener(_ => UpdatePreview());
        }

        /// <summary>Refresh UI to show current settings.</summary>
        public void RefreshUI()
        {
            if (masterVolumeSlider) masterVolumeSlider.value = currentSettings.MasterVolume;
            if (sfxVolumeSlider) sfxVolumeSlider.value = currentSettings.SFXVolume;
            if (musicVolumeSlider) musicVolumeSlider.value = currentSettings.MusicVolume;
            if (ambienceVolumeSlider) ambienceVolumeSlider.value = currentSettings.AmbienceVolume;

            if (difficultyDropdown)
            {
                difficultyDropdown.value = System.Array.IndexOf(
                    new[] { "Easy", "Normal", "Hard", "Nightmare" },
                    currentSettings.Difficulty
                );
            }

            if (screenShakeToggle) screenShakeToggle.isOn = currentSettings.ScreenShakeEnabled;
            if (colorblindModeToggle) colorblindModeToggle.isOn = currentSettings.ColorblindMode;
            if (subtitlesToggle) subtitlesToggle.isOn = currentSettings.SubtitlesEnabled;
        }

        /// <summary>Apply settings and save.</summary>
        public void ApplySettings()
        {
            // Gather settings from UI
            currentSettings.MasterVolume = masterVolumeSlider?.value ?? 1f;
            currentSettings.SFXVolume = sfxVolumeSlider?.value ?? 1f;
            currentSettings.MusicVolume = musicVolumeSlider?.value ?? 1f;
            currentSettings.AmbienceVolume = ambienceVolumeSlider?.value ?? 1f;

            int diffIndex = difficultyDropdown?.value ?? 1;
            currentSettings.Difficulty = new[] { "Easy", "Normal", "Hard", "Nightmare" }[diffIndex];

            currentSettings.ScreenShakeEnabled = screenShakeToggle?.isOn ?? true;
            currentSettings.ColorblindMode = colorblindModeToggle?.isOn ?? false;
            currentSettings.SubtitlesEnabled = subtitlesToggle?.isOn ?? true;

            SaveSettings(currentSettings);
            OnSettingsApplied?.Invoke(currentSettings);

            Debug.Log("Settings applied and saved");
        }

        /// <summary>Reset all settings to defaults.</summary>
        public void ResetToDefaults()
        {
            currentSettings = new GameSettings();
            RefreshUI();
            OnSettingsReset?.Invoke();
        }

        /// <summary>Preview settings (live update).</summary>
        private void UpdatePreview()
        {
            // Can update audio volumes in real-time here
        }

        private GameSettings LoadSettings()
        {
            // Load from PlayerPrefs or file
            return new GameSettings();
        }

        private void SaveSettings(GameSettings settings)
        {
            // Save to PlayerPrefs or file
        }
    }

    /// <summary>
    /// Game settings data structure.
    /// </summary>
    public class GameSettings
    {
        public float MasterVolume { get; set; } = 1f;
        public float SFXVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 0.7f;
        public float AmbienceVolume { get; set; } = 0.5f;
        public string Difficulty { get; set; } = "Normal";
        public bool ScreenShakeEnabled { get; set; } = true;
        public bool ColorblindMode { get; set; } = false;
        public bool SubtitlesEnabled { get; set; } = true;
    }
}
