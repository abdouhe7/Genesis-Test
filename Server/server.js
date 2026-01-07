const express = require('express');
const http = require('http');
const { Server } = require('socket.io');
const cors = require('cors');
const mongoose = require('mongoose');
const path = require('path');
require('dotenv').config();

const app = express();
const server = http.createServer(app);

// Socket.IO setup with CORS
const io = new Server(server, {
    cors: {
        origin: process.env.CLIENT_URL || "http://localhost:3000",
        methods: ["GET", "POST"]
    }
});

// Middleware
app.use(cors());
app.use(express.json());

// MongoDB connection (optional - can run without it for testing)
const MONGODB_URI = process.env.MONGODB_URI || 'mongodb://localhost:27017/combatdemo';
let dbConnected = false;

mongoose.connect(MONGODB_URI)
    .then(() => {
        console.log('✅ Connected to MongoDB');
        dbConnected = true;
    })
    .catch((err) => {
        console.log('⚠️  MongoDB connection failed - running in memory mode');
        console.log('   To use MongoDB, make sure it\'s running or set MONGODB_URI');
    });

// In-memory stats storage (fallback when MongoDB is not available)
let currentStats = {
    totalAttacks: 0,
    punchCount: 0,
    kickCount: 0,
    hitsLanded: 0,
    hitsMissed: 0,
    dashCount: 0,
    hitRate: 0,
    sessionDuration: 0,
    timestamp: new Date().toISOString()
};

let statsHistory = [];
const MAX_HISTORY = 1000;

// Stats Schema (for MongoDB)
const statsSchema = new mongoose.Schema({
    totalAttacks: { type: Number, default: 0 },
    punchCount: { type: Number, default: 0 },
    kickCount: { type: Number, default: 0 },
    hitsLanded: { type: Number, default: 0 },
    hitsMissed: { type: Number, default: 0 },
    dashCount: { type: Number, default: 0 },
    hitRate: { type: Number, default: 0 },
    sessionDuration: { type: Number, default: 0 },
    timestamp: { type: Date, default: Date.now }
});

const Stats = mongoose.model('Stats', statsSchema);

// Event Schema
const eventSchema = new mongoose.Schema({
    eventType: String,
    data: mongoose.Schema.Types.Mixed,
    timestamp: { type: Date, default: Date.now }
});

const Event = mongoose.model('Event', eventSchema);

// ============= API Routes =============

// Health check endpoint
app.get('/api/health', (req, res) => {
    res.json({ 
        status: 'ok',
        mongodb: dbConnected ? 'connected' : 'disconnected',
        timestamp: new Date().toISOString()
    });
});

// POST: Receive stats from Unity
app.post('/api/stats', async (req, res) => {
    try {
        const stats = req.body;
        
        // Update current stats
        currentStats = {
            ...stats,
            timestamp: new Date().toISOString()
        };
        
        // Add to history
        statsHistory.push({ ...currentStats });
        if (statsHistory.length > MAX_HISTORY) {
            statsHistory.shift();
        }
        
        // Save to MongoDB if connected
        if (dbConnected) {
            const statsDoc = new Stats(stats);
            await statsDoc.save();
        }
        
        // Broadcast to all connected dashboard clients
        io.emit('statsUpdate', currentStats);
        
        res.json({ success: true, message: 'Stats received' });
    } catch (error) {
        console.error('Error saving stats:', error);
        res.status(500).json({ success: false, error: error.message });
    }
});

// GET: Retrieve current stats
app.get('/api/stats', (req, res) => {
    res.json(currentStats);
});

// GET: Retrieve stats history
app.get('/api/stats/history', async (req, res) => {
    try {
        const limit = parseInt(req.query.limit) || 100;
        
        if (dbConnected) {
            const history = await Stats.find()
                .sort({ timestamp: -1 })
                .limit(limit);
            res.json(history);
        } else {
            res.json(statsHistory.slice(-limit));
        }
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// POST: Receive custom events from Unity
app.post('/api/events', async (req, res) => {
    try {
        const event = req.body;
        
        // Broadcast to dashboard
        io.emit('gameEvent', event);
        
        // Save to MongoDB if connected
        if (dbConnected) {
            const eventDoc = new Event(event);
            await eventDoc.save();
        }
        
        res.json({ success: true });
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// POST: Reset stats
app.post('/api/stats/reset', async (req, res) => {
    try {
        currentStats = {
            totalAttacks: 0,
            punchCount: 0,
            kickCount: 0,
            hitsLanded: 0,
            hitsMissed: 0,
            dashCount: 0,
            hitRate: 0,
            sessionDuration: 0,
            timestamp: new Date().toISOString()
        };
        
        statsHistory = [];
        
        io.emit('statsReset', currentStats);
        
        res.json({ success: true, message: 'Stats reset' });
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// ============= Socket.IO Events =============

io.on('connection', (socket) => {
    console.log(`📱 Dashboard client connected: ${socket.id}`);
    
    // Send current stats immediately on connection
    socket.emit('statsUpdate', currentStats);
    
    socket.on('disconnect', () => {
        console.log(`📴 Dashboard client disconnected: ${socket.id}`);
    });
    
    // Allow dashboard to request stats refresh
    socket.on('requestStats', () => {
        socket.emit('statsUpdate', currentStats);
    });
    
    // Allow dashboard to reset stats
    socket.on('resetStats', () => {
        currentStats = {
            totalAttacks: 0,
            punchCount: 0,
            kickCount: 0,
            hitsLanded: 0,
            hitsMissed: 0,
            dashCount: 0,
            hitRate: 0,
            sessionDuration: 0,
            timestamp: new Date().toISOString()
        };
        io.emit('statsReset', currentStats);
    });
});

// Serve static files in production
if (process.env.NODE_ENV === 'production') {
    app.use(express.static(path.join(__dirname, 'client/build')));
    
    app.get('*', (req, res) => {
        res.sendFile(path.join(__dirname, 'client/build', 'index.html'));
    });
}

// Start server
const PORT = process.env.PORT || 5000;
server.listen(PORT, () => {
    console.log(`
╔═══════════════════════════════════════════════════════╗
║     🎮 Combat Dashboard Server Running!                ║
╠═══════════════════════════════════════════════════════╣
║  Server:     http://localhost:${PORT}                     ║
║  Health:     http://localhost:${PORT}/api/health          ║
║  Stats API:  http://localhost:${PORT}/api/stats           ║
║  MongoDB:    ${dbConnected ? '✅ Connected' : '⚠️  Not connected (memory mode)'}             ║
╚═══════════════════════════════════════════════════════╝
    `);
});

module.exports = { app, io };
