using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CombatDemo.Combat;
using CombatDemo.Network;

namespace CombatDemo.UI
{
    /// <summary>
    /// Displays combat stats in the Unity UI.
    /// Can be used for debugging or as an in-game HUD.
    /// </summary>
    public class StatsDisplayUI : MonoBehaviour
    {
        [Header("Text References")]
        [SerializeField] private TextMeshProUGUI hitRateText;
        [SerializeField] private TextMeshProUGUI attackCountText;
        [SerializeField] private TextMeshProUGUI dashCountText;
        [SerializeField] private TextMeshProUGUI punchCountText;
        [SerializeField] private TextMeshProUGUI kickCountText;
        [SerializeField] private TextMeshProUGUI hitsLandedText;
        [SerializeField] private TextMeshProUGUI hitsMissedText;
        [SerializeField] private TextMeshProUGUI sessionTimeText;
        
        [Header("Connection Status")]
        [SerializeField] private TextMeshProUGUI connectionStatusText;
        [SerializeField] private Image connectionIndicator;
        [SerializeField] private Color connectedColor = Color.green;
        [SerializeField] private Color disconnectedColor = Color.red;
        
        [Header("Visual Settings")]
        [SerializeField] private bool animateOnChange = true;
        [SerializeField] private float punchScale = 1.2f;
        [SerializeField] private float punchDuration = 0.2f;
        
        private WebSocketClient _webSocketClient;
        private CombatStatsData _lastStats;
        
        private void Start()
        {
            _webSocketClient = FindObjectOfType<WebSocketClient>();
            
            if (_webSocketClient != null)
            {
                _webSocketClient.OnConnected += HandleConnected;
                _webSocketClient.OnDisconnected += HandleDisconnected;
            }
            
            CombatEvents.OnStatsUpdated += UpdateDisplay;
            
            // Initialize display
            UpdateConnectionStatus(false);
            ClearDisplay();
        }
        
        private void OnDestroy()
        {
            if (_webSocketClient != null)
            {
                _webSocketClient.OnConnected -= HandleConnected;
                _webSocketClient.OnDisconnected -= HandleDisconnected;
            }
            
            CombatEvents.OnStatsUpdated -= UpdateDisplay;
        }
        
        private void HandleConnected()
        {
            UpdateConnectionStatus(true);
        }
        
        private void HandleDisconnected()
        {
            UpdateConnectionStatus(false);
        }
        
        private void UpdateConnectionStatus(bool connected)
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text = connected ? "Connected" : "Disconnected";
            }
            
            if (connectionIndicator != null)
            {
                connectionIndicator.color = connected ? connectedColor : disconnectedColor;
            }
        }
        
        private void UpdateDisplay(CombatStatsData stats)
        {
            // Hit Rate
            if (hitRateText != null)
            {
                hitRateText.text = $"{stats.hitRate:F1}%";
                
                if (animateOnChange && _lastStats != null && 
                    Mathf.Abs(stats.hitRate - _lastStats.hitRate) > 0.1f)
                {
                    AnimateText(hitRateText);
                }
            }
            
            // Attack Count
            if (attackCountText != null)
            {
                attackCountText.text = stats.totalAttacks.ToString();
                
                if (animateOnChange && _lastStats != null && 
                    stats.totalAttacks != _lastStats.totalAttacks)
                {
                    AnimateText(attackCountText);
                }
            }
            
            // Dash Count
            if (dashCountText != null)
            {
                dashCountText.text = stats.dashCount.ToString();
                
                if (animateOnChange && _lastStats != null && 
                    stats.dashCount != _lastStats.dashCount)
                {
                    AnimateText(dashCountText);
                }
            }
            
            // Punch Count
            if (punchCountText != null)
            {
                punchCountText.text = stats.punchCount.ToString();
            }
            
            // Kick Count
            if (kickCountText != null)
            {
                kickCountText.text = stats.kickCount.ToString();
            }
            
            // Hits Landed
            if (hitsLandedText != null)
            {
                hitsLandedText.text = stats.hitsLanded.ToString();
            }
            
            // Hits Missed
            if (hitsMissedText != null)
            {
                hitsMissedText.text = stats.hitsMissed.ToString();
            }
            
            // Session Time
            if (sessionTimeText != null)
            {
                int minutes = Mathf.FloorToInt(stats.sessionDuration / 60f);
                int seconds = Mathf.FloorToInt(stats.sessionDuration % 60f);
                sessionTimeText.text = $"{minutes:00}:{seconds:00}";
            }
            
            _lastStats = stats;
        }
        
        private void ClearDisplay()
        {
            if (hitRateText != null) hitRateText.text = "0%";
            if (attackCountText != null) attackCountText.text = "0";
            if (dashCountText != null) dashCountText.text = "0";
            if (punchCountText != null) punchCountText.text = "0";
            if (kickCountText != null) kickCountText.text = "0";
            if (hitsLandedText != null) hitsLandedText.text = "0";
            if (hitsMissedText != null) hitsMissedText.text = "0";
            if (sessionTimeText != null) sessionTimeText.text = "00:00";
        }
        
        private void AnimateText(TextMeshProUGUI text)
        {
            StartCoroutine(AnimateTextCoroutine(text));
        }
        
        private System.Collections.IEnumerator AnimateTextCoroutine(TextMeshProUGUI text)
        {
            Vector3 originalScale = text.transform.localScale;
            Vector3 targetScale = originalScale * punchScale;
            
            float elapsed = 0f;
            float halfDuration = punchDuration / 2f;
            
            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                text.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            elapsed = 0f;
            
            // Scale down
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                text.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            text.transform.localScale = originalScale;
        }
        
        /// <summary>
        /// Reset stats button handler
        /// </summary>
        public void OnResetStatsClicked()
        {
            if (CombatStatsTracker.Instance != null)
            {
                CombatStatsTracker.Instance.ResetStats();
            }
            
            ClearDisplay();
        }
    }
}
