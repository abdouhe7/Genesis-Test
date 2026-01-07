using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using CombatDemo.Combat;

namespace CombatDemo.Network
{
    /// <summary>
    /// WebSocket client for real-time communication with the MERN dashboard.
    /// Uses Unity's native networking for compatibility.
    /// Falls back to HTTP polling if WebSocket is not available.
    /// </summary>
    public class WebSocketClient : MonoBehaviour
    {
        [Header("Server Configuration")]
        [SerializeField] private string serverUrl = "http://localhost:5000";
        [SerializeField] private string wsEndpoint = "/api/stats";
        [SerializeField] private string httpEndpoint = "/api/stats";
        
        [Header("Connection Settings")]
        [SerializeField] private float reconnectInterval = 5f;
        [SerializeField] private float httpFallbackInterval = 1f;
        [SerializeField] private int maxReconnectAttempts = 5;
        [SerializeField] private bool autoConnect = true;
        
        [Header("Debug")]
        [SerializeField] private bool logMessages = false;
        
        // Connection state
        private bool _isConnected;
        private bool _useHttpFallback = true; // WebSocket requires external library
        private int _reconnectAttempts;
        private Coroutine _connectionCoroutine;
        
        // Events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;
        public event Action<string> OnMessageReceived;
        
        public bool IsConnected => _isConnected;
        public string ServerUrl => serverUrl;
        
        private void Start()
        {
            if (autoConnect)
            {
                Connect();
            }
        }
        
        private void OnDestroy()
        {
            Disconnect();
        }
        
        /// <summary>
        /// Connect to the server
        /// </summary>
        public void Connect()
        {
            if (_isConnected) return;
            
            if (_connectionCoroutine != null)
            {
                StopCoroutine(_connectionCoroutine);
            }
            
            _connectionCoroutine = StartCoroutine(ConnectCoroutine());
        }
        
        /// <summary>
        /// Disconnect from the server
        /// </summary>
        public void Disconnect()
        {
            if (_connectionCoroutine != null)
            {
                StopCoroutine(_connectionCoroutine);
                _connectionCoroutine = null;
            }
            
            _isConnected = false;
            OnDisconnected?.Invoke();
        }
        
        private IEnumerator ConnectCoroutine()
        {
            // Test server connectivity
            string testUrl = serverUrl + "/api/health";
            
            using (UnityWebRequest request = UnityWebRequest.Get(testUrl))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    _isConnected = true;
                    _reconnectAttempts = 0;
                    
                    if (logMessages)
                    {
                        Debug.Log($"[WebSocketClient] Connected to server: {serverUrl}");
                    }
                    
                    OnConnected?.Invoke();
                }
                else
                {
                    _reconnectAttempts++;
                    
                    if (logMessages)
                    {
                        Debug.LogWarning($"[WebSocketClient] Connection failed: {request.error}. Attempt {_reconnectAttempts}/{maxReconnectAttempts}");
                    }
                    
                    OnError?.Invoke(request.error);
                    
                    if (_reconnectAttempts < maxReconnectAttempts)
                    {
                        yield return new WaitForSeconds(reconnectInterval);
                        _connectionCoroutine = StartCoroutine(ConnectCoroutine());
                    }
                    else
                    {
                        Debug.LogError("[WebSocketClient] Max reconnection attempts reached. Please check if the server is running.");
                    }
                }
            }
        }
        
        /// <summary>
        /// Send combat stats to the server
        /// </summary>
        public void SendStats(CombatStatsData stats)
        {
            if (!_isConnected)
            {
                if (logMessages)
                {
                    Debug.LogWarning("[WebSocketClient] Cannot send stats - not connected");
                }
                return;
            }
            
            StartCoroutine(SendStatsCoroutine(stats));
        }
        
        private IEnumerator SendStatsCoroutine(CombatStatsData stats)
        {
            string url = serverUrl + httpEndpoint;
            string json = stats.ToJson();
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 5;
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (logMessages)
                    {
                        Debug.Log($"[WebSocketClient] Stats sent successfully");
                    }
                    
                    OnMessageReceived?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    if (logMessages)
                    {
                        Debug.LogWarning($"[WebSocketClient] Failed to send stats: {request.error}");
                    }
                    
                    // Check if server went down
                    if (request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        _isConnected = false;
                        OnDisconnected?.Invoke();
                        
                        // Try to reconnect
                        Connect();
                    }
                }
            }
        }
        
        /// <summary>
        /// Send a custom event to the server
        /// </summary>
        public void SendEvent(string eventType, string data)
        {
            if (!_isConnected) return;
            
            StartCoroutine(SendEventCoroutine(eventType, data));
        }
        
        private IEnumerator SendEventCoroutine(string eventType, string data)
        {
            string url = serverUrl + "/api/events";
            
            EventPayload payload = new EventPayload
            {
                eventType = eventType,
                data = data,
                timestamp = DateTime.UtcNow.ToString("o")
            };
            
            string json = JsonUtility.ToJson(payload);
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success && logMessages)
                {
                    Debug.LogWarning($"[WebSocketClient] Failed to send event: {request.error}");
                }
            }
        }
        
        /// <summary>
        /// Set server URL at runtime
        /// </summary>
        public void SetServerUrl(string url)
        {
            Disconnect();
            serverUrl = url;
            Connect();
        }
        
        [Serializable]
        private class EventPayload
        {
            public string eventType;
            public string data;
            public string timestamp;
        }
    }
}
