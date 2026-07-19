# Playable Demo

This repository includes a code-driven vertical slice bootstrap for testing the core Bingo Quest loop without hand-authored Unity scene assets.

## How to run

1. Create or open a Unity 3D project.
2. Copy this repository's `Assets/BingoQuest` folder into the Unity project's `Assets` folder.
3. Press Play in any scene, including an empty scene.
4. `DemoBootstrap` auto-spawns and builds the arena, camera, player, systems, HUD, and content at runtime.

## First-run flow

1. Choose a class: Warrior, Rogue, Cleric, Mage, or Ranger.
2. Choose a difficulty preset.
3. Fight enemies, complete Bingo objectives, earn pattern rewards, collect loot, and spawn regional bosses.

## Controls

| Action | Input |
|---|---|
| Move | `WASD` |
| Basic attack | `Space` |
| Abilities | `Q`, `W`, `E`, `R` |
| Dodge objective trigger | `Left Shift` / `Right Shift` |
| Open chest loot | `L` |
| Spawn regional boss | `B` |
| Travel to next unlocked region | `N` |
| Toggle Bingo card overlay | `C` |
| Toggle balance dashboard | `F3` |
| Save / load profile | `F5` / `F9` |
| Change difficulty during play | `1`, `2`, `3`, `4` |

## Demo loop

1. Select class and difficulty.
2. Fight spawning enemies in the current region.
3. Use combat, abilities, dodges, loot, and boss kills to progress Bingo objectives.
4. Open the Bingo card with `C` to track the 5x5 objective board.
5. Complete rows, columns, diagonals, corners, or the full card to earn live stat rewards.
6. Press `B` to spawn the current region's authored boss.
7. Defeat the boss for XP and a relic drop.
8. Press `N` to rotate through unlocked regions and generate a fresh Bingo run.
9. Press `F5` to save and `F9` to reload the current profile.

## Current demo systems

- Runtime class selector: Warrior, Rogue, Cleric, Mage, Ranger.
- Runtime difficulty selector with four presets.
- Data-driven regions, enemies, bosses, objectives, classes, abilities, and loot.
- 5x5 Bingo card overlay with progress bars, completion states, and pattern highlights.
- Pattern reward stat buffs with toast notifications.
- Boss spawn/defeat loop with authored regional boss stats and loot.
- Save/load hotkeys using local profile persistence.
- Balance dashboard for difficulty and region multiplier inspection.

## Demo-readiness notes

- The demo intentionally uses IMGUI and runtime primitives, so it is suitable for a playable prototype rather than final presentation.
- No scene setup is required, but the scripts must compile inside a real Unity project.
- The next release-quality pass should focus on Unity compile/play validation, UI polish, input rebinding, and first-play balance tuning.

## Key files

- `Assets/BingoQuest/Scripts/Demo/DemoBootstrap.cs`
- `Assets/BingoQuest/Scripts/Demo/ClassSelectorUI.cs`
- `Assets/BingoQuest/Scripts/Demo/DifficultySelectorUI.cs`
- `Assets/BingoQuest/Scripts/Demo/BingoCardOverlay.cs`
- `Assets/BingoQuest/Scripts/Demo/PatternRewardToast.cs`
- `Assets/BingoQuest/Scripts/Demo/DemoWorldDirector.cs`
- `Assets/BingoQuest/Scripts/Demo/DemoPlayerController.cs`
- `Assets/BingoQuest/Scripts/Demo/DemoEnemySpawner.cs`
- `Assets/BingoQuest/Scripts/Demo/DemoHudOverlay.cs`
