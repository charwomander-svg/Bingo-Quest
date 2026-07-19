# Ability Tuning Guide

## Overview

Bingo Quest's ability system is balanced via **AbilityConfig**, a ScriptableObject that controls:
- **Per-slot cooldowns** — Primary, Secondary, Tertiary, Ultimate
- **Per-slot damage scaling** — Multipliers on base ability damage
- **Resource costs** — Mana/energy per ability
- **Difficulty modifiers** — How cooldowns scale per difficulty
- **Special rules** — Cooldown resets, status effects, movement constraints

All parameters are tunable via Inspector without code changes.

---

## AbilityConfig Structure

### Ability Slot Configuration

Each slot (Primary, Secondary, Tertiary, Ultimate) has:

**Base Cooldown** (0.1–10s, defaults: 0.8s, 2.0s, 3.5s, 8.0s)
- Time between executions
- Primary: fast spam (0.8s) → high uptime, low power
- Ultimate: long cooldown (8s) → rare, high impact

**Damage Multiplier** (0.5–3.0, defaults: 1.0x, 1.2x, 1.5x, 2.5x)
- Scales all damage output
- Ultimate deals 2.5x more damage than Primary
- Used for balance: don't give high CD + high damage together

**Resource Cost** (0–100, default: 0)
- Mana/energy spent per cast
- Primary: 0 (always available)
- Tertiary: 20–30 (situational)
- Ultimate: 50+ (reserved for powerful moments)

**Crit Chance Bonus** (0–1.0, default: 0)
- Bonus crit chance on this ability
- Use for special abilities (e.g., rogue Primary: +10% crit)

**Can Cast While Moving** (bool, default: true)
- Some abilities root the player (false)
- Ultimate typically requires standing still

---

### Global Ability Modifiers

**Global Cooldown Multiplier** (0.5–2.0, default: 1.0)
- Scale all cooldowns at once
- 0.8 = all abilities 20% faster
- 1.2 = all abilities 20% slower

**Global Damage Multiplier** (0.5–2.0, default: 1.0)
- Scale all ability damage at once
- Use for balance passes across all abilities
- Does NOT affect weapon damage, only ability scaling

**Haste Bonus** (0–1.0, default: 0)
- Passive cooldown reduction (0–100%)
- 0.1 = 10% cooldown reduction
- Stacks multiplicatively with other multipliers

**Formula:**
```
finalCooldown = baseCooldown * globalCooldownMult * (1 - hasteBonus)
```

---

### Resource Management

**Max Resource Pool** (integer, default: 100)
- Total mana/energy available
- Abilities cost resources, players regenerate

**Resource Regen Per Second** (float, default: 5.0)
- Regen rate
- At 5.0/s, full bar refills in 20s
- Lower = more resource-gated gameplay

**Resource Cost Per Ability Override** (0 = use per-ability, >0 = fixed cost)
- 0 = each slot has own cost
- 25 = all abilities cost 25 (simplifies economy)

---

### Status Effect Tuning

**Status Effect Chance Bias** (0–100%, default: 0)
- Flat bonus to all status effect chances
- +10 = all status effects 10% more likely

**Status Effect Duration Multiplier** (0.5–2.0, default: 1.0)
- How long effects persist
- 1.5 = 50% longer effects
- 0.7 = 30% shorter effects

---

### Cooldown Reset Rules

**Reset Cooldowns On Kill** (bool, default: false)
- Kills reset all ability cooldowns
- Makes playstyle feel fast/rewarding in trash mobs
- Disable for boss fights (prevents cheesing)

**Reset Cooldowns On Critical** (bool, default: false)
- Critical hits reset abilities
- Encourages crit-based playstyles
- Can create infinite combos if not careful

**Cooldown Reset Chance** (0–1.0, default: 0)
- Random chance per kill to reset one random ability
- 0.2 = 20% chance → feels rewarding but not guaranteed

---

### Difficulty Modifiers

**Easy Cooldown Multiplier** (0.5–2.0, default: 0.7)
- Cooldowns 30% shorter → fast-paced, forgiving

**Normal Cooldown Multiplier** (0.5–2.0, default: 1.0)
- Baseline (recommended 1.0)

**Hard Cooldown Multiplier** (0.5–2.0, default: 1.3)
- Cooldowns 30% longer → resource-gated, deliberate

**Nightmare Cooldown Multiplier** (0.5–2.0, default: 1.6)
- Cooldowns 60% longer → extreme resource management required

---

## Integration Points

### Getting Cooldown for a Slot
```csharp
float cooldown = abilityConfig.GetCooldown(AbilitySlot.Primary);
// Returns: baseCooldown * globalMultiplier * (1 - haste)

float finalCooldown = abilityConfig.CalculateFinalCooldown(
    AbilitySlot.Ultimate, 
    baseCooldown: 8.0f, 
    difficulty: DifficultyMode.Hard
);
// Returns: 8.0 * globalMult * (1 - haste) * hardMultiplier (1.3)
```

### Getting Damage for a Slot
```csharp
float baseDamage = 10;
float finalDamage = abilityConfig.GetDamage(AbilitySlot.Tertiary, baseDamage);
// Returns: 10 * slotDamageMultiplier (1.5) * globalDamageMultiplier
```

### Resource Management
```csharp
int cost = abilityConfig.GetResourceCost(AbilitySlot.Ultimate);
int maxPool = abilityConfig.MaxResourcePool;
float regenPerSec = abilityConfig.ResourceRegenPerSecond;
```

---

## Common Tuning Scenarios

### "Gameplay feels slow / boring"
**Problem:** Long ability cooldowns; not enough casts per second

**Solution:**
- Reduce Primary cooldown from 0.8s to 0.5s (faster spam)
- Reduce globalCooldownMultiplier from 1.0 to 0.8 (20% faster)
- Increase ResourceRegenPerSecond from 5 to 8 (less resource-gated)

**Effect:** Player casts more often; feels responsive.

---

### "Gameplay is too chaotic / overwhelming"
**Problem:** Too many abilities available; ability spam is boring

**Solution:**
- Increase Primary cooldown from 0.8s to 1.5s (less spam)
- Increase globalCooldownMultiplier from 1.0 to 1.2 (20% slower)
- Decrease ResourceRegenPerSecond from 5 to 3 (more waiting)
- Increase resource costs (Ultimate: 50 → 75)

**Effect:** Abilities are precious; requires planning.

---

### "Ultimate ability doesn't feel special"
**Problem:** Ultimate damage doesn't justify long cooldown

**Solution:**
- Increase Ultimate BaseCooldown from 8s to 10s (rarer)
- Increase Ultimate DamageMultiplier from 2.5x to 3.5x (more powerful)
- Add Ultimate CritChanceBonus: +0.15 (guaranteed to crit)
- Can Reset Cooldowns On Kill: false → true (instant resets feel rewarding)

**Effect:** 10s between uses, but deals massive damage.

---

### "Primary ability is boring / never used"
**Problem:** Players skip Primary for stronger abilities

**Solution:**
- Decrease Primary cooldown from 0.8s to 0.3s (spam it)
- Increase Primary DamageMultiplier from 1.0x to 1.1x
- Add Primary CritChanceBonus: +0.05
- Enable Reset Cooldowns On Critical (crits reset Primary)

**Effect:** Primary becomes active playstyle; crit-chance gambling.

---

### "Resources feel meaningless"
**Problem:** Players never run out of mana; abilities always available

**Solution:**
- Reduce ResourceRegenPerSecond from 5 to 1.5
- Increase resource costs: Primary 0 → 10, Tertiary 20 → 40, Ultimate 50 → 80
- Enable Cooldown Reset On Kill (so kills refund resources)

**Effect:** Resources become actual gate; must choose abilities carefully.

---

### "Resources too tight / players frustrated"
**Problem:** Players constantly out of mana; feels unplayable

**Solution:**
- Increase ResourceRegenPerSecond from 5 to 10 (double regen)
- Reduce resource costs by 30% (Primary 10 → 7, Ultimate 80 → 56)
- Enable Reset Cooldowns On Kill (free abilities on kills)
- Increase Max Resource Pool from 100 to 150

**Effect:** Resources feel abundant; no friction.

---

### "Difficulty feels unfair on Hard"
**Problem:** Cooldowns too long; can't use abilities fast enough

**Solution:**
- Reduce Hard Cooldown Multiplier from 1.3 to 1.1 (only 10% penalty)
- Keep Normal at 1.0 baseline
- Add Haste Bonus: 0.05 (5% global cooldown reduction)

**Effect:** Hard still harder, but playable.

---

### "Boss fight feels grindy"
**Problem:** Ultimate cooldown too long; no big moments

**Solution:**
- Enable Reset Cooldowns On Critical Hit (every crit = ready ability)
- Decrease Ultimate cooldown from 8s to 5s
- Increase Ultimate damage from 2.5x to 3.0x
- Increase Critical Chance Bonus on Ultimate: +0.20

**Effect:** Skilled play (high crits) = frequent ultimates.

---

## Balance Testing Workflow

1. **Establish Baseline:**
   - Record ability cast rate (casts per 30s)
   - Record resource usage per fight
   - Record average damage per ability

2. **Identify Pain Points:**
   - Which abilities are never used?
   - Do players run out of resources?
   - Do cooldowns feel too long/short?

3. **Adjust One Parameter:**
   - Change Primary cooldown by ±0.2s
   - Play 5 fights
   - Measure new cast rate

4. **Validate:**
   - If faster feels good, keep change
   - If too fast, revert and reduce by less
   - Cross-check all difficulties

5. **Full Pass:**
   - Ensure all 4 slots are used
   - Verify difficulty progression (Easy → Nightmare feels different)
   - Check boss fights for "ultimate moment" frequency

---

## Pre-Tuned Profiles

**Fast (Action-Heavy):**
- Primary CD: 0.4s
- Global CD Mult: 0.7
- Resource Regen: 10/s
- Enable: Reset on Kill, Reset on Critical

**Balanced (Default):**
- Primary CD: 0.8s
- Global CD Mult: 1.0
- Resource Regen: 5/s
- All cooldown resets disabled

**Strategic (Slow):**
- Primary CD: 1.5s
- Global CD Mult: 1.2
- Resource Regen: 2/s
- Enable: Reset on Kill (only)

**Hardcore (Nightmarish):**
- Primary CD: 2.0s
- Global CD Mult: 1.5
- Resource Regen: 1/s
- All resources required; no resets

---

## Next Steps

1. Create 4 AbilityConfig presets (Fast, Balanced, Strategic, Hardcore)
2. Test each in demo with 3 difficulty modes
3. Measure average encounter length (should increase on harder difficulties)
4. Measure player ability usage (all 4 slots should be used)
5. Iterate based on fun factor and clarity
