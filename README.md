# Genesis Test

A Unity project featuring a third-person combat system with Animancer integration.

## Requirements

**For Unity:**
- Animancer package
- Input System package
- Cinemachine package

**For Dashboard:**
- Make sure that you install Node.js (download from https://nodejs.org)


## Ready Build For Genesis To Test
1. [Dashboard] If you have the required environment For the dashboard  Run the file [Server/START_DASHBOARD.bat]

2. [The Game] The build Located at Folder [Build For Genesis] On root To Test With 

## Quick Setup

### 🎮 Unity Project
1. Clone this repository
2. Open the project in Unity
3. Let Unity import all assets
4. Open the main scene in `Assets/Scenes/`
5. Press Play!

### 📊 Stats Dashboard

**Super Simple - Just Double-Click:**
1. Double-click `Server/START_DASHBOARD.bat`
2. Everything installs and starts automatically!
3. Dashboard opens at `http://localhost:3000`

That's it! The script handles:
-  Checking Node.js
-  Installing all dependencies
-  Starting server
-  Starting dashboard
-  Opening browser automatically

**Having issues?** See `Server/README.md` for detailed troubleshooting.


## Development

This project uses:
- **Git LFS** for large binary files (models, textures, audio)
- **Animancer** for procedural animation control
- **Unity Input System** for cross-platform input handling
