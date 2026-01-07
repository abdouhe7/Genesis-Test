using UnityEngine;
using Animancer;

namespace CombatSystem
{
    /// <summary>
    /// Training dummy that reacts to player hits.
    /// Plays hit reaction animation and gets pushed back.
    /// </summary>
    [RequireComponent(typeof(AnimancerComponent))]
    public class DummyHitReaction : MonoBehaviour, IDamageable
    {
        [Header("=== ANIMATIONS (Animancer) ===")]
        [SerializeField] private ClipTransition idle;
        [SerializeField] private ClipTransition hitReaction;
        
        [Header("=== HIT REACTION SETTINGS ===")]
        [SerializeField] private float pushbackForce = 3f;
        [SerializeField] private float pushbackRecoverySpeed = 5f;
        
        [Header("=== VISUAL FEEDBACK ===")]
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f, 1f);
        
        [Header("=== RETURN TO ORIGIN ===")]
        [SerializeField] private bool returnToOrigin = true;
        [SerializeField] private float returnSpeed = 2f;
        [SerializeField] private float returnDelay = 1f;
        
        private AnimancerComponent _animancer;
        private CharacterController _characterController;
        private Renderer[] _renderers;
        private Color[] _originalColors;
        private Material[] _materials;
        
        private Vector3 _originPosition;
        private Quaternion _originRotation;
        private Vector3 _currentPushback;
        private float _returnDelayTimer;
        private bool _isHitStunned;
        
        // Stats
        private int _totalHitsTaken;
        
        private void Awake()
        {
            _animancer = GetComponent<AnimancerComponent>();
            _characterController = GetComponent<CharacterController>();
            
            // Cache renderers and original colors
            _renderers = GetComponentsInChildren<Renderer>();
            _originalColors = new Color[_renderers.Length];
            _materials = new Material[_renderers.Length];
            
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i].material != null)
                {
                    _materials[i] = _renderers[i].material;
                    if (_materials[i].HasProperty("_Color"))
                    {
                        _originalColors[i] = _materials[i].color;
                    }
                    else if (_materials[i].HasProperty("_BaseColor"))
                    {
                        _originalColors[i] = _materials[i].GetColor("_BaseColor");
                    }
                }
            }
            
            // Store origin
            _originPosition = transform.position;
            _originRotation = transform.rotation;
        }
        
        private void Start()
        {
            // Play idle animation
            if (idle.IsValid())
            {
                _animancer.Play(idle);
            }
            
            // Ensure this has the Dummy tag for player detection
            if (!gameObject.CompareTag("Dummy"))
            {
                Debug.LogWarning($"[DummyHitReaction] Object {name} is not tagged 'Dummy'. Setting tag now.");
                gameObject.tag = "Dummy";
            }
        }
        
        private void Update()
        {
            UpdatePushback();
            UpdateReturnToOrigin();
        }
        
        public void TakeDamage(float damage, AttackType attackType, Vector3 pushDirection)
        {
            Debug.Log($"[DummyHitReaction] Took {damage} damage from {attackType}!");
            
            _totalHitsTaken++;
            _isHitStunned = true;
            _returnDelayTimer = returnDelay;
            
            // Calculate pushback based on attack type
            float pushMultiplier = attackType == AttackType.Kick ? 1.5f : 1f;
            _currentPushback = pushDirection.normalized * pushbackForce * pushMultiplier;
            
            // Play hit reaction animation
            PlayHitReaction();
            
            // Visual feedback
            StartCoroutine(FlashCoroutine());
        }
        
        private void PlayHitReaction()
        {
            if (!hitReaction.IsValid())
            {
                Debug.LogWarning("[DummyHitReaction] Hit Reaction clip is missing!");
                return;
            }
            
            var state = _animancer.Play(hitReaction);
            
            // When hit reaction ends, return to idle
            state.Events(this).OnEnd = () =>
            {
                _isHitStunned = false;
                if (idle.IsValid())
                {
                    _animancer.Play(idle);
                }
            };
        }
        
        private void UpdatePushback()
        {
            if (_currentPushback.magnitude < 0.01f) return;
            
            // Apply pushback movement
            if (_characterController != null)
            {
                _characterController.Move(_currentPushback * Time.deltaTime);
            }
            else
            {
                transform.position += _currentPushback * Time.deltaTime;
            }
            
            // Decay pushback
            _currentPushback = Vector3.Lerp(_currentPushback, Vector3.zero, pushbackRecoverySpeed * Time.deltaTime);
        }
        
        private void UpdateReturnToOrigin()
        {
            if (!returnToOrigin || _isHitStunned) return;
            
            // Wait for delay
            if (_returnDelayTimer > 0f)
            {
                _returnDelayTimer -= Time.deltaTime;
                return;
            }
            
            // Return to origin position
            float distance = Vector3.Distance(transform.position, _originPosition);
            if (distance > 0.1f)
            {
                Vector3 direction = (_originPosition - transform.position).normalized;
                float moveAmount = returnSpeed * Time.deltaTime;
                
                if (_characterController != null)
                {
                    _characterController.Move(direction * moveAmount);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, _originPosition, moveAmount);
                }
            }
            
            // Return to origin rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, _originRotation, returnSpeed * Time.deltaTime);
        }
        
        private System.Collections.IEnumerator FlashCoroutine()
        {
            // Flash to hit color
            SetColor(flashColor);
            
            yield return new WaitForSeconds(flashDuration);
            
            // Return to original colors
            RestoreOriginalColors();
        }
        
        private void SetColor(Color color)
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                if (_materials[i] == null) continue;
                
                if (_materials[i].HasProperty("_Color"))
                {
                    _materials[i].color = color;
                }
                else if (_materials[i].HasProperty("_BaseColor"))
                {
                    _materials[i].SetColor("_BaseColor", color);
                }
            }
        }
        
        private void RestoreOriginalColors()
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                if (_materials[i] == null) continue;
                
                if (_materials[i].HasProperty("_Color"))
                {
                    _materials[i].color = _originalColors[i];
                }
                else if (_materials[i].HasProperty("_BaseColor"))
                {
                    _materials[i].SetColor("_BaseColor", _originalColors[i]);
                }
            }
        }
        
        /// <summary>
        /// Reset dummy to original state
        /// </summary>
        public void ResetDummy()
        {
            transform.position = _originPosition;
            transform.rotation = _originRotation;
            _currentPushback = Vector3.zero;
            _isHitStunned = false;
            _totalHitsTaken = 0;
            
            RestoreOriginalColors();
            
            if (idle.IsValid())
            {
                _animancer.Play(idle);
            }
        }
        
        /// <summary>
        /// Set new origin position
        /// </summary>
        public void SetOrigin(Vector3 position, Quaternion rotation)
        {
            _originPosition = position;
            _originRotation = rotation;
        }
        
        public int TotalHitsTaken => _totalHitsTaken;
    }
}
