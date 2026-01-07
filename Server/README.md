# Combat Dashboard Server - Complete Guide

## 🚀 Super Quick Start (1 Step!)

### Windows - Automatic (Recommended):
**Double-click `START_DASHBOARD.bat`**

Everything happens automatically:
- ✅ Checks if Node.js is installed
- ✅ Installs server dependencies (first time only)
- ✅ Installs dashboard dependencies (first time only)
- ✅ Starts server → `http://localhost:5000`
- ✅ Starts dashboard → `http://localhost:3000`

**Done!** Both services running! The dashboard opens automatically in your browser.

---

### Manual Setup (Any OS):

**Step 1:** Install Node.js from https://nodejs.org (LTS version)

**Step 2:** Install and start everything:
```bash
# From the Server folder
npm install
cd client && npm install && cd ..
npm run dev:all
```

Server: `http://localhost:5000` | Dashboard: `http://localhost:3000` ✅

---

## 🎮 Using with Unity

1. **Start the dashboard** (double-click `START_DASHBOARD.bat`)
2. **Open Unity** and press Play
3. **Stats automatically send** to the server every 0.5 seconds
4. **Watch the dashboard** update in real-time!

Unity auto-connects to `http://localhost:5000` - no configuration needed!

---

## ⚙️ Configuration (Optional)

The server works out of the box! But if you want to customize:

Create `Server/.env` file:
```
PORT=5000
CLIENT_URL=http://localhost:3000
MONGODB_URI=mongodb://localhost:27017/combatdemo
NODE_ENV=development
```

**Note:** MongoDB is **optional** - the server works fine without it (uses in-memory storage)

You can copy `.env.example` to `.env` and modify as needed.

---

## 🔧 Troubleshooting

### ❌ "Node.js is not installed"
**Solution:**
1. Download Node.js from https://nodejs.org (get LTS version)
2. Install it
3. **Restart your computer** (important!)
4. Run `START_DASHBOARD.bat` again

### ❌ "npm: command not found"
- Install Node.js from https://nodejs.org
- Restart your terminal/computer after installing
- Verify: Open terminal and type `node --version`

### ❌ "Port 5000 already in use"
**Solution:**
1. Close other programs using port 5000
2. OR create `.env` file with: `PORT=5001`
3. Update Unity's GameManager → WebSocketClient → Server URL to `http://localhost:5001`

### ❌ "Failed to install dependencies"
**Solution:**
1. Make sure you have internet connection
2. Try running terminal as Administrator
3. Delete `node_modules` folder and `package-lock.json`
4. Run `START_DASHBOARD.bat` again

### ⚠️ Unity not connecting
**Check these:**
1. ✅ Server is running (you should see the fancy box in terminal)
2. ✅ Unity GameManager → WebSocketClient → Server URL = `http://localhost:5000`
3. ✅ Unity GameManager → WebSocketClient → Auto Connect = `true`
4. ✅ Unity console shows connection logs
5. ✅ No firewall blocking localhost

### ⚠️ Dashboard shows "Connecting..." forever
**Check these:**
1. ✅ Server is running on port 5000
2. ✅ Dashboard is running on port 3000
3. ✅ Unity is playing and sending stats
4. ✅ Open `http://localhost:5000/api/health` - should show `{"status":"ok"}`

### ⚠️ Stats not updating
**Solutions:**
- Play Unity and perform attacks (punch, kick)
- Check server terminal for incoming requests
- Open browser console (F12) for errors
- Verify Unity console has "Connected to server" message

---

## 📝 API Endpoints

- `GET /api/health` - Check if server is running
- `GET /api/stats` - Get current stats
- `POST /api/stats` - Send stats from Unity (auto-used by game)
- `POST /api/stats/reset` - Reset all stats
- `GET /api/stats/history` - Get stats history

---

## 🎯 What Does It Do?

1. **Receives stats** from your Unity game in real-time
2. **Stores stats** in memory (or MongoDB if you set it up)
3. **Broadcasts stats** to any connected dashboards via WebSocket
4. **Provides REST API** for querying stats

---

## 💡 Pro Tips

- **Run both server and Unity at once** for real-time stats
- **Open browser to** `http://localhost:3000` to see live dashboard
- **Check** `http://localhost:5000/api/health` to verify server is running
- **Server auto-reconnects** if Unity disconnects/reconnects
