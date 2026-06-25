# Bingo Quest (Xbox RPG) – Production Blueprint

This repository now contains a complete implementation blueprint for **Bingo Quest**, an Xbox-ready action RPG where the Bingo board is the central game system.

## 1) Vision

**Core loop**
1. Enter region
2. Generate run-specific Bingo card
3. Fight/explore/loot/craft to complete objectives
4. Earn square + pattern rewards
5. Defeat boss
6. Progress to harder zones and ascensions

The card is not a side system: every combat, progression, economy, and social feature feeds or is modified by card progression.

---

## 2) Combat Flow (Recommended Final Direction)

### Chosen model: **Diablo-lite ARPG combat**
- Left stick/WASD movement
- Dodge roll with invulnerability window
- 4 active skills + class passive
- Cooldowns, status effects, elite modifiers, telegraphed boss mechanics

### Why this is the best fit
- Skill expression stays high (positioning, timing, target priority)
- Objectives complete as a natural consequence of good play
- Controller-first pacing works on Xbox/PC
- Supports deep loot/build experimentation for 100+ hour longevity

### Combat-to-Bingo flow
1. Player executes actions (`kill`, `crit`, `dodge`, `ability use`, `status apply`)
2. Combat emits typed events (`CombatEventBus`)
3. Objective system consumes events and updates matching squares
4. Completed squares trigger immediate rewards and board-state recalculation
5. Pattern completions apply temporary or run-long power spikes

### Board-powered combat identity
- Row completion: offensive buff (example: `+20% fire damage`)
- Column completion: defensive/utility buff (example: `+10% dodge chance`)
- Diagonal completion: special unlock (example: summon companion)
- Full card: short-duration “Bingo Avatar” overdrive state

This keeps players focused on “one more power spike” rather than checklist chores.

---

## 3) Social/PvP/Co-op/Guild/Raid Systems (Bingo-Native)

### Guild Bingo Boards (weekly)
- Shared large board (example: 20x20)
- Every member contributes objective progress
- Rewards at lines, corners, advanced patterns, full board
- Encourages mixed guild composition (casual + hardcore)

### World Boss Bingo
- Boss fight has shared objective board (part breaks, interrupts, survival checks)
- Completed rows debuff boss or unlock raid buffs
- Server-wide progression event framing

### PvP Bingo Wars
- Same mirrored card for all participants
- Win by first line/pattern completion, not only kill count
- Keeps mode less gear-dominant and more tactical

### Co-op Synergy Squares
- Only generated in co-op playlists
- Objectives require teamwork (revives, combos, shared survival)

### Territory Control
- Guilds compete by filling region-specific boards
- Weekly ownership grants economy and access benefits

### Ghost Boards (high ROI first social feature)
- Save completion path, order, build, and final time
- Others race asynchronous “ghost” records
- Delivers competitive social loop without full-time live matchmaking dependency

---

## 4) Scalable Technical Architecture (Unity 6 / C#)

### High-level modules
- `Core`: bootstrap, DI setup, service registration
- `Gameplay.Combat`: stats, damage pipeline, status effects, ability execution
- `Gameplay.Objectives`: objective contracts + handlers
- `Gameplay.Bingo`: card state, pattern evaluation, reward triggers
- `Gameplay.Progression`: classes, skill tree, run/meta progression
- `Gameplay.Loot`: item generation, rarity/affix logic, bingo-affix effects
- `Gameplay.Regions`: biome rules, enemy pools, boss definitions
- `Meta.Town`: vendors, upgrades, economy sinks
- `Meta.Quests`: story/side/daily/weekly/bingo quests
- `Meta.Seasons`: seasonal objective pools, modifiers, rewards
- `Platform.Save`: save abstraction, autosave/manual/cloud/provider adapters
- `Presentation.UI`: controller-first views, HUD, card UI
- `Presentation.Input`: New Input System action maps
- `Telemetry`: statistics + achievement ingestion

### Key design principles
- ScriptableObject-authored content
- Event-driven gameplay updates
- Logic/presentation separation
- Interface-first systems for unit testing
- No monolithic manager classes

---

## 5) Core Contracts (Implementation Targets)

### Objective contract
```csharp
public interface IObjective
{
    string ObjectiveId { get; }
    int CurrentProgress { get; }
    int RequiredProgress { get; }
    bool IsComplete { get; }
    void Initialize(ObjectiveContext context);
    void UpdateProgress(in ObjectiveEvent evt);
    void Complete();
}
```

### Event contract
```csharp
public readonly record struct ObjectiveEvent(
    ObjectiveEventType Type,
    string SourceId,
    int Amount,
    ElementType Element,
    bool IsCritical,
    bool InCoop,
    bool InPvp
);
```

### Card generator contract
```csharp
public interface ICardGenerator
{
    BingoCard Generate(CardGenerationRequest request);
}
```

Generation rules:
- Respect zone/level/class/difficulty
- Weighted category mix
- No duplicates
- No impossible combinations
- Completion viability guaranteed

---

## 6) Suggested Folder Structure

```text
Assets/
  BingoQuest/
    Scripts/
      Core/
      Gameplay/
        Bingo/
        Objectives/
        Combat/
        Loot/
        Progression/
        Regions/
      Meta/
        Quests/
        Town/
        Seasons/
      Platform/
        Save/
      Presentation/
        UI/
        Input/
      Telemetry/
    ScriptableObjects/
      Classes/
      Regions/
      Enemies/
      Objectives/
      Loot/
      CardModifiers/
      Bosses/
      Seasons/
    Prefabs/
    Scenes/
      Boot.unity
      MainMenu.unity
      Town.unity
      Region_Runtime.unity
      BossArena.unity
    UI/
    Tests/
      EditMode/
      PlayMode/
```

---

## 7) Content Targets

- Starting classes: Warrior, Mage, Ranger, Rogue, Cleric
- Hidden classes: Necromancer, Bingo Knight (unlocks through meta goals)
- Regions: Whispering Forest, Forgotten Mines, Frozen Peaks, Infernal Depths
- Enemy tiers: Normal, Elite, Champion, Boss, World Boss
- Status effects: Burn, Freeze, Poison, Bleed, Shock, Curse
- Loot rarities: Common → Mythic
- Bingo-affix items as build-defining archetypes

---

## 8) Economy + Meta Progression

Persistent currencies:
- Fate Shards
- Bingo Tokens
- Hero Medals

Unlock vectors:
- Classes, talents, regions, cosmetics, card modifiers

Design guardrails:
- Keep run rewards meaningful even on failed runs
- Prevent economy inflation with town sinks and crafting costs

---

## 9) UI/UX + Xbox Compliance Direction

- Controller-first navigation and focus order
- One-button instant Bingo card access at all times
- Legible typography and contrast-safe HUD
- Safe-area compliant layouts
- Input rebinding support via New Input System

Primary screens:
- Main Menu, Character, Inventory, Skill Tree, Map, Quest Log, Bingo Card, Town Hub, Settings

---

## 10) Save/Data Model

Must support:
- Autosave
- Manual save
- Cloud save
- Multiple profile slots

Persistent payload:
- Character state
- Inventory/loadouts
- Meta progression
- Quest/achievement state
- Lifetime statistics

Use provider abstraction to keep save backend swappable.

---

## 11) Statistics + Achievements

Track at minimum:
- Enemies killed
- Cards completed
- Objectives completed
- Bosses defeated
- Gold earned
- Damage dealt/taken
- Fastest card completion
- Rarest item found

Achievement examples:
- First Line
- Full House
- Lucky Legend
- Master of Numbers
- Bingo God

---

## 12) Endgame + Seasonal Live Framework

Endgame modes:
- Adventure
- Nightmare
- Chaos
- Card Ascensions 1–100

Season framework:
- Season objective pools
- Season bosses/rewards
- Rotation rules
- Data-driven enable/disable flags

---

## 13) Performance + Certification Checklist (High Level)

Performance:
- Event batching where possible
- Pool frequently spawned combat objects
- Avoid per-frame allocations in objective/combat loops
- Profile card generation and reward resolution spikes

Xbox submission readiness:
- Suspend/resume stability
- Save integrity and recovery
- Controller disconnect handling
- TRC-compliant messaging and error handling

---

## 14) Practical Production Roadmap

1. Vertical slice: one region, one boss, one 5x5 card, three classes
2. Combat polish + objective reliability pass
3. Loot and bingo-affix build depth
4. Meta progression + town economy
5. Social layer phase 1: Ghost Boards + Guild Board prototype
6. Endgame ascensions + seasonal pipeline

This sequence minimizes risk while proving the central Bingo-combat identity early.
