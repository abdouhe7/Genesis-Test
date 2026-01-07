# Combat System with Animancer - Setup Guide

## Quick Install

1. **Extract the ZIP** into your Unity project's `Assets` folder
   - You should have: `Assets/CombatSystem/` with all scripts inside

2. **Wait for Unity to compile** (may take 30 seconds)

3. **Run the Setup Tool**: 
   - Go to Unity Menu: `Tools > Combat System > Setup Window`
   - Click "Create Upper Body Avatar Mask"
   - Click "Setup Dummy Tag & Layer"
   - Select your player character, click "Add Combat Components to Selected"

---

## Manual Setup

### Step 1: Add Components to Player

On your player character (the one with ThirdPersonController), add:
- `AnimancerComponent` (if not already present)
- `ThirdPersonCombatBridge`
- `ThirdPersonControllerSync`
- `CombatInputActions`

### Step 2: Assign Animations in ThirdPersonCombatBridge

**Locomotion Animations** (from StarterAssets):
| Field | Drag From |
|-------|-----------|
| Idle Clip | `Assets/StarterAssets/.../Stand--Idle.anim` |
| Walk Clip | `Assets/StarterAssets/.../Locomotion--Walk_N.anim` |
| Run Clip | `Assets/StarterAssets/.../Locomotion--Run_N.anim` |

**Combat Animations** (from your Animations folder):
| Field | Drag From |
|-------|-----------|
| Fighting Idle Clip | `Ch44_nonPBR@Bouncing Fight Idle` |
| Punch Jab Clip | `Ch44_nonPBR@Punching Gab` |
| Punch Cross Clip | `Ch44_nonPBR@Punching Cross` |
| Punch Combo Clip | `Ch44_nonPBR@Punch Combo` |
| Kick Clip | `Ch44_nonPBR@Mma Kick` |
| Dodge Forward | `Ch44_nonPBR@Standing Dodge Forward` |
| Dodge Backward | `Ch44_nonPBR@Standing Dodge Backward` |
| Dodge Left | `Ch44_nonPBR@Standing Dodge Left` |
| Dodge Right | `Ch44_nonPBR@Standing Dodge Right` |

### Step 3: Create Upper Body Avatar Mask

1. Right-click in Project: `Create > Avatar Mask`
2. Name it "UpperBodyMask"
3. In Inspector, expand "Humanoid"
4. **Disable**: Root, Left Leg, Right Leg, Left Foot IK, Right Foot IK
5. **Enable**: Body, Head, Left Arm, Right Arm, Left Hand IK, Right Hand IK
6. Drag this mask to `ThirdPersonCombatBridge > Upper Body Mask`

### Step 4: Setup Dummy

1. Add `DummyHitReaction` component to your training dummy
2. Tag the dummy as "Dummy"
3. Add the dummy to a "Dummy" layer
4. Set up hit layers on player's combat bridge

### Step 5: Add Input Actions

In your `StarterAssets.inputactions`:

Add these new actions to the "Player" action map:
- **Punch**: Left Mouse Button (or desired key)
- **Kick**: Right Mouse Button  
- **Dodge**: Space key

---

## Controls

| Input | Action |
|-------|--------|
| WASD | Move (existing) |
| Left Mouse | Punch (tap repeatedly for combo) |
| Right Mouse | Kick |
| Space | Dodge (direction based on movement) |

---

## How It Works

### Fighting Stance
- When player is within `fightingStanceDistance` of dummy
- Upper body blends to fighting idle (hands up)
- Lower body continues locomotion normally

### Punch Combo
1. First punch = Jab
2. Press again during combo window = Cross
3. Press again = Combo finisher
4. Keep pressing = Repeats combo

### Directional Dodge
- Space + W = Dodge forward
- Space + S = Dodge backward
- Space + A = Dodge left
- Space + D = Dodge right
- Space + W+D = Diagonal dodge (blended)

### Hit Detection
- Attacks check for targets in cone in front of player
- Targets need `IDamageable` interface
- Dummy has built-in hit reaction with pushback

---

## Script Overview

| Script | Purpose |
|--------|---------|
| `PlayerCombatAnimancer.cs` | Standalone combat controller (full replacement) |
| `ThirdPersonCombatBridge.cs` | Works WITH existing ThirdPersonController |
| `ThirdPersonControllerSync.cs` | Syncs movement data to combat system |
| `CombatInputActions.cs` | Handles combat input |
| `DummyHitReaction.cs` | Training dummy behavior |
| `CombatSystemSetup.cs` | Editor helper tool |

---

## Troubleshooting

**Animations not playing:**
- Check that AnimationClip fields are assigned
- Verify clips are from FBX, not the FBX itself
- Make sure character uses Humanoid rig

**Fighting stance not blending:**
- Assign the Upper Body Avatar Mask
- Check that targetDummy is assigned or tagged "Dummy"
- Verify distance settings

**Combo not working:**
- Adjust `comboInputWindow` (higher = more forgiving)
- Check console for errors

**Dodge not moving:**
- Ensure CharacterController is present
- Check `dodgeMovementSpeed` setting

**Hits not registering:**
- Set up "Dummy" layer
- Assign `targetLayer` in inspector
- Check `attackRange` and `attackAngle`
