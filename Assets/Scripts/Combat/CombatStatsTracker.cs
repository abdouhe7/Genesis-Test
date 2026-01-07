using UnityEngine;
using CombatDemo.Combat;
using CombatDemo.Network;
using CombatSystem; // For AttackType enum

namespace CombatDemo.Combat
{
    /// <summary>
    /// Tracks all combat statistics and broadcasts updates.
    /// Singleton pattern for easy access from anywhere.
    /// </summary>
    public class CombatStatsTracker : MonoBehaviour
    {
        public static CombatStatsTracker Instance { get; private set; }
        
        [Header("Settings")]
        [SerializeField] private float statsUpdateInterval = 0.5f;
        [SerializeField] private bool sendUpdatesOnEvents = true;
        [SerializeField] private bool sendPeriodicUpdates = true;
        
        [Header("Debug")]
        [SerializeField] private bool logStats = false;
        
        private CombatStatsData _stats;
        private float _sessionStartTime;
        private float _updateTimer;
        private WebSocketClient _webSocketClient;
        
        // Public accessors
        public CombatStatsData CurrentStats => _stats;
        public float SessionDuration => Time.time - _sessionStartTime;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            InitializeStats();
        }
        
        private void Start()
        {
            _webSocketClient = GetComponent<WebSocketClient>();
            if (_webSocketClient == null)
            {
                _webSocketClient = FindObjectOfType<WebSocketClient>();
            }
            
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                UnsubscribeFromEvents();
                Instance = null;
            }
        }
        
        private void Update()
        {
            if (sendPeriodicUpdates)
            {
                _updateTimer += Time.deltaTime;
                if (_updateTimer >= statsUpdateInterval)
                {
                    _updateTimer = 0f;
                    BroadcastStats();
                }
            }
        }
        
        private void InitializeStats()
        {
            _stats = new CombatStatsData();
            _sessionStartTime = Time.time;
        }
        
        private void SubscribeToEvents()
        {
            CombatEvents.OnAttackPerformed += HandleAttackPerformed;
            CombatEvents.OnAttackResult += HandleAttackResult;
            CombatEvents.OnDashPerformed += HandleDashPerformed;
            CombatEvents.OnHitLanded += HandleHitLanded;
            CombatEvents.OnHitMissed += HandleHitMissed;
        }
        
        private void UnsubscribeFromEvents()
        {
            CombatEvents.OnAttackPerformed -= HandleAttackPerformed;
            CombatEvents.OnAttackResult -= HandleAttackResult;
            CombatEvents.OnDashPerformed -= HandleDashPerformed;
            CombatEvents.OnHitLanded -= HandleHitLanded;
            CombatEvents.OnHitMissed -= HandleHitMissed;
        }
        
        private void HandleAttackPerformed(AttackType type)
        {
            _stats.totalAttacks++;
            
            switch (type)
            {
                case AttackType.Punch:
                    _stats.punchCount++;
                    break;
                case AttackType.Kick:
                    _stats.kickCount++;
                    break;
            }
            
            UpdateAndBroadcast();
        }
        
        private void HandleAttackResult(AttackType type, bool hitLanded)
        {
            // Stats are updated in HitLanded/HitMissed handlers
        }
        
        private void HandleDashPerformed()
        {
            _stats.dashCount++;
            UpdateAndBroadcast();
        }
        
        private void HandleHitLanded(AttackType type, Vector3 hitPoint)
        {
            _stats.hitsLanded++;
            UpdateAndBroadcast();
        }
        
        private void HandleHitMissed(AttackType type)
        {
            _stats.hitsMissed++;
            UpdateAndBroadcast();
        }
        
        private void UpdateAndBroadcast()
        {
            UpdateStats();
            
            if (sendUpdatesOnEvents)
            {
                BroadcastStats();
            }
        }
        
        private void UpdateStats()
        {
            _stats.sessionDuration = SessionDuration;
            _stats.CalculateHitRate();
            _stats.timestamp = System.DateTime.UtcNow.ToString("o");
        }
        
        private void BroadcastStats()
        {
            UpdateStats();
            
            // Raise event for UI and other listeners
            CombatEvents.RaiseStatsUpdated(_stats);
            
            // Send to web dashboard via WebSocket
            if (_webSocketClient != null && _webSocketClient.IsConnected)
            {
                _webSocketClient.SendStats(_stats);
            }
            
            if (logStats)
            {
                Debug.Log($"[CombatStats] Attacks: {_stats.totalAttacks}, Hits: {_stats.hitsLanded}, " +
                         $"Misses: {_stats.hitsMissed}, Hit Rate: {_stats.hitRate:F1}%, Dashes: {_stats.dashCount}");
            }
        }
        
        /// <summary>
        /// Reset all stats to zero
        /// </summary>
        public void ResetStats()
        {
            InitializeStats();
            BroadcastStats();
        }
        
        /// <summary>
        /// Get stats as JSON string
        /// </summary>
        public string GetStatsJson()
        {
            UpdateStats();
            return _stats.ToJson();
        }
        
        /// <summary>
        /// Force an immediate stats broadcast
        /// </summary>
        public void ForceBroadcast()
        {
            BroadcastStats();
        }
    }
}
