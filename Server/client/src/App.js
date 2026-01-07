import React, { useState, useEffect, useCallback } from 'react';
import { io } from 'socket.io-client';
import { motion, AnimatePresence } from 'framer-motion';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, AreaChart, Area } from 'recharts';
import './App.css';

// Socket connection
const SOCKET_URL = process.env.REACT_APP_SOCKET_URL || 'http://localhost:5000';

function App() {
  const [socket, setSocket] = useState(null);
  const [isConnected, setIsConnected] = useState(false);
  const [stats, setStats] = useState({
    totalAttacks: 0,
    punchCount: 0,
    kickCount: 0,
    hitsLanded: 0,
    hitsMissed: 0,
    dashCount: 0,
    hitRate: 0,
    sessionDuration: 0
  });
  const [hitRateHistory, setHitRateHistory] = useState([]);
  const [recentEvents, setRecentEvents] = useState([]);

  // Connect to socket
  useEffect(() => {
    const newSocket = io(SOCKET_URL);
    
    newSocket.on('connect', () => {
      console.log('Connected to server');
      setIsConnected(true);
    });

    newSocket.on('disconnect', () => {
      console.log('Disconnected from server');
      setIsConnected(false);
    });

    newSocket.on('statsUpdate', (newStats) => {
      setStats(newStats);
      
      // Add to hit rate history
      setHitRateHistory(prev => {
        const newHistory = [...prev, {
          time: new Date().toLocaleTimeString(),
          hitRate: newStats.hitRate,
          attacks: newStats.totalAttacks
        }];
        return newHistory.slice(-30); // Keep last 30 data points
      });

      // Add to recent events
      setRecentEvents(prev => {
        const event = {
          id: Date.now(),
          type: 'stats',
          message: `Hit Rate: ${newStats.hitRate.toFixed(1)}%`,
          time: new Date().toLocaleTimeString()
        };
        return [event, ...prev].slice(0, 10);
      });
    });

    newSocket.on('gameEvent', (event) => {
      setRecentEvents(prev => {
        const newEvent = {
          id: Date.now(),
          type: event.eventType,
          message: event.data,
          time: new Date().toLocaleTimeString()
        };
        return [newEvent, ...prev].slice(0, 10);
      });
    });

    newSocket.on('statsReset', () => {
      setStats({
        totalAttacks: 0,
        punchCount: 0,
        kickCount: 0,
        hitsLanded: 0,
        hitsMissed: 0,
        dashCount: 0,
        hitRate: 0,
        sessionDuration: 0
      });
      setHitRateHistory([]);
      setRecentEvents([{
        id: Date.now(),
        type: 'system',
        message: 'Stats Reset',
        time: new Date().toLocaleTimeString()
      }]);
    });

    setSocket(newSocket);

    return () => {
      newSocket.close();
    };
  }, []);

  const handleReset = useCallback(() => {
    if (socket) {
      socket.emit('resetStats');
    }
  }, [socket]);

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  return (
    <div className="dashboard">
      {/* Header */}
      <header className="dashboard-header">
        <div className="logo">
          <span className="logo-icon">⚔️</span>
          <h1>COMBAT DASHBOARD</h1>
        </div>
        <div className="connection-status">
          <span className={`status-dot ${isConnected ? 'connected' : 'disconnected'}`}></span>
          <span>{isConnected ? 'Unity Connected' : 'Waiting for Unity...'}</span>
        </div>
      </header>

      <main className="dashboard-content">
        {/* Primary Stats Row */}
        <section className="stats-row primary">
          <StatCard
            title="HIT RATE"
            value={`${stats.hitRate.toFixed(1)}%`}
            icon="🎯"
            color="accent"
            subtitle={`${stats.hitsLanded} hits / ${stats.totalAttacks} attacks`}
            highlight
          />
          <StatCard
            title="TOTAL ATTACKS"
            value={stats.totalAttacks}
            icon="👊"
            color="primary"
            subtitle={`${stats.punchCount} punches, ${stats.kickCount} kicks`}
          />
          <StatCard
            title="DASH COUNT"
            value={stats.dashCount}
            icon="💨"
            color="secondary"
            subtitle="Space key presses"
          />
        </section>

        {/* Secondary Stats Row */}
        <section className="stats-row secondary">
          <StatCard
            title="PUNCHES"
            value={stats.punchCount}
            icon="🥊"
            color="punch"
            small
          />
          <StatCard
            title="KICKS"
            value={stats.kickCount}
            icon="🦶"
            color="kick"
            small
          />
          <StatCard
            title="HITS LANDED"
            value={stats.hitsLanded}
            icon="✅"
            color="success"
            small
          />
          <StatCard
            title="HITS MISSED"
            value={stats.hitsMissed}
            icon="❌"
            color="danger"
            small
          />
          <StatCard
            title="SESSION TIME"
            value={formatTime(stats.sessionDuration)}
            icon="⏱️"
            color="time"
            small
          />
        </section>

        {/* Charts Section */}
        <section className="charts-section">
          <div className="chart-card">
            <h3>📈 Hit Rate Over Time</h3>
            <ResponsiveContainer width="100%" height={250}>
              <AreaChart data={hitRateHistory}>
                <defs>
                  <linearGradient id="hitRateGradient" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#e94560" stopOpacity={0.8}/>
                    <stop offset="95%" stopColor="#e94560" stopOpacity={0.1}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#2a2a4a" />
                <XAxis dataKey="time" stroke="#666" tick={{fill: '#888'}} />
                <YAxis domain={[0, 100]} stroke="#666" tick={{fill: '#888'}} />
                <Tooltip 
                  contentStyle={{ 
                    background: '#1a1a2e', 
                    border: '1px solid #e94560',
                    borderRadius: '8px'
                  }}
                />
                <Area 
                  type="monotone" 
                  dataKey="hitRate" 
                  stroke="#e94560" 
                  fill="url(#hitRateGradient)"
                  strokeWidth={2}
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>

          <div className="chart-card">
            <h3>📊 Attack Breakdown</h3>
            <div className="attack-breakdown">
              <div className="breakdown-item">
                <div className="breakdown-label">Punches</div>
                <div className="breakdown-bar">
                  <motion.div 
                    className="breakdown-fill punch"
                    initial={{ width: 0 }}
                    animate={{ 
                      width: stats.totalAttacks > 0 
                        ? `${(stats.punchCount / stats.totalAttacks) * 100}%` 
                        : '0%' 
                    }}
                    transition={{ duration: 0.5 }}
                  />
                </div>
                <div className="breakdown-value">{stats.punchCount}</div>
              </div>
              <div className="breakdown-item">
                <div className="breakdown-label">Kicks</div>
                <div className="breakdown-bar">
                  <motion.div 
                    className="breakdown-fill kick"
                    initial={{ width: 0 }}
                    animate={{ 
                      width: stats.totalAttacks > 0 
                        ? `${(stats.kickCount / stats.totalAttacks) * 100}%` 
                        : '0%' 
                    }}
                    transition={{ duration: 0.5 }}
                  />
                </div>
                <div className="breakdown-value">{stats.kickCount}</div>
              </div>
              <div className="breakdown-item">
                <div className="breakdown-label">Hit/Miss Ratio</div>
                <div className="breakdown-bar dual">
                  <motion.div 
                    className="breakdown-fill success"
                    initial={{ width: 0 }}
                    animate={{ 
                      width: stats.totalAttacks > 0 
                        ? `${(stats.hitsLanded / stats.totalAttacks) * 100}%` 
                        : '0%' 
                    }}
                    transition={{ duration: 0.5 }}
                  />
                </div>
                <div className="breakdown-value">{stats.hitsLanded}/{stats.hitsMissed}</div>
              </div>
            </div>
          </div>
        </section>

        {/* Event Log */}
        <section className="event-log">
          <h3>📝 Recent Activity</h3>
          <div className="events-container">
            <AnimatePresence>
              {recentEvents.map((event) => (
                <motion.div
                  key={event.id}
                  className={`event-item ${event.type}`}
                  initial={{ opacity: 0, x: -20 }}
                  animate={{ opacity: 1, x: 0 }}
                  exit={{ opacity: 0, x: 20 }}
                  transition={{ duration: 0.3 }}
                >
                  <span className="event-time">{event.time}</span>
                  <span className="event-type">{event.type}</span>
                  <span className="event-message">{event.message}</span>
                </motion.div>
              ))}
            </AnimatePresence>
          </div>
        </section>

        {/* Controls */}
        <section className="controls">
          <button className="btn btn-reset" onClick={handleReset}>
            🔄 Reset Stats
          </button>
        </section>
      </main>

      {/* Footer */}
      <footer className="dashboard-footer">
        <p>Combat Demo Dashboard • Real-time Unity Stats Tracking</p>
      </footer>
    </div>
  );
}

// Stat Card Component
function StatCard({ title, value, icon, color, subtitle, highlight, small }) {
  return (
    <motion.div 
      className={`stat-card ${color} ${highlight ? 'highlight' : ''} ${small ? 'small' : ''}`}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      whileHover={{ scale: 1.02 }}
      transition={{ duration: 0.3 }}
    >
      <div className="stat-icon">{icon}</div>
      <div className="stat-content">
        <h4 className="stat-title">{title}</h4>
        <motion.div 
          className="stat-value"
          key={value}
          initial={{ scale: 1.2 }}
          animate={{ scale: 1 }}
          transition={{ duration: 0.2 }}
        >
          {value}
        </motion.div>
        {subtitle && <p className="stat-subtitle">{subtitle}</p>}
      </div>
    </motion.div>
  );
}

export default App;
