# Difficulty Presets Guide

## Overview

Difficulty Presets bundle all four balance configuration systems (Balance, Progression, Loot, Ability) into preset packages that designers can select from to establish consistent game difficulty across all systems.

**Presets included:**
- **Easy** (Rating 1.0) — Forgiving experience, high progression speed, generous loot
- **Normal** (Rating 2.5) — Balanced baseline, standard progression, moderate loot
- **Hard** (Rating 3.5) — Challenging combat, slower progression, limited loot
- **Nightmare** (Rating 5.0) — Extreme difficulty, minimal progression, rare loot drops

## Architecture

### DifficultyPreset (ScriptableObject)

A bundled configuration asset that holds references to all four balance systems:

```csharp
public class DifficultyPreset : ScriptableObject
{
    // Holds references to all 4 configs
    public BalanceConfig BalanceConfig { get; }
    public ProgressionConfig ProgressionConfig { get; }
    public LootConfig LootConfig { get; }
    public AbilityConfig AbilityConfig { get; }
    
    // Metadata
    public string PresetName { get; }           // "Easy", "Normal", etc.
    public string Description { get; }         // Designer-facing description
    public float DifficultyRating { get; }     // 0-5 scale
    public Color PresetColor { get; }          // UI display color
    
    // Validation
    public bool IsComplete()                   // True if all 4 configs assigned
    public string GetMissingConfigs()         // Lists unassigned configs
}
```

### DifficultyManager (Singleton)

Runtime manager that loads and applies the current difficulty preset:

```csharp
public class DifficultyManager
{
    public static DifficultyManager Instance { get; }
    public DifficultyPreset CurrentPreset { get; }
    public event Action<DifficultyPreset> OnPresetChanged;
    
    public bool SetPreset(DifficultyPreset preset)           // Load preset
    public BalanceConfig GetBalanceConfig()                 // Get active configs
    public ProgressionConfig GetProgressionConfig()
    public LootConfig GetLootConfig()
    public AbilityConfig GetAbilityConfig()
    public DifficultyMode GetDifficultyMode()              // Map rating→mode
}
```

**Key features:**
- Validates all 4 configs present before setting
- Fires `OnPresetChanged` event for UI/systems to react
- Provides direct access to each config
- Maps difficulty rating (0-5) to DifficultyMode enum

## Usage

### For Game Systems

Request configs from DifficultyManager:

```csharp
// In a combat system
var balanceConfig = DifficultyManager.Instance.GetBalanceConfig();
var abilityConfig = DifficultyManager.Instance.GetAbilityConfig();

if (balanceConfig != null)
{
    // Use preset values instead of hardcoded ones
    enemyHealth = balanceConfig.CalculateEnemyHealth(enemyLevel, region);
}
```

### For Designer Workflow

1. **Create a new preset asset:**
   - Right-click in Project → Create → BingoQuest → Difficulty Preset
   - Name it (e.g., `NormalPreset`)

2. **Assign configs to the preset:**
   - Drag BalanceConfig into "Balance Config" field
   - Drag ProgressionConfig into "Progression Config" field
   - Drag LootConfig into "Loot Config" field
   - Drag AbilityConfig into "Ability Config" field

3. **Fill in metadata:**
   - Preset Name: "Normal"
   - Description: "Balanced experience, standard progression"
   - Difficulty Rating: 2.5
   - Preset Color: (Yellow or custom)

4. **Validate:**
   - Select preset and check Inspector — should show all 4 ✓ checkmarks

5. **Use in game:**
   - In DemoBootstrap or main menu: `DifficultyManager.Instance.SetPreset(normalPreset)`
   - Listen to `OnPresetChanged` to update UI

## Pre-Tuned Presets

### Easy Preset (Rating 1.0)

**Goal:** Forgiving learning experience for new players.

**Balance Config tuning:**
- Damage scaling: 0.7x (enemies deal less damage)
- Defense mitigation: 0.5x (player takes reduced damage)
- Enemy HP scaling: 0.8x (enemies die faster)
- Difficulty multiplier: 0.7x

**Progression Config tuning:**
- XP gain multiplier: 1.2x (level up faster)
- Skill points per level: +1 bonus
- Stat scaling: Standard (no change)
- Skill tree requirements: Reduced by 20%

**Loot Config tuning:**
- Rare drop rate: +15% boost
- Affix count range: +1 extra affix for rarity
- Boss loot multiplier: 1.5x

**Ability Config tuning:**
- Cooldown multiplier: 0.8x (abilities available more often)
- Damage per ability: 0.9x (slightly reduced)
- Resource costs: 0.8x (cheaper spells)

**Result:** Players progress quickly, take less damage, find better loot. Ideal for tutorials/story mode.

---

### Normal Preset (Rating 2.5)

**Goal:** Balanced baseline gameplay.

**All configs:** Use default values (1.0x multipliers, standard curves).

**Result:** Published game balance. Foundation for difficulty comparisons.

---

### Hard Preset (Rating 3.5)

**Goal:** Challenging but fair difficulty for experienced players.

**Balance Config tuning:**
- Damage scaling: 1.3x (enemies deal more damage)
- Defense mitigation: 0.7x (player receives more damage)
- Enemy HP scaling: 1.2x (enemies have more HP)
- Difficulty multiplier: 1.3x

**Progression Config tuning:**
- XP gain multiplier: 0.8x (level up slower)
- Skill points per level: -1 penalty
- Stat scaling: Standard
- Skill tree requirements: Increased by 30%

**Loot Config tuning:**
- Rare drop rate: -10% reduction
- Affix count range: -1 fewer affixes
- Boss loot multiplier: 0.7x

**Ability Config tuning:**
- Cooldown multiplier: 1.3x (longer waits between ability use)
- Damage per ability: 1.0x (no change)
- Resource costs: 1.2x (more expensive spells)

**Result:** Players progress slowly, combat requires skill, loot is scarce. For veterans.

---

### Nightmare Preset (Rating 5.0)

**Goal:** Extreme challenge for hardcore players.

**Balance Config tuning:**
- Damage scaling: 1.6x (severe damage output)
- Defense mitigation: 0.4x (player is fragile)
- Enemy HP scaling: 1.5x (boss-tier enemies)
- Difficulty multiplier: 1.6x

**Progression Config tuning:**
- XP gain multiplier: 0.6x (grind required)
- Skill points per level: -2 severe penalty
- Stat scaling: Standard
- Skill tree requirements: Increased by 50%

**Loot Config tuning:**
- Rare drop rate: -25% massive reduction
- Affix count range: -2 very limited affixes
- Boss loot multiplier: 0.5x (even bosses drop little)

**Ability Config tuning:**
- Cooldown multiplier: 1.6x (very long cooldowns)
- Damage per ability: 0.95x (slightly reduced)
- Resource costs: 1.5x (expensive, limited uptime)

**Result:** One-hit kills possible. Extreme resource scarcity. Permadeath-viable. Only for hardcore players.

---

## Creating Custom Presets

### Template for a new preset:

```
Preset Name:        "Custom"
Description:        "[Your description here]"
Difficulty Rating:  [0.0-5.0]
Preset Color:       [Custom color]

Balance Config multipliers:
  - Damage scaling:         [default: 1.0x]
  - Defense mitigation:     [default: 1.0x]
  - Enemy HP scaling:       [default: 1.0x]
  - Difficulty multiplier:  [default: 1.0x]

Progression Config multipliers:
  - XP gain:                [default: 1.0x]
  - Skill points bonus:     [default: 0]
  - Stat scaling:           [default: 1.0x]

Loot Config adjustments:
  - Rare drop boost:        [default: 0%]
  - Affix count delta:      [default: 0]
  - Boss loot multiplier:   [default: 1.0x]

Ability Config multipliers:
  - Cooldown multiplier:    [default: 1.0x]
  - Damage per ability:     [default: 1.0x]
  - Resource costs:         [default: 1.0x]
```

### Design philosophy:

1. **Coherence:** All 4 systems should trend the same direction (easier = lower damage + faster progression + better loot)
2. **Playstyle impact:** Adjust ability cooldowns to change combat feel (fast-paced vs. tactical)
3. **Progression pacing:** XP multiplier determines grind; lower = more playtime per level
4. **Loot feedback:** Better loot = more rewarding experience (but can trivialize combat on Easy)
5. **Test thoroughly:** Playtest each preset for 30+ minutes to catch balance issues

## Integration Checklist

- [ ] Create 4 difficulty preset assets (Easy, Normal, Hard, Nightmare)
- [ ] Assign all 4 config types to each preset
- [ ] Fill in metadata (names, descriptions, ratings, colors)
- [ ] Validate each preset is "complete" in Inspector
- [ ] Add preset selector to game menu/demo bootstrap
- [ ] Wire `OnPresetChanged` event to update UI
- [ ] Playtest each preset for difficulty curve
- [ ] Document any region-specific tuning (desert vs. forest)
- [ ] Create preset profiles as backups in version control

## Testing

Unit tests verify:
- Preset completeness validation
- Missing config detection
- DifficultyManager preset switching
- Config access after preset load
- Difficulty rating → mode mapping
- Event firing on preset change
- Singleton behavior

Run tests:
```
Unity → Window → General → Test Runner
  → BingoQuest.Tests.EditMode.DifficultyPresetTests
  → Run All
```

Expected: All 20+ tests pass with no warnings.

## Next Steps

1. Create 4 preset .asset files (Easy, Normal, Hard, Nightmare)
2. Assign configs to each preset and tune multipliers
3. Integrate preset selector into game UI/bootstrap
4. Playtest progression curve on each difficulty
5. Gather community feedback on balance
6. Iterate on presets based on telemetry (if available)
