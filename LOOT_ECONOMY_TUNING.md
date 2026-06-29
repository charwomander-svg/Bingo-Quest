# Loot Economy Tuning Guide

## Overview

Bingo Quest's loot economy is managed by **LootConfig**, a ScriptableObject that controls:
- **Rarity distribution** — How often each rarity tier drops
- **Affix generation** — Number of modifiers per rarity
- **Power scaling** — Item level + rarity multipliers
- **Drop rates** — Enemy/chest/boss loot chances
- **Milestone rewards** — Boss loot and chest rewards

Tuning the loot economy directly impacts **player progression**, **gear satisfaction**, and **long-term engagement**.

---

## LootConfig Structure

### Rarity Distribution

**Common Weight** (30–80, default 50): How often Common items drop
**Uncommon Weight** (10–40, default 25): How often Uncommon items drop
**Rare Weight** (5–20, default 15): How often Rare items drop
**Epic Weight** (1–10, default 7): How often Epic items drop
**Legendary Weight** (0.1–3, default 2): How often Legendary items drop
**Mythic Weight** (0.01–1, default 0.5): How often Mythic items drop

These are **weights**, not percentages. They're normalized before use.

**Formula:**
```
P(Common) = commonWeight / (sum of all weights)
P(Uncommon) = uncommonWeight / (sum of all weights)
... etc
```

**Default Distribution:**
| Rarity | Weight | Probability |
|--------|--------|-------------|
| Common | 50 | 58.8% |
| Uncommon | 25 | 29.4% |
| Rare | 15 | 17.6% |
| Epic | 7 | 8.2% |
| Legendary | 2 | 2.4% |
| Mythic | 0.5 | 0.6% |

**Tuning Tips:**
- If loot feels worthless (too many Commons), reduce commonWeight to 30–40
- If rares feel too common, reduce rareWeight to 8–10
- Legendary should be rare (2–3 per 100 kills)
- Mythic should be ultra-rare (< 1 per 100 kills)

---

### Rarity Bonuses

**Rarity Power Multiplier** (0.5–1.5, default 1.0): Exponential multiplier per rarity tier
- 1.0 = linear scaling (each tier +25% power)
- 1.2 = exponential scaling (Mythic items 2.5x stronger than Common)
- Use higher values to make rare items feel special

**Rarity Affix Bonus Scale** (1.0–5.0, default 1.0): How much affix values increase per rarity
- 1.0 = each rarity tier adds +1.0 to min/max affix ranges
- 2.0 = each tier adds +2.0 (rare items have much stronger affixes)

**Tuning Tips:**
- If Rare items don't feel better than Uncommon, increase rarityPowerMultiplier to 1.1–1.2
- If late-game items feel weak, increase rarityAffixBonusScale to 1.5

---

### Affix Generation

**Affix Value Base Min** (1–5, default 1): Minimum affix value on Common items
**Affix Value Base Max** (1–5, default 4): Maximum affix value on Common items
**Affix Value Rarity Scale** (0.5–2.0, default 0.5): How much each rarity increases min/max

**Formula:**
```
min = baseMin + (rarityLevel * rarityScale)
max = baseMax + (rarityLevel * rarityScale)
value = random(min, max)
```

**Example (Default):**
| Rarity | Min | Max | Range |
|--------|-----|-----|-------|
| Common | 1.0 | 4.0 | 3 |
| Uncommon | 1.5 | 4.5 | 3 |
| Rare | 2.0 | 5.0 | 3 |
| Epic | 2.5 | 5.5 | 3 |
| Legendary | 3.0 | 6.0 | 3 |
| Mythic | 3.5 | 6.5 | 3 |

**Affixable Stats** (string array): Which stats can roll as affixes
- Default: Attack, Defense, CritChance, DodgeChance, ElementalPower, MaxHealth
- Can add/remove to control which stats players prioritize

**Tuning Tips:**
- If early items have no affixes feel weak, increase baseMin from 1 to 1.5
- If late-game affixes feel too powerful, reduce rarityScale from 0.5 to 0.3

---

### Item Level Scaling

**Power Per Item Level** (0.8–1.2, default 1.0): Exponential multiplier per level
- 1.0 = each level adds 0% power (flat scaling)
- 1.05 = 5% power increase per level
- 1.10 = 10% power increase per level

**Power Roll Variance** (90–110, default 100): Variance window for power rolls
- 90–110 = ±10% variance
- 95–105 = ±5% variance (more deterministic)
- 80–120 = ±20% variance (more random)

**Formula:**
```
rolledPower = basePower * itemLevelMultiplier * rarityMultiplier * bossMultiplier * variance%
```

**Tuning Tips:**
- If loot feels flat across levels, increase powerPerItemLevel to 1.08–1.10
- If loot feels too random, reduce variance to 95–105
- If you want deterministic gear, set to 99–101

---

### Boss & Chest Rewards

**Boss Rarity Bonus Multiplier** (1.0–5.0, default 2.0): How much rarer boss loot is
- 2.0 = boss drops are 2x more likely to be rare/epic
- Used as a bonus to rarity rolls

**Boss Loot Power Multiplier** (1.0–10.0, default 1.5): Boss items have this much more power
- 1.5 = boss loot is 50% stronger than regular loot
- Use to incentivize boss fights

**Chest Drop Count** (1–10, default 3): How many items per chest
- 3 = balanced rewards
- 5 = generous (feels rewarding)
- 1 = sparse (feels punishing)

**Chest Rarity Bonus** (0.0–0.3, default 0.05): How much rarer chest items are
- 0.05 = 5% bonus to rarity rolls
- Chests should feel rewarding

**Tuning Tips:**
- If boss fights feel unrewarding, increase bossDearityBonusMultiplier to 3–4
- If boss items still feel weak, increase bossLootPowerMultiplier to 2.0
- If chests feel stingy, increase chestDropCount to 4–5
- If chests drop too much loot, reduce to 2–3

---

### Drop Rates

**Enemy Drop Rate** (0.0–1.0, default 0.8): Chance a kill yields loot
- 0.8 = 80% of kills drop something
- 1.0 = every kill drops loot
- 0.5 = only 50% of kills drop (loot feels rare)

**Chest Drop Rate** (0.0–1.0, default 1.0): Chance a chest grants items
- Almost always 1.0 (chests should always reward)

**Boss Drop Rate** (0.0–1.0, default 1.0): Chance a boss grants loot
- Almost always 1.0 (boss fights must reward)

**Tuning Tips:**
- If loot feels too sparse, increase enemyDropRate to 0.95–1.0
- If loot feels devalued (too common), reduce to 0.6–0.7
- Chests and bosses should always drop

---

## Integration Points

### ItemGenerator Constructor
```csharp
var generator = new ItemGenerator(seed, lootConfig);
var item = generator.Generate(definition, playerLevel, rarityBonus, isBossLoot: true);
```

### Rarity Distribution
```csharp
var (c, u, r, e, l, m) = config.GetRarityWeights();
// Normalized probabilities summing to 1.0
```

### Chest Rewards
```csharp
var drops = generator.GenerateChestDrops(lootTable, playerLevel);
// Uses ChestDropCount and ChestRarityBonus
```

---

## Common Tuning Scenarios

### "Loot feels weak / not rewarding"
**Problem:** Players don't feel progression; items seem worthless

**Solution:**
- Increase `rarityPowerMultiplier` from 1.0 to 1.15–1.2
- Increase `bossLootPowerMultiplier` from 1.5 to 2.0–2.5
- Increase `powerPerItemLevel` from 1.0 to 1.05–1.08

**Effect:** Each item tier feels significantly stronger. Boss loot becomes special.

---

### "Loot feels overpowered"
**Problem:** Level 10 items trivialize level 20 content

**Solution:**
- Reduce `rarityPowerMultiplier` from 1.0 to 0.9
- Reduce `powerPerItemLevel` from 1.0 to 0.98 (almost flat)
- Reduce `bossLootPowerMultiplier` from 1.5 to 1.1

**Effect:** Gear matters less; progression is slower and more skill-based.

---

### "Rares are too common"
**Problem:** Every kill drops Rare+ loot; feels devalued

**Solution:**
- Reduce `rareWeight` from 15 to 5–8
- Reduce `epicWeight` from 7 to 2–3
- Increase `commonWeight` from 50 to 65–70

**Effect:** Commons become baseline. Rares drop ~5% of time.

---

### "Rares are too rare / never see good loot"
**Problem:** Players grind for hours without seeing Rare drops

**Solution:**
- Increase `rareWeight` from 15 to 25–30
- Increase `epicWeight` from 7 to 12–15
- Reduce `commonWeight` from 50 to 30–40

**Effect:** Rares drop ~30% of time. Feels rewarding.

---

### "Boss loot doesn't feel special"
**Problem:** Boss drops feel same as regular kills

**Solution:**
- Increase `bossDearityBonusMultiplier` from 2.0 to 3.0–4.0
- Increase `bossLootPowerMultiplier` from 1.5 to 2.5–3.0
- Ensure bosses drop guaranteed loot (bossDropRate = 1.0)

**Effect:** 70% of boss kills yield Epic+. Items are 2.5x stronger.

---

### "Chests don't reward enough"
**Problem:** Chest rewards feel mediocre vs. random enemy drops

**Solution:**
- Increase `chestDropCount` from 3 to 4–5
- Increase `chestRarityBonus` from 0.05 to 0.10–0.15
- Ensure chestDropRate = 1.0

**Effect:** Chests yield 4+ items, frequently Rare+. Finding chests feels special.

---

### "Affix values are too low / don't impact gameplay"
**Problem:** Affixes add +1–2 to stats; barely noticeable

**Solution:**
- Increase `affixValueBaseMin` from 1 to 2–3
- Increase `affixValueBaseMax` from 4 to 8–10
- Increase `affixValueRarityScale` from 0.5 to 1.0–1.5

**Effect:** Affixes now matter. A Legendary with 5 affixes can provide +20 Attack.

---

## Balance Testing Workflow

1. **Establish Baseline:**
   - Kill 100 enemies, record rarity distribution
   - Open 10 chests, record avg items/rarity
   - Defeat 3 bosses, record loot quality

2. **Identify Pain Points:**
   - Do players feel loot progression?
   - Are certain rarities over/under-represented?
   - Do boss/chest rewards feel special?

3. **Adjust One Parameter:**
   - Change `rareWeight` by ±3
   - Re-kill 100 enemies
   - Compare distribution

4. **Measure Impact:**
   - If rarity increased, keep change
   - If too many rares now, revert and reduce by less

5. **Cross-Check Stats:**
   - Verify power scaling matches item level expectations
   - Ensure Mythic items feel endgame-worthy

---

## Pre-Tuned Profiles

**Generous (Loot-Heavy):**
- commonWeight: 30
- rareWeight: 25
- epicWeight: 12
- bossLootPowerMultiplier: 2.5
- chestDropCount: 5

**Balanced (Default):**
- commonWeight: 50
- rareWeight: 15
- epicWeight: 7
- bossLootPowerMultiplier: 1.5
- chestDropCount: 3

**Hardcore (Sparse Loot):**
- commonWeight: 70
- rareWeight: 8
- epicWeight: 2
- enemyDropRate: 0.6
- bossLootPowerMultiplier: 1.2
- chestDropCount: 2

---

## Next Steps

1. Create 3 LootConfig variants (Generous, Balanced, Hardcore)
2. Test in demo with each config
3. Measure avg rarity distribution per 100 kills
4. Gather player feedback on loot satisfaction
5. Iterate based on retention and engagement
