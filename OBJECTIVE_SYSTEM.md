# Bingo Quest - Objective System Documentation

## Overview

The **Objective System** is the core mechanic of Bingo Quest. Every square on the Bingo card represents an objective—a gameplay goal that progresses through combat, exploration, and interaction events. This system decouples objectives from the card, making it flexible and data-driven.

---

## Architecture

### 1. Core Contracts

#### `IObjective` Interface
Represents a single objective on the Bingo card. All objectives implement this contract.

```csharp
public interface IObjective
{
    string ObjectiveId { get; }
    int CurrentProgress { get; }
    int RequiredProgress { get; }
    bool IsComplete { get; }
    ObjectiveCategory Category { get; }

    void Initialize(ObjectiveContext context);
    void UpdateProgress(in ObjectiveEvent evt);
    void Complete();
    void Reset();
}
```

**Key Properties:**
- `ObjectiveId`: Unique identifier (e.g., "kill_5", "damage_100")
- `CurrentProgress`: Current steps toward completion
- `RequiredProgress`: Target steps needed
- `IsComplete`: Automatically set when progress >= required
- `Category`: Combat, Survive, Loot, Explore, Social

#### `ObjectiveEvent` Record
Strongly-typed events emitted by combat systems.

```csharp
public readonly record struct ObjectiveEvent(
    ObjectiveEventType Type,
    string SourceId,
    int Amount,
    ElementType Element = ElementType.Physical,
    bool IsCritical = false,
    ...
);
```

**Factory Methods:**
- `ObjectiveEvent.Kill(enemyId)`
- `ObjectiveEvent.Damage(sourceId, amount, element)`
- `ObjectiveEvent.CriticalHit(sourceId, amount)`
- `ObjectiveEvent.DodgeAction()`
- `ObjectiveEvent.StatusEffectApplied(effectName, element)`
- And more...

### 2. Event Bus

`ObjectiveEventBus` is a singleton that distributes events from combat systems to all active objectives.

```csharp
// Combat system emits
ObjectiveEventBus.Instance.Emit(ObjectiveEvent.Kill("goblin_123"));

// Objectives listen automatically via BingoSystem
```

**Features:**
- Deferred event processing (prevents re-entrancy)
- Batch event support for performance
- Safe subscriber error handling

### 3. Core Objectives

Pre-built objective types cover most gameplay scenarios:

| Class | Trigger | Use Case |
|-------|---------|----------|
| `KillEnemiesObjective` | Kill events | "Defeat 10 enemies" |
| `DealDamageObjective` | Damage events | "Deal 500 damage" |
| `CriticalHitsObjective` | Crit events | "Land 5 critical hits" |
| `UseAbilitiesObjective` | Ability usage | "Use 20 abilities" |
| `DodgeActionsObjective` | Dodge events | "Dodge 10 times" |
| `ApplyStatusEffectsObjective` | Status application | "Apply fire 3 times" |
| `LootItemsObjective` | Loot events (rarity-filtered) | "Find 5 rare items" |
| `DefeatBossObjective` | Boss defeat events | "Defeat the boss" |
| `OpenChestsObjective` | Chest open events | "Open 5 chests" |

---

## Bingo Card & Pattern System

### BingoCard
5x5 grid state manager.

```csharp
var card = new BingoCard();
card.SetSquare(0, 0, new BingoSquare(0, 0, "kill_5"));
card.CompleteSquare(0, 0);  // Mark square complete
Debug.Log(card.CompletedSquareCount);  // 1
```

### PatternDetector
Automatically detects completed lines and generates rewards.

```csharp
var detector = new PatternDetector(card);
detector.Evaluate();

foreach (var pattern in detector.DetectedPatterns)
    Debug.Log(pattern);  // Row, Column, DiagonalLeft, etc.

foreach (var newPattern in detector.NewPatterns)
    Debug.Log($"NEW: {newPattern}");  // Only patterns completed this frame
```

**Detected Patterns:**
- `Row`: Full horizontal line
- `Column`: Full vertical line
- `DiagonalLeft`: Top-left to bottom-right
- `DiagonalRight`: Top-right to bottom-left
- `Corners`: All four corner squares
- `FullCard`: All 25 squares

### BingoSystem
Central orchestrator that ties it all together.

```csharp
// Initialize
BingoSystem.Instance.Initialize();

// Generate new run
var context = new ObjectiveContext
{
    ZoneId = "whispering_forest",
    CardDifficulty = 1,
    PlayerClassId = "warrior",
    RunSeed = 12345
};
BingoSystem.Instance.GenerateNewRun(context);

// Listen to events
BingoSystem.Instance.OnSquareCompleted += (row, col) => 
    Debug.Log($"Square {row},{col} complete!");

BingoSystem.Instance.OnPatternDetected += (pattern) =>
    Debug.Log($"Pattern {pattern} completed!");
```

---

## Integration Flow

### 1. Combat System → Event Emission

```csharp
// In your combat system (enemy defeat)
if (enemy.Health <= 0)
{
    ObjectiveEventBus.Instance.Emit(ObjectiveEvent.Kill(enemy.Id));
}

// Ability used
if (ability.IsExecuted)
{
    ObjectiveEventBus.Instance.Emit(ObjectiveEvent.AbilityUsed(ability.Id));
}

// Damage dealt
int damageDealt = CalculateDamage(...);
ObjectiveEventBus.Instance.Emit(
    ObjectiveEvent.Damage("player", damageDealt, ElementType.Fire)
);
```

### 2. Event Processing

- `ObjectiveEventBus` receives event
- Distributes to all subscribed objectives (via `BingoSystem`)
- Each objective's `UpdateProgress()` is called
- Progress is checked; if complete, `Complete()` is invoked

### 3. Card Update

- `BingoSystem` marks corresponding square complete
- Fires `OnSquareCompleted` event
- Triggers `PatternDetector.Evaluate()`

### 4. Pattern Reward

- `PatternRewardHandler` detects new patterns
- Grants gameplay buffs/effects
- Fires `OnRewardGranted` event

---

## Usage Example: Custom Objective

Extend `ObjectiveBase` to create custom objectives:

```csharp
public class DefeatSpecificEnemyObjective : ObjectiveBase
{
    private string targetEnemyId;

    public override ObjectiveCategory Category => ObjectiveCategory.Combat;

    public DefeatSpecificEnemyObjective(string id, string enemyId)
    {
        ObjectiveId = id;
        targetEnemyId = enemyId;
        RequiredProgress = 1;
    }

    protected override void OnProgressUpdate(in ObjectiveEvent evt)
    {
        if (evt.Type == ObjectiveEventType.Kill && evt.SourceId == targetEnemyId)
            CurrentProgress = 1;
    }

    protected override void OnComplete()
    {
        Debug.Log($"Defeated {targetEnemyId}!");
    }
}
```

---

## Testing

Unit tests cover all major scenarios:

```bash
# Run tests in Unity Test Framework
Window > TextMesh Pro > Test Runner
```

Test coverage includes:
- Individual objective progress tracking
- Pattern detection (rows, columns, diagonals, corners, full card)
- New pattern identification
- Card state management
- Element filtering for status effects

---

## Performance Considerations

1. **Event Batching**: Use `EmitBatch()` for rapid events
   ```csharp
   ObjectiveEventBus.Instance.EmitBatch(
       ObjectiveEvent.Damage("player", 50),
       ObjectiveEvent.CriticalHit("player", 150),
       ObjectiveEvent.StatusEffectApplied("burn", ElementType.Fire)
   );
   ```

2. **Pattern Evaluation**: `PatternDetector.Evaluate()` is O(n) per evaluation; called only when squares complete.

3. **Subscriber Safety**: Exception handling prevents one failing objective from breaking others.

---

## Future Extensions

### Data-Driven Generation
Replace placeholder objective generation with ScriptableObject pools:

```csharp
[CreateAssetMenu(menuName = "Bingo/Objective Pool")]
public class ObjectivePool : ScriptableObject
{
    public List<ObjectiveDefinition> objectives;
}
```

### Co-op Synergy Objectives
Objectives that only work in multiplayer (e.g., "Revive teammate 3 times"):

```csharp
public class CoopSynergyObjective : ObjectiveBase
{
    protected override void OnProgressUpdate(in ObjectiveEvent evt)
    {
        if (!evt.InCoop) return;  // Skip if not in co-op
        // ... sync progress across players
    }
}
```

### PvP-Specific Objectives
Competitive objectives for PvP Bingo Wars:

```csharp
public class PvPObjective : ObjectiveBase
{
    protected override void OnProgressUpdate(in ObjectiveEvent evt)
    {
        if (!evt.InPvp) return;
        // ... only count PvP-relevant events
    }
}
```

### Seasonal Modifiers
Seasonal rules that alter objective difficulty or mechanics.

---

## Summary

The Objective System provides:
- ✅ Event-driven, decoupled architecture
- ✅ Flexible objective composition
- ✅ Automatic pattern detection with rewards
- ✅ Scalable design for co-op, PvP, and seasonal content
- ✅ Comprehensive test coverage
- ✅ Performance-optimized event processing

Ready for production implementation!
