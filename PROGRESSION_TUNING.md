# Progression Balance Tuning Guide

## Overview

Bingo Quest progression is driven by **ProgressionConfig**, a ScriptableObject that controls:
- **XP curves** — How much XP each level requires
- **Skill points** — When and how many per level
- **Stat growth** — Base stat increases per level
- **Difficulty modifiers** — How fast players level on each difficulty

All parameters are designer-tunable via Unity Inspector.

---

## ProgressionConfig Parameters

### Experience Curve

**Base XP Threshold** (integer): XP required to reach level 2. Default: 100
- Higher values slow early progression
- Lower values speed it up

**XP Scaling Per Level** (1.05–1.25): Multiplier per level. Default: 1.1
- 1.05 = gentle curve (level 50 needs ~4x more XP than level 2)
- 1.15 = steep curve (exponential wall late-game)
- 1.1 = moderate (recommended baseline)

**Max Level** (integer): Hard cap on progression. Default: 50
- Prevents unlimited scaling
- Provides target for end-game design

**Bonus XP Multiplier** (0.5–2.0): Global XP multiplier. Default: 1.0
- 1.2 = 20% XP bonus everywhere
- Use for events or seasonal boosts

**Formula:** `xp_for_level(n) = baseXPThreshold * (xpScalingPerLevel ^ (n - 1))`

**Example Progression (Default):**
| Level | XP Required | Total XP | Time to Level* |
|-------|-------------|----------|----------------|
| 1→2   | 100         | 100      | 10 kills       |
| 5→6   | 161         | 691      | 16 kills       |
| 10→11 | 259         | 2594     | 26 kills       |
| 20→21 | 673         | 12604    | 67 kills       |
| 30→31 | 1745        | 43880    | 175 kills      |
| 50→51 | 11740       | 307101   | 1174 kills     |

*Assuming 10 XP per enemy kill

---

### Skill Points

**Starting Skill Points** (integer): Points at level 1. Default: 2
- Allows early class customization
- Keep ≥1 to avoid first level being wasted

**Skill Points Per Level** (integer): Points gained per level. Default: 1
- 1 = slow tree progression (need ~20 levels to unlock endgame)
- 2 = fast tree progression (all skills available by level 10)

**Bonus Skill Point Interval** (integer): Every N levels, grant bonus. Default: 10
- At level 10, 20, 30, etc., gain extra skill points
- Creates milestone rewards for leveling streaks

**Bonus Skill Points Amount** (integer): Points granted at milestone. Default: 1

**Example (Default):**
- Level 1: 2 skill points
- Level 1–10: 1 point per level = 10 points → 12 total at level 10
- Level 10: +1 bonus = 13 total
- Level 11–20: 1 point per level = 10 points → 23 total at level 20
- Level 20: +1 bonus = 24 total
- Total available by level 50: ~50 skill points (enough for most tree builds)

---

### Stat Progression

**Health Per Level** (integer): HP bonus per level. Default: 5
- 5 = player gains 5 HP per level
- At level 50: +250 HP from leveling alone
- Low values = glass cannon feel; high values = tank scaling

**Attack Per Level** (integer): ATK bonus per level. Default: 1
- Directly increases damage output
- At level 50: +50 ATK

**Defense Per Level** (integer): DEF bonus per level. Default: 1
- Reduces incoming damage (via BalanceConfig mitigation factor)
- At level 50: +50 DEF

**Crit Chance Per Level** (0–0.1): Crit% increase per level. Default: 0.005 (0.5%)
- At level 50: +2.5% crit chance
- Combined with skill tree bonuses, can reach high crit builds

**Dodge Chance Per Level** (0–0.1): Dodge% increase per level. Default: 0.003 (0.3%)
- At level 50: +1.5% dodge chance
- Asymptotic (clamped to 0–1)

---

### Leveling Speed (Difficulty Modifiers)

XP multipliers per difficulty mode:

- **Easy**: 1.2x XP (20% bonus) → level faster, accessible
- **Normal**: 1.0x XP (baseline)
- **Hard**: 0.8x XP (20% penalty) → level slower, grind-heavy
- **Nightmare**: 0.6x XP (40% penalty) → severe grind

Affects `GainExperience(amount, difficulty)` in CharacterProgression.

**Tuning Tips:**
- If Hard feels too punishing, bump to 0.9x
- If Easy is still too slow, increase to 1.3–1.5x
- Nightmare should reward players willing to grind; 0.6x is steep

---

### Milestone Rewards

**Level Up Health Heal** (integer): HP restored on level up. Default: 0
- 0 = no heal (players must rest)
- MaxHealth = full heal (feels generous)
- Useful for: single-player campaigns where healing is rare

**Reset Ability Cooldowns On Level Up** (bool): Clear all ability cooldowns. Default: false
- Useful for dungeon-crawlers (reward progression)
- Leave false for sustained combat feel

---

## Integration Points

### CharacterProgression.GainExperience()
```csharp
progression.GainExperience(10, Progression.DifficultyMode.Normal);
// Applies difficulty multiplier, checks level-up thresholds
// Fires OnLevelUp event
```

### Stat Calculation
```csharp
var (health, atk, def, crit, dodge) = config.CalculateLevelBonuses(level);
// Get total stat bonuses from leveling alone
```

### Skill Point Budget
```csharp
int totalSkills = config.CalculateSkillPointsForLevel(50);
// Total skill points available by level 50
```

---

## Common Tuning Scenarios

### "Players level too fast"
**Problem:** Game feels grindy; rewards come too quickly

**Solution:**
- Increase `xpScalingPerLevel` from 1.1 to 1.15
- Reduce `skillPointsPerLevel` from 1 to 0.5
- OR increase `baseXPThreshold` from 100 to 150

**Effect:** Each level takes 15% longer. By level 30, doubled progression time.

---

### "Players level too slow"
**Problem:** Long stretches without rewards; feels tedious

**Solution:**
- Reduce `xpScalingPerLevel` from 1.1 to 1.05
- Increase `skillPointsPerLevel` from 1 to 1.5
- Add `bonusSkillPointInterval = 5` for frequent milestones

**Effect:** Each level takes 20% less XP. Skill tree feels more rewarding.

---

### "Early game is too easy"
**Problem:** First 5 levels are trivial; no challenge

**Solution:**
- Lower `startingSkillPoints` from 2 to 1 (less customization)
- Reduce health/attack bonuses per level
- Rely on BalanceConfig combat difficulty instead

---

### "Late game (levels 40+) is grindy"
**Problem:** Leveling to 50 takes forever; players quit at 30

**Solution:**
- Add `bonusSkillPointInterval = 5` for milestone dopamine
- Reduce `xpScalingPerLevel` from 1.1 to 1.08 (gentler curve)
- Increase `baseXPThreshold` and reduce `maxLevel` (cap at 30 instead of 50)

---

### "Stat scaling feels weak"
**Problem:** Level 50 player barely feels stronger than level 20

**Solution:**
- Double `healthPerLevel` from 5 to 10
- Double `attackPerLevel` from 1 to 2
- Increase `critChancePerLevel` from 0.005 to 0.01

**Effect:** Level 50 player has ~250 extra health, +100 ATK, +5% crit.

---

### "Stat scaling feels overpowered"
**Problem:** Level 30 players one-shot everything

**Solution:**
- Reduce `healthPerLevel` to 3
- Reduce `attackPerLevel` to 0.5
- Keep skill tree as primary power source (don't over-rely on level stats)

---

## Balance Testing Workflow

1. **Baseline Pass:**
   - Record kill times per level in each region
   - Record XP per kill vs. XP required for next level
   - Note player feedback on progression speed

2. **Measure Progression Rate:**
   - How many fights to level up at level 10? Level 30? Level 50?
   - Is curve smooth, or does it spike?

3. **Adjust One Parameter:**
   - Change `xpScalingPerLevel` by ±0.05
   - Play through and measure new progression rate
   - If better, keep; if worse, revert

4. **Cross-Check All Modes:**
   - Progression should feel different on Easy vs. Nightmare
   - Ensure skill trees are unlocking at reasonable pace

5. **Validate Stat Scaling:**
   - Record damage-to-health ratio at levels 1, 10, 30, 50
   - Should have consistent kill times (adjusted for region difficulty)

---

## Creating a ProgressionConfig Asset

1. Right-click in Project → **Create → BingoQuest → Progression Config**
2. Name: `ProgressionConfig_[DifficultyMode]_[Version]`
   - `ProgressionConfig_Normal_v1.asset` (baseline)
   - `ProgressionConfig_Casual_v1.asset` (faster leveling, easier)
3. Adjust sliders in Inspector
4. Test with DemoBootstrap → verify level-up speed feels good

---

## Pre-Tuned Profiles

**Casual (Fast Leveling):**
- baseXPThreshold: 75
- xpScalingPerLevel: 1.05
- skillPointsPerLevel: 1.5
- healthPerLevel: 8

**Standard (Balanced):**
- baseXPThreshold: 100
- xpScalingPerLevel: 1.10
- skillPointsPerLevel: 1
- healthPerLevel: 5

**Hardcore (Slow Leveling):**
- baseXPThreshold: 150
- xpScalingPerLevel: 1.15
- skillPointsPerLevel: 0.5
- healthPerLevel: 3

---

## Next Steps

1. Create 3 ProgressionConfig variants (Casual, Standard, Hardcore)
2. Tie difficulty selector to config selection
3. Measure average session length per difficulty
4. Adjust based on player retention
5. Integrate with telemetry system for long-term tuning
