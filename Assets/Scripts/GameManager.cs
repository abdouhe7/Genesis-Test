using UnityEngine;
using CombatDemo.Combat;
using CombatDemo.Network;

namespace CombatDemo
{
    /// <summary>
    /// Simplified game manager for the combat demo.
    /// Finds and manages existing player and dummy in the scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene References (Optional - will auto-find if not assigned)")]
        [SerializeField] private GameObject player;
        [SerializeField] private GameObject dummy;

        [Header("Network")]
        [SerializeField] private bool connectOnStart = true;

        private WebSocketClient _webSocketClient;
        private CombatStatsTracker _statsTracker;

        public GameObject Player => player;
        public GameObject Dummy => dummy;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InitializeComponents();
            FindSceneObjects();
        }

        private void Start()
        {
            if (connectOnStart && _webSocketClient != null)
            {
                _webSocketClient.Connect();
            }
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
        }

        private void FindSceneObjects()
        {
            // Find player if not manually assigned
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    // Fallback: find by component
                    var playerCombat = FindObjectOfType<CombatSystem.ThirdPersonCombatBridge>();
                    if (playerCombat != null)
                    {
                        player = playerCombat.gameObject;
                        Debug.Log("[GameManager] Found player by component: " + player.name);
                    }
                    else
                    {
                        Debug.LogWarning("[GameManager] Player not found in scene! Make sure player has 'Player' tag or ThirdPersonCombatBridge component.");
                    }
                }
                else
                {
                    Debug.Log("[GameManager] Found player by tag: " + player.name);
                }
            }

            // Find dummy if not manually assigned
            if (dummy == null)
            {
                dummy = GameObject.FindGameObjectWithTag("Dummy");
                if (dummy == null)
                {
                    // Fallback: find by component
                    var dummyController = FindObjectOfType<Dummy.DummyController>();
                    if (dummyController == null)
                    {
                        var dummyHit = FindObjectOfType<CombatSystem.DummyHitReaction>();
                        if (dummyHit != null)
                        {
                            dummy = dummyHit.gameObject;
                            Debug.Log("[GameManager] Found dummy by component: " + dummy.name);
                        }
                    }
                    else
                    {
                        dummy = dummyController.gameObject;
                        Debug.Log("[GameManager] Found dummy by component: " + dummy.name);
                    }

                    if (dummy == null)
                    {
                        Debug.LogWarning("[GameManager] Dummy not found in scene! Make sure dummy has 'Dummy' tag or DummyController/DummyHitReaction component.");
                    }
                }
                else
                {
                    Debug.Log("[GameManager] Found dummy by tag: " + dummy.name);
                }
            }
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

            // Reset dummy
            var dummyController = dummy?.GetComponent<Dummy.DummyController>();
            if (dummyController != null)
            {
                dummyController.ResetDummy();
            }
            else
            {
                var dummyHit = dummy?.GetComponent<CombatSystem.DummyHitReaction>();
                if (dummyHit != null)
                {
                    dummyHit.ResetDummy();
                }
            }
        }
        
        /// <summary>
        /// Pause/Resume the game
        /// </summary>
        public void SetPaused(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
        }
        
    }
}
