# Complete UI & UX Implementation Guide

## Overview

Production-ready UI system for Bingo Quest with inventory management, character sheet, skill tree, and settings menu. All systems support filtering, sorting, stat comparison, and event-driven architecture.

## Architecture

### 1. **InventoryUI** — Item Management & Display

**Core Features:**
- Grid-based inventory layout with item slots
- Rarity-based color coding (Common, Uncommon, Rare, Epic, Legendary)
- Item inspection panel with stats display
- Sorting options (by Name, by Rarity)
- Filtering options (All, Weapons, Armor, Accessories)
- Item comparison (selected vs equipped)
- Consumable item support (potions, scrolls, etc.)

**Data Structure (InventoryItem):**
```csharp
public class InventoryItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Sprite Icon { get; set; }
    public string Rarity { get; set; }              // Common, Uncommon, Rare, Epic, Legendary
    public string Type { get; set; }                // Weapon, Armor, Accessory, Consumable
    public int Damage { get; set; }
    public int Defense { get; set; }
    public int LevelRequired { get; set; }
    public int GoldValue { get; set; }
    public bool IsConsumable { get; set; }
}
```

**Events:**
- `OnItemSelected` — Item clicked for inspection
- `OnItemEquipped` — Item equipped from inventory
- `OnItemConsumed` — Consumable item used

**Setup in Unity:**
1. Create Canvas with GridLayoutGroup for inventory grid
2. Create ItemSlot prefab (Image + Button)
3. Add InventoryUI component to Canvas
4. Assign required UI elements in Inspector:
   - Inventory Grid (GridLayoutGroup)
   - Item Slot Prefab
   - Detail Panel (name, description, stats, rarity indicator)
   - Sort & Filter buttons
5. Configure rarity colors (Common=white, Uncommon=green, Rare=blue, Epic=purple, Legendary=gold)

**Usage Example:**
```csharp
List<InventoryItem> items = GetPlayerInventory();
inventoryUI.RefreshInventory(items);

inventoryUI.OnItemEquipped += (item) =>
{
    playerCharacter.EquipItem(item);
    audioManager.PlaySFX("Item_Equip");
};

inventoryUI.OnItemConsumed += (item) =>
{
    playerCharacter.UseConsumable(item);
    inventoryUI.RefreshInventory(GetPlayerInventory());
};
```

**Rarity Color Scheme:**
| Rarity | Color | RGB |
|--------|-------|-----|
| Common | White | 1.0, 1.0, 1.0 |
| Uncommon | Green | 0.2, 1.0, 0.2 |
| Rare | Blue | 0.2, 0.8, 1.0 |
| Epic | Purple | 1.0, 0.2, 1.0 |
| Legendary | Gold | 1.0, 0.84, 0.0 |

---

### 2. **CharacterSheetUI** — Character Stats & Progression

**Core Features:**
- Character portrait and name display
- Level and experience bar
- Core stats (HP, Mana, Damage, Defense, Crit, Attack Speed)
- Attributes display (Strength, Dexterity, Intelligence, Vitality)
- Equipped items showcase (weapon, armor, accessory slots)
- Available attribute points tracker
- Character comparison mode

**Data Structure (CharacterData):**
```csharp
public class CharacterData
{
    public string Name { get; set; }
    public int Level { get; set; }
    public Sprite PortraitIcon { get; set; }
    public int CurrentXP { get; set; }
    public int XPForNextLevel { get; set; }
    
    // Core Stats
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int CurrentMana { get; set; }
    public int MaxMana { get; set; }
    public int Damage { get; set; }
    public int Defense { get; set; }
    public float CritChance { get; set; }
    public float AttackSpeed { get; set; }
    
    // Attributes
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Vitality { get; set; }
    public int AttributePoints { get; set; }
    
    // Equipment
    public InventoryItem EquippedWeapon { get; set; }
    public InventoryItem EquippedArmor { get; set; }
    public InventoryItem EquippedAccessory { get; set; }
}
```

**UI Layout:**
- Left column: Character portrait, name, level
- Center: Experience bar, core stats grid
- Right column: Equipped items slots, attributes list
- Bottom: Available attribute points, attribute point spend buttons

**Setup in Unity:**
1. Create panel with character portrait
2. Add TextMeshPro elements for name, level, stats
3. Add Slider for experience bar
4. Create equipped item display zones (3 image slots)
5. Add attributes display section
6. Wire attribute increase buttons to OnAttributePointSpent event

**Usage Example:**
```csharp
CharacterData character = playerCharacter.GetCharacterData();
characterSheetUI.RefreshCharacterSheet(character);

characterSheetUI.OnAttributePointSpent += (attributeType) =>
{
    playerCharacter.IncreaseAttribute(attributeType);
    characterSheetUI.RefreshCharacterSheet(playerCharacter.GetCharacterData());
};
```

---

### 3. **SkillTreeUI** — Ability Progression & Respec

**Core Features:**
- Node-based skill tree layout
- Skill unlock with skill point spending
- Skill node highlighting (unlocked/locked states)
- Skill description and requirements display
- Full respec system (costly but available)
- Skill point counter
- Visual feedback for available actions

**Node Tracking:**
- Each skill has unique ID (1-N)
- Skill name, description, icon per node
- Prerequisites (optional, for branching trees)
- Unlocked state tracking

**Setup in Unity:**
1. Create Canvas with node buttons arranged in skill tree pattern
2. Each node button has:
   - Button component
   - Image for icon (gray when locked, colored when unlocked)
   - Text for skill name
3. Add detail panel below tree showing:
   - Selected skill name and description
   - Unlock button (enabled only if unlocked and points available)
   - Respec button with cost display
4. Add skill points counter at top

**Skill Tree Example:**
```
                [Fireball]
               /          \
          [Fire Mastery]  [Flame Burst]
               \          /
              [Inferno] (requires both)
```

**Node Properties:**
```csharp
public class SkillNodeUI
{
    public Button nodeButton;
    public Image nodeIcon;
    public TextMeshProUGUI nodeNameText;
    public TextMeshProUGUI nodeDescriptionText;
    public bool isUnlocked;
    public int skillId;
    // Optional: List<int> prerequisites
}
```

**Events:**
- `OnSkillUnlocked(skillId)` — Skill purchased with skill point
- `OnRespecRequested()` — Player requested full respec

**Respec System:**
- Gold cost per respec (e.g., 1000 gold)
- Refunds all spent skill points
- Reset all unlocked skills to locked state
- Enables player to completely reorganize skill allocation

**Usage Example:**
```csharp
List<int> unlockedSkills = playerCharacter.GetUnlockedSkills();
skillTreeUI.RefreshSkillTree(10, unlockedSkills);

skillTreeUI.OnSkillUnlocked += (skillId) =>
{
    playerCharacter.UnlockSkill(skillId);
    audioManager.PlaySFX("SkillUnlock");
};

skillTreeUI.OnRespecRequested += () =>
{
    if (playerCharacter.HasGold(1000))
    {
        playerCharacter.RemoveGold(1000);
        playerCharacter.RespecSkills();
        skillTreeUI.RefreshSkillTree(playerCharacter.GetSkillPoints(), new List<int>());
    }
};
```

---

### 4. **SettingsMenuUI** — Game Configuration

**Core Features:**
- Volume control (Master, SFX, Music, Ambience)
- Difficulty selector (Easy, Normal, Hard, Nightmare)
- Accessibility options:
  - Screen shake toggle
  - Colorblind mode (adjusts UI colors)
  - Subtitles toggle
- Reset to defaults button
- Settings persistence (PlayerPrefs or save file)

**Data Structure (GameSettings):**
```csharp
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
```

**UI Layout:**
- Top section: Volume sliders (Master, SFX, Music, Ambience)
- Middle section: Difficulty dropdown
- Bottom section: Checkboxes for screen shake, colorblind mode, subtitles
- Buttons: Reset to Defaults, Apply, Close

**Setup in Unity:**
1. Create settings panel with sliders and toggles
2. Sliders for volume (0-1 range)
3. Dropdown for difficulty selection
4. Toggles for accessibility options
5. Buttons at bottom (Reset, Apply, Close)

**Integration Points:**
```csharp
settingsUI.OnSettingsApplied += (settings) =>
{
    audioManager.SetCategoryVolume("SFX", settings.SFXVolume);
    audioManager.SetCategoryVolume("Music", settings.MusicVolume);
    audioManager.SetCategoryVolume("Ambience", settings.AmbienceVolume);
    audioManager.MasterVolume = settings.MasterVolume;
    
    difficultyManager.SetDifficulty(settings.Difficulty);
    
    cameraController.EnableScreenShake = settings.ScreenShakeEnabled;
    uiColorScheme.SetColorblindMode(settings.ColorblindMode);
    subtitleSystem.EnableSubtitles(settings.SubtitlesEnabled);
    
    SaveSettings(settings);
};

settingsUI.OnSettingsReset += () =>
{
    settingsUI.RefreshUI(); // Reload from saved defaults
};
```

---

## UI Layout Templates

### Inventory Screen Layout
```
┌─────────────────────────────────────┐
│ Inventory    [Sort ▼] [Filter ▼]   │
├─────────────────────────────────────┤
│ [Item Slots Grid 5x4]   │ Item Details
│                          │ ──────────
│                          │ Sword
│                          │ Rarity: Rare
│                          │ Damage: +50
│                          │ Lvl Req: 10
│                          │ Value: 500g
│                          │
│                          │ [Equip] [Compare]
└─────────────────────────────────────┘
```

### Character Sheet Layout
```
┌─────────────────────────────────────┐
│ [Portrait] Hero              Level 10│
│            Progress: ████░░░░ 500/1K │
├─────────────────────────────────────┤
│ ┌─────────────┐  ┌──────────┐        │
│ │ HP:    100  │  │[Weapon] ●│        │
│ │ Mana:   50  │  │[Armor]  ●│        │
│ │ DMG:    25  │  │[Acc]    ●│        │
│ │ DEF:    15  │  └──────────┘        │
│ │ Crit: 15.5% │  Str:10 Dex:8        │
│ │ AtkSpd:1.5x │  Int:12 Vit:9        │
│ └─────────────┘  Pts: 3              │
└─────────────────────────────────────┘
```

### Skill Tree Layout
```
┌─────────────────────────────────────┐
│ Skill Points: 5                      │
├─────────────────────────────────────┤
│           [Fireball]                 │
│          /          \                │
│    [Fire Mastery] [Flame Burst]      │
│          \          /                │
│       [Inferno] ●                    │
├─────────────────────────────────────┤
│ Inferno: Massive AOE damage         │
│ Level Req: 25  |  [Unlock] [Respec] │
└─────────────────────────────────────┘
```

### Settings Menu Layout
```
┌─────────────────────────────────────┐
│ SETTINGS                             │
├─────────────────────────────────────┤
│ Master Volume: ███░░░░░░ 0.7        │
│ SFX Volume:    ██████░░░ 0.8        │
│ Music Volume:  █████░░░░ 0.7        │
│ Ambience:      ███░░░░░░ 0.5        │
│                                      │
│ Difficulty: [Normal ▼]              │
│                                      │
│ ☑ Screen Shake                       │
│ ☐ Colorblind Mode                   │
│ ☑ Subtitles                         │
│                                      │
│  [Reset] [Apply] [Close]            │
└─────────────────────────────────────┘
```

---

## Setup Checklist

### Inventory UI
- [ ] Create GridLayoutGroup for item slots
- [ ] Create ItemSlot prefab (Image + Button)
- [ ] Assign all UI elements to InventoryUI script
- [ ] Configure rarity colors for each tier
- [ ] Create item icons (256x256 sprites)
- [ ] Wire sort/filter buttons
- [ ] Test with sample inventory data

### Character Sheet
- [ ] Create character portrait display
- [ ] Add stat text elements (HP, Mana, Damage, Defense, Crit, AttackSpeed)
- [ ] Create experience progress bar with slider
- [ ] Add equipped item display slots (weapon, armor, accessory)
- [ ] Add attributes display section
- [ ] Create attribute increase buttons
- [ ] Test with sample character data

### Skill Tree
- [ ] Design skill tree layout (nodes in hierarchy)
- [ ] Create button prefabs for skill nodes
- [ ] Arrange nodes in grid or custom layout
- [ ] Create skill icon sprites (64x64)
- [ ] Add detail panel for selected skill
- [ ] Add unlock button and respec button
- [ ] Configure skill descriptions and requirements
- [ ] Test unlock/respec flow

### Settings Menu
- [ ] Create panel with sliders for volume
- [ ] Add dropdown for difficulty selection
- [ ] Add toggles for accessibility options
- [ ] Create buttons (Reset, Apply, Close)
- [ ] Wire up audio manager integration
- [ ] Implement settings persistence (PlayerPrefs)
- [ ] Test all volume adjustments in real-time

---

## Event Integration Examples

### Item Equipped Event
```csharp
inventoryUI.OnItemEquipped += (item) =>
{
    // Update character equipment
    playerStats.EquipItem(item);
    
    // Update character sheet display
    characterSheet.RefreshCharacterSheet(playerStats.GetCharacterData());
    
    // Play equip sound
    audioManager.PlaySFX("Item_Equip");
    
    // Show notification
    uiNotification.Show($"Equipped {item.Name}");
};
```

### Skill Unlocked Event
```csharp
skillTreeUI.OnSkillUnlocked += (skillId) =>
{
    // Add skill to character
    playerCharacter.UnlockSkill(skillId);
    
    // Update available actions
    actionBar.AddSkillToActionBar(skillId);
    
    // Play unlock sound
    audioManager.PlaySFX("SkillUnlock");
    
    // Show screen effect
    vfxSpawner.SpawnEffect("SkillUnlock", screenCenter);
};
```

### Settings Applied Event
```csharp
settingsUI.OnSettingsApplied += (settings) =>
{
    // Update audio
    audioManager.SetCategoryVolume("SFX", settings.SFXVolume);
    audioManager.SetCategoryVolume("Music", settings.MusicVolume);
    audioManager.MasterVolume = settings.MasterVolume;
    
    // Update difficulty
    difficultyManager.SetPresetByName(settings.Difficulty);
    
    // Update visual settings
    if (settings.ScreenShakeEnabled)
        cameraController.EnableScreenShake();
    else
        cameraController.DisableScreenShake();
    
    // Persist settings
    PlayerPrefs.SetFloat("MasterVolume", settings.MasterVolume);
    PlayerPrefs.SetString("Difficulty", settings.Difficulty);
    PlayerPrefs.Save();
};
```

---

## Performance Tips

- **Item Grids:** Use object pooling for inventory slots (create ~20 at startup, reuse)
- **Character Sheet:** Only refresh on stat change, not every frame
- **Skill Tree:** Cache node button references, don't search by name
- **Settings:** Load from PlayerPrefs once at startup, not every frame
- **Text Updates:** Use TextMeshPro (not legacy UI Text) for better performance

---

## Testing Workflow

Run tests:
```
Unity → Window → General → Test Runner
  → BingoQuest.Tests.EditMode.UISystemTests
  → Run All
```

Expected: 40+ tests pass with no warnings.

---

## Next Steps

1. Create item icons for all equipment types
2. Design character portrait system with customization
3. Create skill tree node layout in design tool
4. Create sound effects for UI interactions
5. Implement settings persistence system
6. Add notification/toaster system
7. Add inventory search functionality
8. Create item tooltip hover system
9. Add drag-and-drop support for equipment
10. Implement crafting UI (optional)

## Files Delivered

- `MenuUI.cs` (824 lines) — InventoryUI, CharacterSheetUI, SkillTreeUI, SettingsMenuUI
- `UISystemTests.cs` (470 lines) — 40+ comprehensive tests
