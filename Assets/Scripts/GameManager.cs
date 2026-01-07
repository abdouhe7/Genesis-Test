using UnityEngine;
using CombatDemo.Combat;
using CombatDemo.Network;

namespace CombatDemo
{
    /// <summary>
    /// Main game manager for the combat demo.
    /// Handles initialization, scene setup, and game state.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Scene References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject dummyPrefab;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform dummySpawnPoint;
        
        [Header("Camera Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 5f, -8f);
        [SerializeField] private float cameraFollowSpeed = 5f;
        
        [Header("Network")]
        [SerializeField] private bool connectOnStart = true;
        
        [Header("Debug")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool showDebugUI = true;
        
        private GameObject _player;
        private GameObject _dummy;
        private WebSocketClient _webSocketClient;
        private CombatStatsTracker _statsTracker;
        
        public GameObject Player => _player;
        public GameObject Dummy => _dummy;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            InitializeComponents();
        }
        
        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnPlayer();
                SpawnDummy();
            }
            
            if (connectOnStart && _webSocketClient != null)
            {
                _webSocketClient.Connect();
            }
        }
        
        private void LateUpdate()
        {
            UpdateCamera();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                CombatEvents.ClearAll();
                Instance = null;
            }
        }
        
        private void InitializeComponents()
        {
            // Find or create WebSocketClient
            _webSocketClient = FindObjectOfType<WebSocketClient>();
            if (_webSocketClient == null)
            {
                GameObject networkObj = new GameObject("NetworkManager");
                networkObj.transform.SetParent(transform);
                _webSocketClient = networkObj.AddComponent<WebSocketClient>();
            }
            
            // Find or create StatsTracker
            _statsTracker = FindObjectOfType<CombatStatsTracker>();
            if (_statsTracker == null)
            {
                GameObject statsObj = new GameObject("StatsTracker");
                statsObj.transform.SetParent(transform);
                _statsTracker = statsObj.AddComponent<CombatStatsTracker>();
            }
            
            // Setup camera
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
        
        private void SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogWarning("[GameManager] Player prefab not assigned!");
                return;
            }
            
            Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            Quaternion spawnRot = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;
            
            _player = Instantiate(playerPrefab, spawnPos, spawnRot);
            _player.name = "Player";
        }
        
        private void SpawnDummy()
        {
            if (dummyPrefab == null)
            {
                Debug.LogWarning("[GameManager] Dummy prefab not assigned!");
                return;
            }
            
            Vector3 spawnPos = dummySpawnPoint != null ? dummySpawnPoint.position : new Vector3(0f, 0f, 3f);
            Quaternion spawnRot = dummySpawnPoint != null ? dummySpawnPoint.rotation : Quaternion.identity;
            
            _dummy = Instantiate(dummyPrefab, spawnPos, spawnRot);
            _dummy.name = "TrainingDummy";
        }
        
        private void UpdateCamera()
        {
            if (mainCamera == null || _player == null) return;
            
            Vector3 targetPosition = _player.transform.position + cameraOffset;
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, 
                targetPosition, 
                cameraFollowSpeed * Time.deltaTime
            );
            
            mainCamera.transform.LookAt(_player.transform.position + Vector3.up);
        }
        
        /// <summary>
        /// Reset the game state
        /// </summary>
        public void ResetGame()
        {
            // Reset stats
            if (_statsTracker != null)
            {
                _statsTracker.ResetStats();
            }
            
            // Reset player position
            if (_player != null && playerSpawnPoint != null)
            {
                _player.transform.position = playerSpawnPoint.position;
                _player.transform.rotation = playerSpawnPoint.rotation;
            }
            
            // Reset dummy
            Dummy.DummyController dummyController = _dummy?.GetComponent<Dummy.DummyController>();
            if (dummyController != null)
            {
                dummyController.ResetDummy();
            }
        }
        
        /// <summary>
        /// Pause/Resume the game
        /// </summary>
        public void SetPaused(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw spawn points
            if (playerSpawnPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(playerSpawnPoint.position, 0.5f);
                Gizmos.DrawLine(playerSpawnPoint.position, playerSpawnPoint.position + playerSpawnPoint.forward);
            }
            
            if (dummySpawnPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(dummySpawnPoint.position, 0.5f);
                Gizmos.DrawLine(dummySpawnPoint.position, dummySpawnPoint.position + dummySpawnPoint.forward);
            }
        }
        #endif
    }
}
