# Content Authoring System

Bingo Quest uses a data-driven content system built on Unity ScriptableObjects. This allows designers to create and balance content without touching code.

## Overview

The content system is organized into four main types:

1. **EnemyDefinition** - Individual enemy types with stats, rewards
2. **BossDefinition** - Boss encounters with phases and special stats
3. **AbilityPool** - Collections of abilities for characters/bosses
4. **ObjectivePool** - Collections of objectives for Bingo card generation

## Creating Content

### Enemy Definitions

```csharp
EnemyDefinition:
  EnemyId: "goblin"
  DisplayName: "Goblin"
  BaseHealth: 30
  BaseAttack: 6
  BaseDefense: 1
  BaseCritChance: 0.02
  BaseDodgeChance: 0.01
  ExperienceReward: 15
  LootDropChance: 0.03
  LootMaterialId: "goblin_fang"
```

**Steps:**
1. Right-click in `Assets/BingoQuest/Content/Enemies/`
2. Create > EnemyDefinition
3. Fill in name, stats, rewards
4. Register in ContentLibrary before game start

### Boss Definitions

```csharp
BossDefinition:
  BossId: "dragon_lord"
  DisplayName: "Dragon Lord"
  BaseHealth: 500
  BaseAttack: 25
  BaseDefense: 8
  ExperienceReward: 500
  LootCount: 5
  PhaseNames: ["Phase 1", "Phase 2", "Phase 3"]
```

### Ability Pools

```csharp
AbilityPool:
  PoolId: "warrior_abilities"
  name: "Warrior Abilities"
  Abilities:
    - AbilityId: "slash"
      Name: "Slash"
      Cooldown: 0.6
      DamageScale: 1.3
      ElementType: Physical
    - AbilityId: "whirlwind"
      Name: "Whirlwind"
      Cooldown: 3.0
      DamageScale: 2.0
      ElementType: Physical
```

### Objective Pools

```csharp
ObjectivePool:
  PoolId: "tier1_objectives"
  name: "Tier 1 Objectives"
  Objectives:
    - ObjectiveId: "kill_5"
      Type: Kill
      RequiredProgress: 5
    - ObjectiveId: "damage_100"
      Type: Damage
      RequiredProgress: 100
    - ObjectiveId: "crits_3"
      Type: CriticalHit
      RequiredProgress: 3
```

## Integration with Game

### Registering Content

```csharp
var library = new ContentLibrary();
library.RegisterEnemy(enemyDef);
library.RegisterBoss(bossDef);
library.RegisterAbilityPool(abilityPool);
library.RegisterObjectivePool(objectivePool);
```

### Using Enemy Stats

```csharp
var enemyDef = library.GetEnemy("goblin");
var stats = ContentFactory.CreateStatsFromEnemy(enemyDef);
// Apply to enemy Combatant
```

### Creating Abilities from Pool

```csharp
var pool = library.GetAbilityPool("warrior_abilities");
var abilities = library.GetAbilitiesFromPool("warrior_abilities");
foreach (var abilityData in abilities)
{
    var ability = ContentFactory.CreateAbilityFromData(abilityData);
    actionBar.SetAbility(slot, ability);
}
```

## Folder Structure

```
Assets/BingoQuest/Content/
  Enemies/
    Goblin.asset
    Orc.asset
    Skeleton.asset
  Bosses/
    DragonLord.asset
    LichKing.asset
  Abilities/
    WarriorAbilities.asset
    MageAbilities.asset
    RangerAbilities.asset
  Objectives/
    Tier1Objectives.asset
    Tier2Objectives.asset
    Tier3Objectives.asset
```

## Design Guidelines

- **Enemy difficulty curves**: Check region multipliers when setting base stats
- **Ability balance**: DamageScale 1.0 = 100% of character attack; higher scales should have longer cooldowns
- **Objective progression**: Arrange by difficulty within pools; later positions = harder objectives
- **Loot rewards**: ExperienceReward should match region difficulty multipliers

## Example: Adding a New Enemy Type

1. Create `Assets/BingoQuest/Content/Enemies/Troll.asset`
2. Set `EnemyId = "troll"`, `DisplayName = "Troll"`
3. Stats: Health 120, Attack 14, Defense 4, Crit 0.08, Dodge 0.02
4. Rewards: XP 50, Loot Chance 0.05, Material "troll_hide"
5. Register in demo or content loader
6. Add to ObjectivePool if needed (kill 3 trolls, etc.)

See `ContentSystemTests.cs` for unit test examples.
