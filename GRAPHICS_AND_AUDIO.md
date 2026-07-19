# Graphics and Audio Implementation Guide

## Overview

Complete graphics and audio systems for Bingo Quest, including character animations, particle effects, sound management, and combat audio integration. All systems are modular, event-driven, and designed for iterative designer workflows.

## Architecture

### Graphics Systems

#### 1. **CharacterAnimator** — State Machine Animation Controller

Synchronizes Unity Animator parameters with character state:

```csharp
// Core methods
PlayAttack(slotIndex)      // Play attack by ability slot
PlayCast(castDuration)     // Play casting animation
PlayHurt()                 // Play damage/stagger animation
PlayDeath()                // Play death animation
SetGrounded(bool)          // Update grounded state for jump/fall
GetCurrentStateName()      // Query current animation state
IsAnimationPlaying(name)   // Check if specific animation active
```

**Setup in Unity:**
1. Create Animator with state machine (Idle, Running, Attack, Cast, Hurt, Death)
2. Add CharacterAnimator component to character GameObject
3. Assign Animator in Inspector
4. Add animation events to animation clips for footsteps, hit detection, cast release, etc.

**Key Features:**
- Smooth locomotion blending (Speed parameter)
- 8-directional movement (Direction parameter)
- Grounded state for jump/fall detection
- Attack slot indexing (different animations per ability)
- Automatic death disable (prevents movement after death)

---

#### 2. **VFXSpawner** — Particle Effect Manager

Spawns and manages particle effects for combat, abilities, and environment:

```csharp
// Core methods
SpawnEffect(name, position, rotation)           // Generic effect
SpawnHitEffect(position, hitDirection)          // Impact hit at enemy
SpawnCastEffect(position, abilityType)          // Ability cast effect
SpawnEnvironmentEffect(effectType, position)    // Dust, debris, etc.
RegisterEffectPrefab(name, prefab)              // Add effect at runtime
```

**Pre-defined Effect Naming:**
- `Hit` — Generic impact effect
- `Cast_<AbilityType>` — Cast effect per ability (e.g., `Cast_Fire`, `Cast_Ice`)
- `Environment_<Type>` — Environmental effects (e.g., `Environment_Dust`)

**Setup in Unity:**
1. Create ParticleSystem prefabs for each effect (Hit, Cast_Fire, etc.)
2. Add VFXSpawner component to world
3. Populate VFXDefinition list with prefab+name pairs
4. VFXSpawner auto-pools and recycles particle systems

**Example: Creating Fire Cast Effect**
```
Create ParticleSystem:
  - Shape: Sphere, radius 1
  - Emission: 50 particles/sec, burst on play
  - Lifetime: 1 second
  - Velocity: Random 2-4 m/s
  - Color: Orange to red gradient
  - Name: Cast_Fire
```

---

#### 3. **UIEffectLayer** — Screen-Space Damage Numbers & Status

Spawn floating damage numbers and status indicators in world space:

```csharp
// Core methods
SpawnDamageNumber(damage, worldPos, isCrit)    // Float damage text
SpawnStatusIndicator(statusType, worldPos)     // Float status icon
```

**Status Colors:**
- Poison: Green
- Burning: Orange
- Frozen: Cyan
- Stunned: Yellow

**Setup in Unity:**
1. Create TextMesh prefab for damage numbers
2. Add UIEffectLayer component to Canvas
3. Assign TextMesh prefab in Inspector

---

#### 4. **ProjectileTracer** — Ranged Attack Visuals

Renders projectile arcs and trails for ranged attacks:

```csharp
// Core method
Fire(origin, target)  // Launch projectile from origin to target
```

**Uses:**
- LineRenderer for arc visualization
- TrailRenderer for motion blur
- Auto-destroys on impact

---

### Audio Systems

#### 1. **AudioManager** — Centralized Audio Broker

Singleton manager for all game audio:

```csharp
// Core methods
PlaySFX(clipName, position, volumeMultiplier)  // Play sound effect
PlayMusic(clipName, fadeInDuration)            // Play music (looped)
PlayAmbience(clipName, volume)                 // Play ambient sound (looped)
Stop(source, fadeDuration)                     // Stop audio with fade
StopCategory(categoryName, fadeDuration)       // Stop entire category
SetCategoryVolume(categoryName, volume)        // Set category volume (0-1)
GetCategoryVolume(categoryName)                // Get current category volume
```

**Audio Categories (Configurable):**
- `SFX` — Sound effects (is3D: true, poolSize: 20)
- `Music` — Background music (is3D: false, poolSize: 2)
- `Ambience` — Environmental sounds (is3D: false, poolSize: 5)
- `UI` — UI feedback sounds (is3D: false, poolSize: 10)
- `Dialogue` — Voiceover (is3D: false, poolSize: 5)

**Audio Clip Definition:**
```csharp
public class AudioClipDefinition
{
    public string clipName;           // "Footstep_Grass"
    public AudioClip clip;            // WAV/MP3 file
    public string category;           // "SFX"
    public float volume;              // 0-1 override
    public float spatialBlend;        // 0 = 2D, 1 = 3D
}
```

**Setup in Unity:**
1. Create AudioManager prefab with AudioListener component
2. Configure audio categories (SFX, Music, Ambience, UI, etc.)
3. Add AudioClipDefinitions for all sounds in game
4. Mark as DontDestroyOnLoad for persistence

**Example: Combat SFX Configuration**
```
Category: SFX
  - Clip: Player_Hit (volume: 0.8, spatial: 1.0)
  - Clip: Enemy_Hit (volume: 0.9, spatial: 1.0)
  - Clip: Crit_Hit (volume: 1.0, spatial: 1.0)
  - Clip: Fire_Cast (volume: 0.7, spatial: 1.0)
  - Clip: Death (volume: 0.95, spatial: 1.0)
```

---

#### 2. **CombatAudioEvents** — Combat System Audio Hooks

Event-driven audio for damage, abilities, and status effects:

```csharp
// Core methods
OnPlayerTakeDamage(damage, position)
OnPlayerDealDamage(damage, isCrit, position)    // Plays crit or normal hit
OnAbilityCast(abilityType, position)
OnCharacterDeath(position)
OnLevelUp(position)
OnStatusEffectApplied(statusType, position)
```

**Integration:**
Connect these to your damage system's events:
```csharp
characterStats.OnDamageDealt += combatAudio.OnPlayerDealDamage;
characterStats.OnDamageTaken += combatAudio.OnPlayerTakeDamage;
ability.OnCast += combatAudio.OnAbilityCast;
character.OnDeath += combatAudio.OnCharacterDeath;
```

---

#### 3. **FootstepAudio** — Synchronized Footstep System

Plays footstep sounds triggered by animation events:

```csharp
// Core methods
PlayFootstep()               // Single footstep (called from animation event)
SetTerrainType(terrainType)  // Change surface type (Grass, Stone, Metal)
PlayFootstepSequence(count)  // Multiple rapid footsteps
```

**Terrain Types:**
- `Grass` — Soft, muffled
- `Stone` — Hard, echo
- `Metal` — Metallic, hollow

**Setup in Unity:**
1. Add FootstepAudio component to character
2. In animation editor, add animation event at foot-strike frames
3. Call `PlayFootstep()` on that event
4. Change terrain type when entering new ground (via collider triggers)

**Example: Animation Events**
```
Idle animation: No events
Walk animation: 
  - Frame 15 (left foot): PlayFootstep()
  - Frame 30 (right foot): PlayFootstep()
Run animation:
  - Frame 10 (left foot): PlayFootstep()
  - Frame 20 (right foot): PlayFootstep()
```

---

#### 4. **UISoundEffects** — Menu Audio Feedback

Simple sound triggers for UI interactions:

```csharp
// Methods (call on button click, menu open, etc)
OnButtonClick()     // Click feedback
OnMenuOpen()        // Menu open sound
OnMenuClose()       // Menu close sound
OnItemPickup()      // Item acquired sound
OnError()          // Error/invalid action sound
OnNotification()   // Alert/notification sound
```

**Setup in Unity:**
1. Create UI Button with onClick event
2. Add component → Events → onClick
3. Drag UISoundEffects object
4. Select OnButtonClick()

---

## Integration Workflows

### Scenario 1: Player Takes Damage

```csharp
// In CharacterStats.TakeDamage()
OnDamageTaken?.Invoke(damageAmount, position);

// CombatAudioEvents listens:
combatAudioEvents.OnPlayerTakeDamage(damageAmount, position);
  → Plays "Player_Hit" at 3D position

// UIEffectLayer listens:
uiEffects.SpawnDamageNumber(damageAmount, position, isCrit: false);
  → Shows floating damage number
```

### Scenario 2: Player Casts Ability

```csharp
// In Ability.Execute()
animator.PlayCast(castDuration);
vfxSpawner.SpawnCastEffect(position, abilityType);
combatAudioEvents.OnAbilityCast(abilityType, position);
  → Animation plays
  → Cast particle effect spawns
  → Cast sound plays

// On cast release event:
combatAudioEvents.OnPlayerDealDamage(damage, isCrit, targetPos);
vfxSpawner.SpawnHitEffect(targetPos, hitDirection);
```

### Scenario 3: Character Death

```csharp
// In CharacterStats.Die()
animator.PlayDeath();
combatAudioEvents.OnCharacterDeath(position);
vfxSpawner.SpawnEffect("Death", position);
// Character is disabled and becomes corpse
```

---

## Audio Tuning Parameters

### Volume Levels (dB reference)
- Music: -6 dB (0.5x volume)
- SFX: 0 dB (1.0x volume, normal)
- UI: -3 dB (0.7x volume, subtle)
- Ambience: -12 dB (0.25x volume, background)

### Spatial Audio (3D positioning)
- Combat SFX: 100% spatial (full 3D audio)
- Footsteps: 100% spatial
- Ability casts: 80% spatial (slightly localized)
- Music/UI: 0% spatial (always centered in player ears)

### Pitch Variation
- Footsteps: Random ±5% pitch variation (natural feel)
- Hit sounds: Random ±10% (avoid repetition)
- UI clicks: Random ±2% (subtle variation)

---

## Animation Setup Checklist

### Character Animator States
- [ ] **Idle** — Stationary, breathing animation
- [ ] **Walk** — Forward movement, arm swing
- [ ] **Run** — Faster movement, full body motion
- [ ] **Attack0-3** — Per-ability attack animations (4 total)
- [ ] **Cast** — Charge/cast pose with VFX sync
- [ ] **Hurt** — Knockback/stagger reaction (0.3s)
- [ ] **Death** — Fall/collapse animation (plays once)
- [ ] **Jump** — Upward flight animation (if platforming)
- [ ] **Fall** — Descending animation

### Animation Events Needed
- [ ] Footsteps: Call `PlayFootstep()` at foot-strike frames
- [ ] Attack hit: Call `OnAttackHit()` at impact frame
- [ ] Cast release: Call `OnCastRelease()` when spell fires
- [ ] Death end: Call `OnDeathEnd()` to disable character

---

## VFX Setup Checklist

### Required Particle Systems
- [ ] **Hit** — Generic impact (orange burst, 0.5s)
- [ ] **Cast_Fire** — Fire ability cast (orange flames, 1.0s)
- [ ] **Cast_Ice** — Ice ability cast (blue shards, 0.8s)
- [ ] **Cast_Lightning** — Lightning cast (electric arcs, 0.6s)
- [ ] **Death** — Character death (blood/energy dissipation, 2.0s)
- [ ] **LevelUp** — Level up celebration (confetti, sparkles, 3.0s)
- [ ] **StatusPoison** — Poison aura (green mist, looping)
- [ ] **StatusBurning** — Burning effect (fire, looping)
- [ ] **Environment_Dust** — Dust cloud (impact dust, 1.0s)
- [ ] **Environment_Debris** — Debris scatter (rocks, wood, 1.5s)

---

## Audio Setup Checklist

### Sound Effects Library
- [ ] **Combat SFX**
  - Player_Hit, Enemy_Hit, Crit_Hit
  - Fire_Cast, Ice_Cast, Lightning_Cast
  - Death, LevelUp
  - StatusApplied (variants per status)

- [ ] **Footsteps**
  - Footstep_Grass, Footstep_Stone, Footstep_Metal
  - Running_Grass (rushing sound)

- [ ] **UI Sounds**
  - UI_Click (button), UI_Open (menu open), UI_Close (menu close)
  - Item_Pickup, Notification, Error

- [ ] **Music**
  - MainTheme (looped)
  - BossTheme (looped)
  - MenuTheme (looped)

- [ ] **Ambience**
  - Forest_Ambience (wind, birds)
  - Desert_Ambience (wind, distant creatures)
  - Dungeon_Ambience (dripping, echo)

---

## Testing

### Graphics Tests (38 tests)
- Character animation state transitions
- VFX spawning and cleanup
- Damage number floating
- Status indicator display
- Projectile trajectory

Run: `Window → General → Test Runner → GraphicsAndAudioTests`

### Audio Tests (38 tests)
- AudioManager singleton pattern
- SFX playback and spatial audio
- Music fade-in/fade-out
- Category volume control
- Combat audio event triggering
- Footstep terrain variation
- UI sound effects

---

## Performance Optimization

### Audio Pooling
- Pre-allocate 20 SFX sources at startup
- Recycle sources after playback
- Limit simultaneous SFX to 32 (configurable)

### VFX Pooling
- Pre-instantiate 5 copies of each effect
- Recycle after lifespan expiration
- Use object pooling library if heavy use

### Animation Optimization
- Cache animator parameter hashes (done: SpeedHash, AttackHash, etc.)
- Use animator integer states instead of string lookups
- Disable animator on death to stop processing

---

## Next Steps

1. Create all animation clips in 3D modeling software (Blender/Maya)
2. Import into Unity and set up Animator state machine
3. Configure VFX definitions with particle system prefabs
4. Set up AudioManager with all sound definitions
5. Wire event hooks from combat/UI systems
6. Playtest animations and audio in context
7. Iterate on timing, feel, and balance

## Files Delivered

- `AnimationAndVFX.cs` (CharacterAnimator, VFXSpawner, UIEffectLayer, ProjectileTracer)
- `AudioManager.cs` (AudioManager, CombatAudioEvents, FootstepAudio, UISoundEffects)
- `GraphicsAndAudioTests.cs` (38 tests covering all systems)
