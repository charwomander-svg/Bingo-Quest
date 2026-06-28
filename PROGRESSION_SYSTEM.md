# Bingo Quest - Character Progression System

## Overview

The **Progression System** enables character building with:
- **5 distinct classes** with unique stat profiles and ability trees
- **Leveling system** with experience gain and skill point allocation
- **Skill tree progression** with prerequisites, unlock requirements, and stat bonuses
- **Permanent unlocks** that grant abilities and stat improvements

---

## Class System

### Built-In Classes

Each class has unique stat modifiers applied to the base character stats:

#### Warrior
- **Health:** 1.3x (tank)
- **Attack:** 1.1x
- **Defense:** 1.4x (highest)
- **Crit Chance:** -5%
- **Dodge Chance:** -5%
- **Passive:** Shield Mastery
- **Theme:** Melee tank with high survivability

#### Mage
- **Health:** 0.8x (squishy)
- **Attack:** 1.4x (highest damage)
- **Defense:** 0.7x (vulnerable)
- **Crit Chance:** +10%
- **Dodge Chance:** +10%
- **Passive:** Mana Shield
- **Theme:** Ranged caster with burst damage

#### Ranger
- **Health:** 1.0x (balanced)
- **Attack:** 1.2x
- **Defense:** 0.9x
- **Crit Chance:** +15% (high)
- **Dodge Chance:** +15% (high)
- **Passive:** Steady Aim
- **Theme:** Agile archer with accuracy

#### Rogue
- **Health:** 0.9x
- **Attack:** 1.3x
- **Defense:** 0.8x
- **Crit Chance:** +20% (highest)
- **Dodge Chance:** +20% (highest)
- **Passive:** Evasion
- **Theme:** Swift striker with precision

#### Cleric
- **Health:** 1.1x
- **Attack:** 0.9x (lowest damage)
- **Defense:** 1.1x
- **Crit Chance:** 0%
- **Dodge Chance:** +5%
- **Passive:** Holy Shield
- **Theme:** Hybrid healer with support

### Creating a Character

```csharp
var warriorTree = SkillTreeFactory.CreateWarriorTree();
var character = new Character(
    id: "player_1",
    name: "Conan",
    classDefinition: BuiltInClasses.Warrior,
    skillTree: warriorTree
);

Debug.Log(character);
// Output: Conan (Warrior) - Level 1 Warrior | EXP: 0/100 | Skill Points: 2 | ...
```

---

## Leveling & Experience

### Experience Gain

```csharp
character.Progression.GainExperience(50);  // Add 50 XP

// Automatic level-up when reaching threshold
if (character.Progression.Level > 1)
{
    Debug.Log("Level up!");
}
```

**Leveling Formula:**
- Starting Threshold: 100 XP
- Per Level: `100 × 1.1^(level-1)`
- Skill Points: 1 point per level (+ 2 starting)

**Events:**
- `OnLevelUp(int newLevel)`: Fired when character levels
- `OnSkillPointsChanged(int availablePoints)`: Fired when points change

---

## Skill Tree System

### SkillNode

A node represents a learnable ability or stat upgrade:

```csharp
var fireballNode = new SkillNode("fireball", "Fireball")
{
    DisplayName = "Fireball",
    Description = "Hurl a ball of fire",
    PointCost = 1,
    MinimumLevel = 1,
    AttackBonus = 3,
    UnlocksAbilityId = "ability_fireball"
};
```

**Properties:**
- `NodeId`: Unique identifier
- `PointCost`: Skill points required to unlock
- `MinimumLevel`: Character level requirement
- `Prerequisites`: Other nodes that must be unlocked first
- `AttackBonus` / `DefenseBonus` / `HealthBonus`: Permanent stat increases
- `CritChanceBonus` / `DodgeChanceBonus`: Percentage bonuses
- `UnlocksAbilityId`: Optional ability unlocked when node is purchased

### SkillTree

Container for all nodes in a class progression:

```csharp
var tree = SkillTreeFactory.CreateWarriorTree();

// Query nodes
var slashNode = tree.GetNode("slash");
bool isUnlocked = tree.IsNodeUnlocked("slash");

// Modify unlocks
tree.UnlockNode("slash");
tree.LockNode("slash");

// Get unlocked nodes
var unlockedList = tree.GetUnlockedNodes();
```

### Unlock Requirements

A skill can be unlocked only if:

```csharp
node.CanUnlock(
    currentLevel: character.Progression.Level,
    availablePoints: character.Progression.SkillPoints,
    unlockedNodes: skillTree.UnlockedNodes
)
```

1. **Level Requirement Met**: `currentLevel >= MinimumLevel`
2. **Sufficient Points**: `availablePoints >= PointCost`
3. **Prerequisites Unlocked**: All nodes in `Prerequisites` list are unlocked

---

## Pre-Built Skill Trees

### Warrior Tree

Progression emphasizes survivability and AoE:

```
Level 1
├─ Slash (1 pt, 0 prereqs)
│  └─ Shield Bash (1 pt, +3 DEF)
│     ├─ Whirlwind (2 pts, +5 ATK, AoE)
│     └─ Defensive Stance (1 pt, +4 DEF)
│
Level 10
└─ Execute (3 pts, +10 ATK, +25% CRIT)
```

**Unlocks Abilities:**
- `ability_slash`
- `ability_shield_bash`
- `ability_whirlwind`
- `ability_defensive_stance`
- `ability_execute`

### Mage Tree

Progression emphasizes spell damage and protection:

```
Level 1
├─ Fireball (1 pt)
│  └─ Frostbolt (1 pt)
│     └─ Mana Shield (2 pts, +5 DEF)
│
Level 15
└─ Meteor Storm (4 pts, +12 ATK)
```

### Ranger Tree

Progression emphasizes crit chance and multi-attack:

```
Level 1
├─ Basic Shot (1 pt, +5% CRIT)
│  └─ Aimed Shot (2 pts, +15% CRIT)
│     └─ Multishot (3 pts)
```

---

## Character Progression

### Unlocking Skills

```csharp
bool unlocked = character.Progression.TryUnlockSkill("fireball");

if (unlocked)
{
    Debug.Log("Fireball unlocked!");
    character.Progression.ApplyBonusesToStats(character.Stats);
}
```

**What Happens on Unlock:**
1. Deduct skill points
2. Mark node as unlocked in skill tree
3. Apply stat bonuses (ATK, DEF, HP, CRIT, DODGE)
4. Fire `OnAbilityUnlocked` event if an ability is granted
5. Log the unlock

### Stat Bonuses

Bonuses from skill tree accumulate:

```csharp
int totalAttackBonus = character.Progression.GetStatBonus("attack");
float totalCritBonus = character.Progression.GetFloatBonus("crit_chance");

// Apply to current stats
character.Progression.ApplyBonusesToStats(character.Stats);
```

**Bonus Types:**
- `health`: Direct HP increase
- `attack`: ATK stat increase
- `defense`: DEF stat increase
- `crit_chance`: Percentage increase (e.g., 0.1 = +10%)
- `dodge_chance`: Percentage increase

### Level Up Rewards

When a character levels:

```csharp
progression.OnLevelUp += (newLevel) =>
{
    Debug.Log($"Now level {newLevel}!");
    UpdateUI(progression);
};

progression.OnSkillPointsChanged += (points) =>
{
    Debug.Log($"Available skill points: {points}");
    UpdateSkillPointsUI(points);
};
```

---

## Integration Example

### Complete Character Setup

```csharp
// Create warrior character
var warriorTree = SkillTreeFactory.CreateWarriorTree();
var warrior = new Character("p1", "Thorin", BuiltInClasses.Warrior, warriorTree);

Debug.Log(warrior);
// Output: Thorin (Warrior) - Level 1 | HP: 130/130 | ATK: 11 | DEF: 7

// Gain experience and level up
warrior.Progression.OnLevelUp += (level) =>
{
    Debug.Log($"Warrior is now level {level}!");
};

warrior.Progression.GainExperience(250);  // Level up to 3

// Unlock skills
warrior.Progression.TryUnlockSkill("slash");          // Success
warrior.Progression.TryUnlockSkill("shield_bash");    // Success
warrior.Progression.TryUnlockSkill("whirlwind");      // Fail (level too low)

// Gain more levels and try again
warrior.Progression.GainExperience(500);
warrior.Progression.TryUnlockSkill("whirlwind");      // Success!

// Apply all bonuses
warrior.Progression.ApplyBonusesToStats(warrior.Stats);
Debug.Log(warrior.Stats);
// Output: HP: 130/135 | ATK: 16 | DEF: 11 | ...
```

---

## Extensibility

### Adding Custom Classes

```csharp
var customClass = new ClassDefinition("paladin", "Paladin")
{
    Description = "Holy warrior",
    HealthMultiplier = 1.2f,
    AttackMultiplier = 1.15f,
    DefenseMultiplier = 1.3f,
    CritChanceBonus = -0.05f,
    DodgeChanceBonus = 0.05f,
    PassiveAbilityId = "paladin_divine_shield"
};

var character = new Character("p1", "Uther", customClass, customTree);
```

### Adding Custom Skill Trees

```csharp
var tree = new SkillTree();

tree.AddNode(new SkillNode("custom_ability", "Custom Ability")
{
    PointCost = 2,
    MinimumLevel = 5,
    Prerequisites = new() { "slash" },
    AttackBonus = 5,
    UnlocksAbilityId = "ability_custom"
});

var character = new Character("p1", "Hero", classDefinition, tree);
```

---

## Performance Notes

- Skill tree operations are O(1) to O(n) depending on prerequisite chains
- Stat bonus application is O(number of unlocked nodes)
- Experience gain is fast with early termination

---

## Summary

The Progression System provides:
- ✅ 5 distinct, balanced classes with unique identities
- ✅ Flexible skill tree architecture with prerequisites
- ✅ Level-up and experience system
- ✅ Permanent stat and ability unlocks
- ✅ Easy to extend with custom classes/trees
- ✅ 23 comprehensive unit tests

Ready for meta progression, cosmetics, and seasonal content!
