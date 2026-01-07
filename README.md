# Genesis Test

A Unity project featuring a third-person combat system with Animancer integration.

## Features

- **Third-Person Combat System**: Punch combos, kicks, and dodge mechanics
- **Animancer Integration**: Smooth animation blending and transitions
- **Hit Detection**: Physics-based combat hit detection
- **Training Dummy**: Interactive dummy with hit reactions
- **Real-Time Stats Dashboard**: Web-based dashboard to track combat stats

## Requirements

**For Unity:**
- Animancer package
- Input System package
- Cinemachine package

**For Dashboard (Optional):**
- Node.js (download from https://nodejs.org)

## Quick Setup

### 🎮 Unity Project
1. Clone this repository
2. Open the project in Unity
3. Let Unity import all assets
4. Open the main scene in `Assets/Scenes/`
5. Press Play!

### 📊 Stats Dashboard (Optional)

**Super Simple - Just Double-Click:**
1. Double-click `Server/START_DASHBOARD.bat`
2. Everything installs and starts automatically!
3. Dashboard opens at `http://localhost:3000`

That's it! The script handles:
- ✅ Checking Node.js
- ✅ Installing all dependencies
- ✅ Starting server
- ✅ Starting dashboard
- ✅ Opening browser automatically

**Having issues?** See `Server/README.md` for detailed troubleshooting.

## Combat Controls

- **Punch**: Left Mouse Button / Gamepad Button
- **Kick**: Right Mouse Button / Gamepad Button
- **Dodge**: Space / Gamepad Button
- **Movement**: WASD / Left Stick

## Development

This project uses:
- **Git LFS** for large binary files (models, textures, audio)
- **Animancer** for procedural animation control
- **Unity Input System** for cross-platform input handling
