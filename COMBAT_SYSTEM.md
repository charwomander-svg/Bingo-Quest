# Bingo Quest - Combat System Documentation

## Overview

The **Combat System** is a Diablo-lite ARPG framework with:
- Character stats and damage calculation
- 4-ability action bar with cooldowns
- Status effects (Burn, Freeze, Poison, Bleed, Shock, Curse)
- Dodging and critical hits
- Objective event emission for Bingo progression

---

## Core Components

### 1. CharacterStats

Character attributes that drive all combat calculations.

```csharp
var stats = new CharacterStats
{
    Health = 100,
    MaxHealth = 100,
    Attack = 15,
    Defense = 8,
    CritChance = 0.2f,    // 20% crit chance
    CritDamage = 1.5f,    // 1.5x multiplier
    DodgeChance = 0.1f,   // 10% dodge chance
    AttackSpeed = 1.0f,   // Cooldown multiplier
    ElementalPower = 5    // Bonus to elemental damage
};

stats.TakeDamage(25);  // Now at 75 HP
stats.Heal(10);        // Back to 85 HP
```

**Key Properties:**
- `Health`/`MaxHealth`: Current and max HP
- `Attack`/`Defense`: Damage scaling and mitigation
- `CritChance` (0-1): Probability of critical hit
- `CritDamage`: Multiplier applied on crit (e.g., 1.5 = 50% bonus)
- `DodgeChance` (0-1): Probability of dodging incoming damage
- `AttackSpeed`: Multiplier for cooldown reduction (0.5 = 2x faster)
- `ElementalPower`: Bonus damage for status effects

### 2. DamageCalculator

Computes actual damage with all modifiers.

```csharp
var calc = new DamageCalculator();

var abilityDef = new AbilityDefinition
{
    DamageScale = 1.2f,
    ElementType = ElementType.Fire
};

var result = calc.CalculateDamage(attacker, defender, abilityDef, difficultyModifier: 1.0f);

Debug.Log(result); // Output: "CRIT! 45 damage (Fire)" or "DODGED!"
```

**Damage Formula:**
```
baseDamage = AttackerAttack × AbilityDamageScale + ElementalPower (if not Physical)
isCrit = Random() < AttackerCritChance
if isCrit:
    baseDamage *= AttackerCritDamage

mitigatedDamage = max(1, baseDamage - DefenderDefense/2)
finalDamage = mitigatedDamage × difficultyModifier
```

**Dodge Check:**
```
if Random() < DefenderDodgeChance:
    return Dodged (0 damage)
```

### 3. AbilityDefinition

Configuration for an ability (typically authored as ScriptableObject).

```csharp
var fireball = new AbilityDefinition
{
    Id = "fireball",
    Name = "Fireball",
    Cooldown = 2.0f,
    DamageScale = 1.5f,
    ElementType = ElementType.Fire,
    
    AppliesStatusEffect = true,
    StatusEffectType = StatusEffectType.Burn,
    StatusEffectChance = 75,  // 75% chance to burn
    
    IsAOE = true,
    AOERadius = 5.0f
};
```

### 4. Ability + ActionBar

Manage ability cooldowns and execution.

```csharp
var actionBar = new ActionBar();

var ability = new Ability(new AbilityDefinition { Cooldown = 1.5f });
actionBar.SetAbility(AbilitySlot.Primary, ability);

// In Update()
actionBar.ReduceAllCooldowns(Time.deltaTime);

// Execute ability
if (actionBar.GetAbility(AbilitySlot.Primary).TryExecute())
{
    Debug.Log("Ability executed!");
}

// Query UI
float cooldownPercent = actionBar.GetCooldownPercent(AbilitySlot.Primary);
```

### 5. StatusEffectManager

Manages stacking effects, DoT damage, and stat modifications.

```csharp
var effects = new StatusEffectManager();

// Apply burn (3 second duration, 5 DPS)
effects.ApplyEffect(StatusEffectType.Burn, duration: 3.0f, dps: 5.0f);

// Stack another burn
effects.ApplyEffect(StatusEffectType.Burn, duration: 3.0f, dps: 5.0f);
Assert.AreEqual(2, effects.ActiveEffects[StatusEffectType.Burn].StackCount);

// Tick effects and get DoT damage
float damageThisFrame = effects.TickAndGetDamage(Time.deltaTime);
```

**Effect Types & Mechanics:**

| Effect | Duration | Mechanic | Interaction |
|--------|----------|----------|-------------|
| **Burn** | 3s | 5 DPS | Stacks damage |
| **Freeze** | 3s | Slows to 50% attack speed | AttackSpeed *= 0.5 |
| **Poison** | 3s | 4 DPS | Stacks damage |
| **Bleed** | 3s | 3 DPS | Stacks damage |
| **Shock** | 3s | Increases damage taken by 25% | Incoming damage × 1.25 |
| **Curse** | 3s | Reduces offensive power to 75% | DamageMultiplier *= 0.75 |

### 6. Combatant (Main Character Entity)

Core game object that executes combat actions.

```csharp
var playerCombatant = GetComponent<Combatant>();

// Listen to combat events
playerCombatant.OnHealthChanged += hp => UpdateHPBar(hp);
playerCombatant.OnDefeated += () => ShowGameOverScreen();
playerCombatant.OnDamageDealt += (target, result) => PlayCritEffect();

// Execute ability
playerCombatant.ExecuteAbility(AbilitySlot.Primary, enemyCombatant);

// Auto-attack
playerCombatant.AutoAttack(enemyCombatant);

// Take damage
playerCombatant.TakeDamage(25, attacker, isObjective: true);

// Apply status
playerCombatant.ApplyStatusEffect(StatusEffectType.Burn, ElementType.Fire);
```

---

## Integration with Objective System

Combat events automatically emit to `ObjectiveEventBus` for Bingo progression:

```csharp
// When ExecuteAbility hits:
ObjectiveEventBus.Instance.Emit(ObjectiveEvent.CriticalHit(sourceId, damage));
ObjectiveEventBus.Instance.Emit(ObjectiveEvent.Damage(sourceId, finalDamage, element));
ObjectiveEventBus.Instance.Emit(ObjectiveEvent.AbilityUsed(abilityId));

// When status applied:
ObjectiveEventBus.Instance.Emit(ObjectiveEvent.StatusEffectApplied(effectName, element));

// When defeated:
ObjectiveEventBus.Instance.Emit(ObjectiveEvent.Kill(defeatedId));

// When dodge successful:
ObjectiveEventBus.Instance.Emit(ObjectiveEvent.DodgeAction());
```

These events trigger objectives like "Land 5 crits" or "Apply burn 3 times" automatically.

---

## Example Combat Encounter

```csharp
// Setup
var player = Instantiate(playerPrefab).GetComponent<Combatant>();
var enemy = Instantiate(enemyPrefab).GetComponent<Combatant>();

// Equip abilities
var fireball = new Ability(new AbilityDefinition
{
    Id = "fireball",
    DamageScale = 1.5f,
    ElementType = ElementType.Fire,
    Cooldown = 2.0f,
    AppliesStatusEffect = true,
    StatusEffectType = StatusEffectType.Burn,
    StatusEffectChance = 75
});
player.ActionBar.SetAbility(AbilitySlot.Primary, fireball);

// Listen to events
player.OnDamageDealt += (target, result) =>
    Debug.Log($"Dealt {result.FinalDamage} {result.Element} damage" +
              $" {(result.IsCritical ? "CRIT!" : "")}");

// Combat loop
while (player.IsAlive && enemy.IsAlive)
{
    // Player attacks
    if (Input.GetKeyDown(KeyCode.Q))
    {
        player.ExecuteAbility(AbilitySlot.Primary, enemy);
    }

    // Enemy AI (simple counter-attack)
    if (Random.Range(0, 1.0f) > 0.7f)
    {
        enemy.AutoAttack(player);
    }
}
```

---

## Performance Notes

1. **Damage Calculation**: O(1) per hit, uses fast random number generation
2. **Status Effects**: O(active effects) per update frame
3. **Cooldown Reduction**: O(4) per frame (4 ability slots)
4. **Event Emission**: Uses ObjectiveEventBus deferred processing

---

## Future Extensions

### 1. Dodge Roll Invulnerability Window
```csharp
public class DodgeRoll : AbilityBase
{
    protected override void OnExecute()
    {
        StartCoroutine(InvulnerabilityWindow(0.5f));
    }

    private IEnumerator InvulnerabilityWindow(float duration)
    {
        character.IsInvulnerable = true;
        yield return new WaitForSeconds(duration);
        character.IsInvulnerable = false;
    }
}
```

### 2. AOE Damage
```csharp
foreach (var target in GetEnemiesInRadius(epicenter, abilityDef.AOERadius))
{
    ExecuteAbility(abilityDef, target);
}
```

### 3. Elite Modifiers
```csharp
public enum EliteModifier
{
    Fast,      // 1.5x attack speed
    Strong,    // 1.3x damage
    Resistant, // 0.7x damage taken
    Healing    // Regenerates HP
}
```

### 4. Telegraphed Boss Mechanics
```csharp
public class TelegraphedAttack
{
    public float WarningDuration = 1.0f;
    public float DamageRadius = 10.0f;
    
    public bool CanPlayerDodge(Vector3 playerPos) =>
        Vector3.Distance(playerPos, targetZone) > DamageRadius;
}
```

---

## Summary

The Combat System provides:
- ✅ Diablo-lite ARPG feel with abilities and cooldowns
- ✅ Rich stat/damage interactions (crits, defense, scaling)
- ✅ 6 status effects with stacking and mechanics
- ✅ Automatic Objective event emission
- ✅ Easy to extend with custom abilities and modifiers
- ✅ 15+ unit tests for reliability

Ready to integrate with UI, enemies, and environmental hazards!
