# Combat Stats Dashboard - Complete Setup Guide

## 📦 What You Need

1. **Node.js** - Download from https://nodejs.org (get the LTS version)
2. **Your Unity Project** - Already set up!

That's it! No database required, no complex configuration.

---

## ⚡ Fastest Setup (Windows)

### Option 1: Double-Click Method

1. **Install Node.js** from https://nodejs.org
2. Navigate to: `Assets/Server/`
3. **Double-click** `START_SERVER.bat`
4. Done! Server is running!

### Option 2: Command Line Method

```bash
# Step 1: Go to server folder
cd "P:\Unity Projects\Genesis test\Assets\Server"

# Step 2: Install dependencies (first time only)
npm install

# Step 3: Start server
npm start
```

---

## ✅ Verify It's Working

After starting the server, you should see:
```
╔═══════════════════════════════════════════════════════╗
║     🎮 Combat Dashboard Server Running!                ║
╠═══════════════════════════════════════════════════════╣
║  Server:     http://localhost:5000                     ║
║  Health:     http://localhost:5000/api/health          ║
║  Stats API:  http://localhost:5000/api/stats           ║
║  MongoDB:    ⚠️  Not connected (memory mode)            ║
╚═══════════════════════════════════════════════════════╝
```

**Test it:** Open browser → `http://localhost:5000/api/health`

You should see: `{"status":"ok","mongodb":"disconnected","timestamp":"..."}`

---

## 🎮 Connect Unity to Server

Good news: **It's already configured!**

1. Open Unity
2. Find **GameManager** object in the scene
3. Check the **WebSocketClient** component:
   - Server URL: `http://localhost:5000` ✅
   - Auto Connect: `true` ✅

4. **Play the scene**
5. Watch the server console - you should see:
   ```
   📱 Unity client connected
   ```

---

## 📊 View Stats in Browser (Optional)

Want a visual dashboard? Follow these steps:

```bash
# 1. Go to client folder
cd "P:\Unity Projects\Genesis test\Assets\Server\client"

# 2. Install client dependencies (first time only)
npm install

# 3. Start the dashboard
npm start
```

Dashboard will open at: `http://localhost:3000`

**Live stats will appear** as you play the game!

---

## 🔄 Typical Workflow

### Daily Use:

1. **Start Server:**
   ```bash
   cd Assets/Server
   npm start
   ```
   OR double-click `START_SERVER.bat`

2. **Open Unity** and press Play

3. **Optional:** Start dashboard
   ```bash
   cd Assets/Server/client
   npm start
   ```

4. **Fight!** Stats appear in real-time!

### First Time Setup:

1. Install Node.js
2. `cd Assets/Server && npm install`
3. Done!

---

## 🛠️ Configuration (Advanced - Optional)

Create `Assets/Server/.env` file (optional):

```env
PORT=5000
CLIENT_URL=http://localhost:3000
```

**Don't need MongoDB?** Skip it! Server works great without it.

**Want MongoDB?** Add to `.env`:
```env
MONGODB_URI=mongodb://localhost:27017/combatdemo
```

---

## 🐛 Troubleshooting

### Problem: "npm: command not found"
**Solution:** Install Node.js from https://nodejs.org, then restart terminal

### Problem: "Port 5000 already in use"
**Solution:**
- Close other apps using port 5000
- OR change port in `.env`: `PORT=5001`
- Update Unity's Server URL to match

### Problem: Unity not connecting to server
**Check:**
1. ✅ Server is running (see the fancy box in terminal)
2. ✅ Unity's Server URL = `http://localhost:5000`
3. ✅ No firewall blocking localhost
4. ✅ Check Unity console for connection logs

### Problem: Dashboard shows "Connecting..."
**Check:**
1. ✅ Server is running on port 5000
2. ✅ Dashboard is running on port 3000
3. ✅ Unity is playing and sending stats

---

## 📡 How It Works

```
Unity Game (Port: Game)
    ↓ Sends stats via HTTP
Server (Port: 5000)
    ↓ Broadcasts via WebSocket
Dashboard (Port: 3000)
    ↓ Displays real-time stats
```

1. **Unity sends stats** to server every 0.5 seconds
2. **Server receives** and stores stats
3. **Server broadcasts** to all connected dashboards
4. **Dashboard updates** in real-time

---

## 🎯 What Can You Track?

- Total attacks (punches, kicks)
- Hits landed vs missed
- Hit accuracy percentage
- Dash count
- Session duration
- Custom events

---

## 💡 Tips

- **Leave server running** while developing
- **Server reconnects automatically** if Unity restarts
- **Check** `http://localhost:5000/api/stats` to see current stats
- **Multiple dashboards** can connect simultaneously
- **No database needed** for basic usage

---

## 🚀 Production Deployment (Future)

When ready to deploy:

```bash
# Build client
cd client
npm run build

# Set environment
export NODE_ENV=production

# Start server (serves both API and dashboard)
npm start
```

Deploy to: Heroku, Render, Railway, etc.

---

## 📝 Quick Reference

**Start server:**
```bash
cd Assets/Server && npm start
```

**Start dashboard:**
```bash
cd Assets/Server/client && npm start
```

**Check health:**
```
http://localhost:5000/api/health
```

**View stats:**
```
http://localhost:5000/api/stats
```

**Reset stats:**
```bash
curl -X POST http://localhost:5000/api/stats/reset
```

---

## ✨ You're All Set!

The dashboard is now ready to use. Just:
1. Start the server
2. Play Unity
3. See stats in real-time!

No complex setup, no database required, works out of the box! 🎉
