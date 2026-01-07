# Combat Demo - Unity + MERN Stack Real-Time Dashboard

A complete combat training system with real-time statistics tracking. Player actions in Unity are tracked and displayed on a live web dashboard.

## 🎮 Features

### Unity Side
- **Player Controller**: WASD movement with camera-relative directions
- **Combat System**: Punch (Left Mouse), Kick (Right Mouse), Dash (Space)
- **Training Dummy**: Hit reactions with pushback and visual feedback
- **Modular Architecture**: Event-driven, easily extendable

### Dashboard Side
- **Real-Time Stats**: Hit rate, attack count, dash count
- **Live Charts**: Hit rate over time with area chart
- **Attack Breakdown**: Visual punch/kick/hit ratio bars
- **Event Log**: Real-time activity feed
- **Socket.IO**: Instant updates without page refresh

---

## 📁 Project Structure

```
CombatDemo/
├── Scripts/                    # Unity C# Scripts
│   ├── Combat/
│   │   ├── CombatEvents.cs     # Event system
│   │   └── CombatStatsTracker.cs
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── PlayerCombat.cs
│   │   └── PlayerAnimator.cs
│   ├── Dummy/
│   │   └── DummyController.cs
│   ├── Network/
│   │   └── WebSocketClient.cs
│   ├── UI/
│   │   └── StatsDisplayUI.cs
│   └── GameManager.cs
├── InputActions/
│   └── CombatInputActions.inputactions
├── Animations/                 # Place Mixamo animations here
├── Materials/
├── Prefabs/
└── Server/                     # MERN Stack Backend
    ├── server.js
    ├── package.json
    ├── .env.example
    └── client/                 # React Dashboard
        ├── package.json
        ├── public/
        │   └── index.html
        └── src/
            ├── index.js
            ├── index.css
            ├── App.js
            └── App.css
```

---

## 🚀 Setup Instructions

### Prerequisites

- **Unity**: 2021.3 LTS or newer (with Input System package)
- **Node.js**: v18+ recommended
- **npm** or **yarn**
- **MongoDB** (optional - server works without it in memory mode)

---

## Part 1: Unity Setup

### Step 1: Copy Scripts to Unity

Copy all the scripts from the `Scripts/` folder to your Unity project's `Assets/CombatDemo/Scripts/` directory, maintaining the folder structure.

### Step 2: Install Required Packages

In Unity, go to **Window > Package Manager** and ensure these are installed:
- **Input System** (com.unity.inputsystem)
- **TextMeshPro** (com.unity.textmeshpro)

### Step 3: Download Character & Animations from Mixamo

1. Go to [Mixamo.com](https://www.mixamo.com/) (free Adobe account required)

2. **Download a Character** (recommended: "Y Bot" or "X Bot" for testing):
   - Select a character
   - Click "Download"
   - Format: **FBX for Unity**
   - Pose: **T-Pose**

3. **Download Animations** (search for these):

   | Animation | Search Term | Settings |
   |-----------|-------------|----------|
   | Idle | "idle" | Loop ✓ |
   | Run | "running" | Loop ✓ |
   | Punch | "punch" or "jab" | In Place ✓ |
   | Kick | "kick" | In Place ✓ |
   | Dash | "dodge" or "roll forward" | - |
   | Hit Reaction | "hit reaction" or "impact" | - |

4. Import all FBX files into `Assets/CombatDemo/Animations/`

### Step 4: Configure Animation Import Settings

For each animation FBX:
1. Select the FBX in Project window
2. In Inspector, go to **Rig** tab
3. Set **Animation Type**: Humanoid
4. Click **Apply**
5. Go to **Animation** tab
6. Check **Loop Time** for Idle and Run
7. Click **Apply**

### Step 5: Create Animator Controller

1. Right-click in `Assets/CombatDemo/Animations/`
2. Create > **Animator Controller**
3. Name it "PlayerAnimator"
4. Double-click to open Animator window
5. Set up states and transitions

**Parameters to create:**
- `Speed` (Float) - For blend tree
- `Punch` (Trigger)
- `Kick` (Trigger)
- `Dash` (Trigger)
- `Hit` (Trigger)

**State Setup:**
- Idle → Run (Speed > 0.1)
- Run → Idle (Speed < 0.1)
- Any State → Punch (Punch trigger)
- Any State → Kick (Kick trigger)
- Any State → Dash (Dash trigger)
- Punch/Kick/Dash → Exit (auto)

### Step 6: Import Input Actions

1. Copy `CombatInputActions.inputactions` to `Assets/CombatDemo/InputActions/`
2. Select it in Unity
3. In Inspector, click **Generate C# Class**
4. Click **Apply**

### Step 7: Create Player Prefab

1. Drag your Mixamo character into the scene
2. Add Components:
   - **Character Controller** (Height: 1.8, Radius: 0.3, Center Y: 0.9)
   - **Player Input** (Actions: CombatInputActions, Behavior: Invoke Unity Events)
   - **PlayerController.cs**
   - **PlayerCombat.cs**
   - **PlayerAnimator.cs**
   - **Animator** (Controller: PlayerAnimator)

3. Configure **Player Input** component:
   - Under Events > Player:
     - Move → PlayerController.OnMove
     - Punch → PlayerController.OnPunch
     - Kick → PlayerController.OnKick
     - Dash → PlayerController.OnDash

4. Configure **PlayerCombat** component:
   - Target Layers: Create a "Dummy" layer and select it
   - Attack Range: 1.5
   - Attack Radius: 0.5

5. Drag to `Assets/CombatDemo/Prefabs/` to create prefab

### Step 8: Create Dummy Prefab

1. Duplicate the same character or use a different one
2. Add Components:
   - **Character Controller**
   - **DummyController.cs**
   - **Animator** (with hit reaction animations)

3. Set Layer to "Dummy"
4. Configure **DummyController**:
   - Pushback Recovery Speed: 5
   - Hit Stun Duration: 0.5

5. Create prefab

### Step 9: Set Up Animation Events

For **Punch** and **Kick** animations:
1. Select the animation clip
2. In Animation window, add events:
   - At impact frame (~50%): Call `OnAttackHitFrame()`
   - At end frame: Call `OnAttackEnd()`

### Step 10: Scene Setup

1. Create an empty scene or use existing
2. Create **GameManager** empty GameObject:
   - Add **GameManager.cs**
   - Add **CombatStatsTracker.cs**
   - Add **WebSocketClient.cs**

3. Create spawn points (empty GameObjects):
   - PlayerSpawnPoint at (0, 0, 0)
   - DummySpawnPoint at (0, 0, 3) facing player

4. Assign references in GameManager:
   - Player Prefab
   - Dummy Prefab
   - Spawn Points

5. Create a ground plane (Scale: 10, 1, 10)

### Step 11: Configure WebSocketClient

In the WebSocketClient component:
- Server URL: `http://localhost:5000`
- Auto Connect: ✓

---

## Part 2: Backend Setup

### Step 1: Navigate to Server Directory

```bash
cd CombatDemo/Server
```

### Step 2: Install Dependencies

```bash
# Install server dependencies
npm install

# Install client dependencies
cd client
npm install
cd ..
```

### Step 3: Configure Environment (Optional)

```bash
# Copy example env
cp .env.example .env

# Edit if needed (MongoDB is optional)
```

### Step 4: Start the Server

```bash
# Development mode (auto-restart on changes)
npm run dev

# Or standard mode
npm start
```

You should see:
```
╔═══════════════════════════════════════════════════════╗
║     🎮 Combat Dashboard Server Running!                ║
╠═══════════════════════════════════════════════════════╣
║  Server:     http://localhost:5000                     ║
║  Health:     http://localhost:5000/api/health          ║
║  Stats API:  http://localhost:5000/api/stats           ║
╚═══════════════════════════════════════════════════════╝
```

### Step 5: Start the Dashboard

In a new terminal:

```bash
cd CombatDemo/Server/client
npm start
```

Dashboard opens at: **http://localhost:3000**

---

## Part 3: Running the Demo

### Quick Start

1. **Start the backend server** (Terminal 1):
   ```bash
   cd CombatDemo/Server
   npm run dev
   ```

2. **Start the dashboard** (Terminal 2):
   ```bash
   cd CombatDemo/Server/client
   npm start
   ```

3. **Run Unity** - Press Play in the Editor

4. **Open Dashboard** at http://localhost:3000

### Controls

| Input | Action |
|-------|--------|
| W/A/S/D | Move |
| Left Mouse | Punch |
| Right Mouse | Kick |
| Space | Dash |

---

## 🏗️ Architecture

### Event Flow

```
Unity                          Server                    Dashboard
─────                          ──────                    ─────────
Player attacks
     │
     ▼
CombatEvents (C# events)
     │
     ▼
CombatStatsTracker
     │
     ▼
WebSocketClient ──HTTP POST──▶ Express.js ──Socket.IO──▶ React App
                               (port 5000)              (port 3000)
```

### Modular Design

Each component is independent and communicates via events:

- **PlayerController**: Only handles movement
- **PlayerCombat**: Only handles attacks
- **PlayerAnimator**: Only handles animations
- **CombatEvents**: Central event bus
- **CombatStatsTracker**: Only tracks stats
- **WebSocketClient**: Only handles networking

---

## 📊 API Reference

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/health` | Server health check |
| GET | `/api/stats` | Get current stats |
| POST | `/api/stats` | Submit stats from Unity |
| GET | `/api/stats/history` | Get stats history |
| POST | `/api/stats/reset` | Reset all stats |
| POST | `/api/events` | Submit custom events |

### Socket Events

| Event | Direction | Description |
|-------|-----------|-------------|
| `statsUpdate` | Server → Client | New stats received |
| `statsReset` | Server → Client | Stats were reset |
| `gameEvent` | Server → Client | Custom game event |
| `requestStats` | Client → Server | Request stats refresh |
| `resetStats` | Client → Server | Request stats reset |

---

## 🔧 Troubleshooting

### Unity Can't Connect to Server

1. Check server is running on port 5000
2. Check firewall settings
3. Verify WebSocketClient.serverUrl is `http://localhost:5000`
4. Check Unity Console for errors

### Animations Not Playing

1. Verify Animator Controller is assigned
2. Check parameter names match: Speed, Punch, Kick, Dash, Hit
3. Ensure animation events are set up correctly
4. Check Humanoid avatar configuration

### Stats Not Updating

1. Check CombatStatsTracker is in scene
2. Verify CombatEvents are being raised (add debug logs)
3. Check browser console for Socket.IO errors
4. Ensure WebSocketClient shows "Connected" in Unity

### MongoDB Connection Failed

The server works without MongoDB (memory mode). To use MongoDB:
1. Install MongoDB locally or use MongoDB Atlas
2. Start MongoDB service
3. Set MONGODB_URI in .env file

---

## 🚀 Extending the System

### Adding New Attack Types

1. Add to `AttackType` enum in `CombatEvents.cs`:
   ```csharp
   public enum AttackType { Punch, Kick, UpperCut }
   ```

2. Add input binding in `CombatInputActions.inputactions`

3. Add handler in `PlayerController.cs`

4. Add animation trigger in `PlayerAnimator.cs`

5. Update `CombatStatsData` and dashboard

### Adding New Metrics

1. Add field to `CombatStatsData` class:
   ```csharp
   public int comboCount;
   ```

2. Track in `CombatStatsTracker.cs`

3. Add to server schema in `server.js`

4. Add UI component in `App.js`

### Adding Visual Effects

1. Create VFX prefabs (particles, trails)
2. Reference in `PlayerCombat` or `DummyController`
3. Instantiate on hit events

---

## 📝 Checklist

### Unity Setup
- [ ] Scripts copied to project
- [ ] Input System package installed
- [ ] Character imported from Mixamo
- [ ] Animations imported and configured
- [ ] Animator Controller created with parameters
- [ ] Animation events added to attack clips
- [ ] Player prefab created with all components
- [ ] Dummy prefab created with DummyController
- [ ] Dummy layer created and assigned
- [ ] GameManager set up with references
- [ ] WebSocketClient configured

### Backend Setup
- [ ] Node.js installed
- [ ] Server dependencies installed (`npm install`)
- [ ] Client dependencies installed
- [ ] Server running on port 5000
- [ ] Dashboard running on port 3000

### Testing
- [ ] Unity connects to server (check console)
- [ ] Dashboard shows "Unity Connected"
- [ ] Stats update when attacking
- [ ] Charts display data
- [ ] Reset button works

---

## 🎨 Recommended Mixamo Assets

### Characters
- **Y Bot** - Great for testing, clean design
- **X Bot** - Alternative to Y Bot
- **Paladin** - More detailed, fantasy style

### Animations
- "Standing Idle" - base idle
- "Running" - locomotion
- "Jab Cross" or "Boxing" - punch
- "Roundhouse Kick" or "Mma Kick" - kick
- "Standing Dodge Left/Right" - dash
- "Standing React Small From Front" - hit reaction

---

## License

MIT License - Feel free to use and modify for your projects.

---

## Support

If you encounter issues:
1. Check the troubleshooting section
2. Verify all components are properly connected
3. Check Unity and browser consoles for errors
4. Ensure all prerequisites are installed
