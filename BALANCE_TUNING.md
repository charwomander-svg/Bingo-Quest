# Combat Balance Tuning Guide

## Overview

Bingo Quest uses a centralized **BalanceConfig** ScriptableObject to manage all combat difficulty parameters. This enables rapid iteration on game feel and progression curves without touching code.

**Key Principle:** Balance config changes are immediately visible in play tests—no recompilation required.

---

## BalanceConfig Structure

### Damage Scaling
- **Base Damage Scale** (0.5–2.0): Global multiplier on all incoming damage. Adjust to make combat faster or slower.
- **Critical Damage Multiplier** (1.0–3.0): Damage increase on critical hits. Default 1.5x = 50% bonus.
- **Elemental Power Scale** (0.01–1.0): How much Elemental Power stat contributes to ability damage.

**Tuning Tips:**
- Start with `baseDamageScale = 1.0` (neutral), then adjust ±0.1 based on feel.
- Increase `critDamageMultiplier` to reward skill-based builds (crit-focused characters).
- High `elementalPowerScale` makes elemental builds viable vs. physical.

---

### Defense & Mitigation
- **Defense Mitigation Factor** (0.1–1.0): What percent of DEF stat reduces damage.
  - 0.5 = enemies with 10 DEF reduce 5 damage
  - 1.0 = 1:1 damage reduction (defense-heavy builds viable)
  - 0.1 = defense barely matters
- **Min Damage Percent** (0–1.0): Minimum damage threshold as % of base damage.
  - 0.05 = 5% of base damage always hits, even if DEF is high
  - Prevents "defense stacking" triviializing damage

**Tuning Tips:**
- If players feel defense is useless, increase `defenseMitigationFactor` to 0.6+.
- If bosses are unkillable due to high defense, increase `minDamagePercent` to 0.1+.

---

### Difficulty Multipliers
- **Easy** (0.5–2.0): Recommended 0.75
- **Normal** (0.5–2.0): Recommended 1.0 (baseline)
- **Hard** (0.5–2.0): Recommended 1.35
- **Nightmare** (0.5–2.0): Recommended 1.75

Applied as multipliers to all enemy damage output.

**Tuning Tips:**
- Keep Normal at 1.0 as your reference point.
- If Easy is too hard, drop to 0.6. If too easy, bump to 0.85.
- Nightmare should feel punishing; consider 2.0+ if you want extreme challenge.

---

### Enemy HP Scaling
- **Level Health Multiplier** (0.8–1.2): Exponential health growth per level.
  - 1.05 = 5% health increase per level
  - 1.01 = very slow scaling (flat game)
  - 1.15 = steep scaling (levels matter a lot)
- **Base Enemy Health** (integer): Default health for level 1 enemies. Default 50 HP.
- **Boss Health Multiplier** (1.0–3.0): Multiplier on top of level scaling. Default 3.0x.

**Formula:** `health = baseHealth * (levelMultiplier ^ (level - 1)) * (1 if not boss else bossMultiplier)`

**Tuning Tips:**
- If level 1 enemies die in 1-2 hits, increase baseEnemyHealth to 70-100.
- If progression feels too fast, increase levelHealthMultiplier from 1.05 to 1.10 (steeper curve).
- Bosses should be mini-dungeons, not trivial. Keep bossHealthMultiplier ≥ 2.5.

**Example Progression:**
- Level 1 Enemy: 50 HP
- Level 5 Enemy: ~62 HP (50 * 1.05^4)
- Level 10 Enemy: ~82 HP
- Level 1 Boss: 150 HP (50 * 3.0)
- Level 10 Boss: 246 HP (82 * 3.0)

---

### Region Scaling
- **Starting Region Health Mult** (float): Base multiplier for Tier 0 region enemies.
- **Starting Region Damage Mult** (float): Base multiplier for Tier 0 region enemies.
- **Region Health Mult Step** (float): How much health increases per region tier.
- **Region Damage Mult Step** (float): How much damage increases per region tier.

**Example (Default Values):**
- Tier 0 (Town Hub): 1.0x health, 1.0x damage
- Tier 1 (Forest): 1.35x health, 1.25x damage
- Tier 2 (Mines): 1.70x health, 1.50x damage
- Tier 3 (Peaks): 2.05x health, 1.75x damage

**Tuning Tips:**
- If region progression feels too hard too fast, reduce step multipliers (0.2 instead of 0.35).
- If endgame regions feel trivial at high player level, increase step values.

---

### Progression
- **Base XP Per Enemy** (integer): Default 10 XP. Adjust global progression speed.
- **XP Scaling Per Level** (1.0–1.5): How fast XP thresholds grow.
  - 1.08 = 8% XP increase per level (slow burn to max)
  - 1.15 = 15% increase (fast early, much longer late game)
- **Boss Damage Multiplier** (float): How much harder bosses hit vs. regular enemies.

**Tuning Tips:**
- If players level too fast, reduce baseXPPerEnemy or increase xpScalingPerLevel.
- If boss encounters feel unfair, reduce bossDamageMultiplier to 1.2.

---

### Loot Multipliers
- **Common Drop Rate** (0.5–2.0): Multiplier on common item spawn chance.
- **Rare Drop Rate** (0.5–2.0): Multiplier on rare item spawn chance.
- **Epic Drop Rate** (0.5–2.0): Multiplier on epic item spawn chance.

Affects `LootGeneration.cs` rarity rolls.

**Tuning Tips:**
- If loot feels too sparse, increase all multipliers to 1.2+.
- If loot feels devalued (too common), reduce epic/rare rates to 0.7.

---

## Integration Points

### DamageCalculator
```csharp
var calculator = new DamageCalculator(balanceConfig);
var damage = calculator.CalculateDamage(attacker, defender, ability, difficultyModifier);
```

### Enemy Spawner
```csharp
int enemyHealth = balanceConfig.CalculateEnemyHealth(playerLevel, isBoss: true);
var (healthMult, damageMult) = balanceConfig.GetRegionMultipliers(regionTier);
```

### Region System
```csharp
float difficulty = balanceConfig.GetDifficultyMultiplier(DifficultyMode.Hard);
```

---

## Balance Testing Workflow

1. **Establish Baseline:** Record kill times, health% at end, resource usage in each region at each level.
2. **Identify Pain Points:** Which regions feel too hard? Where do players get stuck?
3. **Adjust Single Parameter:** Change one value (e.g., `defenseMitigationFactor`), test, measure impact.
4. **Iterate:** Record new kill times. If improvement, commit the change. If regression, revert.
5. **Cross-Check:** Ensure difficulty curve feels smooth across all 4 difficulty modes.

---

## Common Tuning Scenarios

### "Combat feels too slow"
→ Increase `baseDamageScale` to 1.1–1.2
→ Reduce `baseEnemyHealth` by 20%

### "Defense is useless"
→ Increase `defenseMitigationFactor` to 0.7
→ Reduce `minDamagePercent` to 0.01

### "Leveling is too fast"
→ Increase `xpScalingPerLevel` to 1.12
→ Reduce `baseXPPerEnemy` by 20%

### "Boss encounters are trivial"
→ Increase `bossDamageMultiplier` to 2.0
→ Increase `bossHealthMultiplier` to 4.0

### "Late-game regions feel too easy"
→ Increase `regionHealthMultStep` to 0.5
→ Increase `regionDamageMultStep` to 0.35

---

## Creating a BalanceConfig Asset

1. In Unity Project window, right-click → **Create → BingoQuest → Balance Config**
2. In Inspector, tweak values under each header
3. Apply changes (Ctrl+S)
4. Play test immediately in demo
5. Iterate until feel is right

**Naming Convention:** `BalanceConfig_[DifficultyMode]_[Version]`
- `BalanceConfig_Normal_v1.asset` (baseline)
- `BalanceConfig_Hard_v2.asset` (hardened variant)

---

## Next Steps

After tuning combat balance:
1. Measure and tune **progression curves** (XP per level, skill points per level)
2. Balance **loot drop rates** and **rarity distribution**
3. Tune **ability costs & cooldowns** per class
4. Create difficulty presets (Easy, Normal, Hard, Nightmare)
5. Add telemetry to measure actual play patterns vs. design intent
