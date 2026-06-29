# Playable Demo

This repository now includes a code-driven playable demo bootstrap.

## How to run

1. Open the project in Unity.
2. Press Play in any scene (or an empty scene).
3. `DemoBootstrap` auto-spawns and builds the demo arena at runtime.

## Controls

- Move: `WASD`
- Basic attack: `Space`
- Abilities: `Q`, `W`, `E`, `R`
- Dodge objective trigger: `Left Shift` / `Right Shift`
- Open chest loot: `L`
- Trigger boss objective event: `B`

## Demo loop

- Fight spawning enemies in the arena.
- Complete objective events through combat actions.
- Progress the Bingo card and trigger pattern rewards.
- Gain experience and levels from kills.
- Collect loot and currency from drops/chests.

## Files

- `Assets/BingoQuest/Scripts/Demo/DemoBootstrap.cs`
- `Assets/BingoQuest/Scripts/Demo/DemoPlayerController.cs`
- `Assets/BingoQuest/Scripts/Demo/DemoEnemySpawner.cs`
- `Assets/BingoQuest/Scripts/Demo/DemoHudOverlay.cs`
