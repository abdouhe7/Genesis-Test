# First Time Setup - Combat Dashboard

## ⚡ Super Simple 2-Minute Setup

### Prerequisites
- ✅ Unity Project (You have this!)
- ⬜ Node.js (Need to install)

---

## Step-by-Step (First Time Only)

### 1️⃣ Install Node.js (2 minutes)

1. Go to: **https://nodejs.org**
2. Click the **big green button** (LTS version)
3. Download and install
4. **Restart your computer** (or at least your terminal)

**How to verify:** Open terminal/cmd and type:
```bash
node --version
```
You should see something like `v20.x.x` or `v18.x.x`

---

### 2️⃣ Install Server Dependencies (1 minute)

**Windows - Easy Way:**
1. Navigate to: `P:\Unity Projects\Genesis test\Assets\Server\`
2. **Double-click** `START_SERVER.bat`
3. It will auto-install everything and start!

**Manual Way (Any OS):**
```bash
cd "P:\Unity Projects\Genesis test\Assets\Server"
npm install
```

Wait for it to finish (shows "added XXX packages")

---

### 3️⃣ Start the Server

**Windows:**
- Double-click `START_SERVER.bat`

**Manual:**
```bash
npm start
```

You should see this:
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

---

### 4️⃣ Test It Works

Open your browser and go to:
```
http://localhost:5000/api/health
```

You should see:
```json
{
  "status": "ok",
  "mongodb": "disconnected",
  "timestamp": "2025-01-07T..."
}
```

✅ **Success!** Server is running!

---

### 5️⃣ Test with Unity

1. **Keep the server running** (don't close the terminal)
2. **Open Unity**
3. **Press Play**
4. **Perform some attacks** (punch, kick)
5. **Watch the server terminal** - you should see logs!

---

## 🎉 That's It!

From now on, you only need to:
1. Start server (double-click `START_SERVER.bat`)
2. Play Unity
3. Done!

---

## ❓ Common Questions

### Q: Do I need to install dependencies every time?
**A:** No! Only the first time. After that, just run `npm start`

### Q: Do I need MongoDB?
**A:** No! The server works perfectly without it. Stats are stored in memory.

### Q: Can I close the server?
**A:** Yes, press Ctrl+C in the terminal. Stats will be lost (unless you use MongoDB).

### Q: What if I restart my computer?
**A:** Just start the server again. Unity will auto-reconnect.

### Q: Can multiple Unity instances connect?
**A:** Yes! All instances will send stats to the same server.

---

## 📊 Optional: Visual Dashboard

Want to see stats in a nice web UI?

```bash
# One-time setup
cd "P:\Unity Projects\Genesis test\Assets\Server\client"
npm install

# Start dashboard
npm start
```

Dashboard opens at: `http://localhost:3000`

---

## 🔧 Troubleshooting

### "npm: command not found"
→ Node.js not installed or terminal not restarted
→ Solution: Install Node.js and restart terminal

### "EADDRINUSE: Port 5000 already in use"
→ Something else using port 5000
→ Solution: Close other apps or change port in `.env` file

### Unity console: "Failed to connect to server"
→ Server not running
→ Solution: Start the server first!

### Stats not updating
→ Check GameManager has "Connect On Start" enabled
→ Check Server URL is `http://localhost:5000`

---

## 🎯 Next Steps

1. ✅ Server is running
2. ✅ Unity connects automatically
3. ✅ Stats appear in terminal
4. 📊 Optional: Install dashboard for visual stats
5. 🎮 Start developing!

---

## 💡 Pro Tips

- **Leave server running** while working in Unity
- **Check stats anytime:** http://localhost:5000/api/stats
- **Reset stats:** Visit http://localhost:5000/api/stats/reset
- **Multiple developers?** One person runs server, everyone connects!
- **Want persistence?** Set up MongoDB (advanced, see docs)

---

## 📞 Need Help?

Check these files:
- `QUICK_START.txt` - Cheat sheet
- `README.md` - Server features
- `SETUP_DASHBOARD.md` - Complete guide

---

**You're all set! Happy developing! 🚀**
