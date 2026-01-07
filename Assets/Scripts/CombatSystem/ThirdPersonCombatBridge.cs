using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Animancer;
using CombatDemo.Combat; // For CombatEvents

namespace CombatSystem
{
    /// <summary>
    /// Bridges the existing StarterAssets ThirdPersonController with the combat system.
    /// Uses Animancer Transitions for smooth blending and customization.
    /// </summary>
    [RequireComponent(typeof(AnimancerComponent))]
    public class ThirdPersonCombatBridge : MonoBehaviour
    {
        #region Animation References
        
        [Header("=== LOCOMOTION (Animancer) ===")]
        [Tooltip("Configure this Mixer with Idle, Walk, Run. If left empty, the system will try to use the Legacy clips below.")]
        [SerializeField] private LinearMixerTransition locomotion;
        
        [Header("=== LEGACY LOCOMOTION (Fallback) ===")]
        [Tooltip("Fallback: Used if Locomotion Mixer above is not configured.")]
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip walkClip;
        [SerializeField] private AnimationClip runClip;
        
        [Header("=== COMBAT STANCE ===")]
        [Tooltip("Upper body fighting idle")]
        [SerializeField] private ClipTransition fightingIdle;
        
        [Header("=== ATTACKS ===")]
        [SerializeField] private ClipTransition punchJab;
        [SerializeField] private ClipTransition punchCross;
        [SerializeField] private ClipTransition punchCombo;
        [SerializeField] private ClipTransition kick;
        
        [Header("=== DODGE ===")]
        [SerializeField] private ClipTransition dodgeForward;
        [SerializeField] private ClipTransition dodgeBackward;
        [SerializeField] private ClipTransition dodgeLeft;
        [SerializeField] private ClipTransition dodgeRight;
        
        [Header("=== AVATAR MASK ===")]
        [Tooltip("Create an AvatarMask that only includes upper body for fighting stance blend")]
        [SerializeField] private AvatarMask upperBodyMask;
        
        #endregion
        
        #region Settings
        
        [Header("=== LOCOMOTION SETTINGS ===")]
        [SerializeField] private float locomotionBlendSpeed = 10f;
        
        [Header("=== FIGHTING STANCE ===")]
        [SerializeField] private float fightingStanceRange = 6f;
        [SerializeField] private float stanceBlendSpeed = 5f;
        [SerializeField] private Transform targetDummy;
        
        [Header("=== COMBO TIMING ===")]
        [SerializeField] private float comboWindowStart = 0.4f;  // When combo window opens (% of anim)
        [SerializeField] private float comboWindowEnd = 0.9f;    // When combo window closes
        [SerializeField] private float layerFadeInDuration = 0f; // Instant combat layer (set to 0.15 for smooth blend)
        [SerializeField] private float layerFadeOutDuration = 0.25f; // Smooth exit from combat
        
        [Header("=== DODGE SETTINGS ===")]
        [SerializeField] private float dodgeCooldown = 0.6f;
        [SerializeField] private float dodgeSpeed = 8f;
        
        [Header("=== HIT DETECTION ===")]
        [SerializeField] private float punchHitTime = 0.35f;     // When punch hits (% of anim)
        [SerializeField] private float kickHitTime = 0.45f;      // When kick hits
        [SerializeField] private float attackRange = 2.0f;
        [SerializeField] private float attackAngle = 90f;
        [SerializeField] private LayerMask hitLayers = -1;
        [SerializeField] private bool debugHitDetection = true;
        
        #endregion
        
        #region Private State
        
        private AnimancerComponent _animancer;
        private CharacterController _controller;
        private Animator _legacyAnimator;
        
        // Animancer layers
        private AnimancerLayer _locomotionLayer;
        private AnimancerLayer _fightingStanceLayer;
        private AnimancerLayer _combatLayer;
        
        // Locomotion state
        private LinearMixerState _locomotionMixerState;
        private float _currentLocomotionParam;
        
        // Fighting stance
        private float _stanceWeight;
        
        // Combat state
        private enum CombatState { None, Punching, Kicking, Dodging }
        private CombatState _combatState = CombatState.None;
        
        // Combo
        private int _comboIndex; // 0=none, 1=jab, 2=cross, 3+=combo
        private bool _comboQueued;
        private bool _hitChecked;
        
        // Dodge
        private float _dodgeCooldownTimer;
        private Vector2 _lastMoveInput;
        private Vector3 _dodgeVelocity;
        
        // Input flags
        private bool _punchInput;
        private bool _kickInput;
        private bool _dodgeInput;
        
        // External state
        private float _moveSpeed;
        private bool _isGrounded = true;
        private bool _isJumping;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _animancer = GetComponent<AnimancerComponent>();
            _controller = GetComponent<CharacterController>();
            _legacyAnimator = GetComponent<Animator>();

            if (_legacyAnimator != null)
            {
                _legacyAnimator.enabled = true;
                if (_legacyAnimator.runtimeAnimatorController != null)
                {
                    _legacyAnimator.runtimeAnimatorController = null;
                }
            }
        }
        
        private void Start()
        {
            SetupLayers();
            SetupLocomotion();
            FindDummy();
        }
        
        private void Update()
        {
            UpdateTimers();
            UpdateLocomotion();
            UpdateFightingStance();
            
            if (_combatState == CombatState.None)
            {
                ProcessCombatInput();
            }
            else
            {
                UpdateCombatState();
            }
            
            ClearInputFlags();
        }
        
        #endregion
        
        #region Setup
        
        private void SetupLayers()
        {
            _locomotionLayer = _animancer.Layers[0];

            _fightingStanceLayer = _animancer.Layers[1];

            if (upperBodyMask != null)
            {
                _fightingStanceLayer.Mask = upperBodyMask;
            }
            else
            {
                Debug.LogWarning("[CombatBridge] Upper Body Mask is missing! Fighting stance blending will affect the whole body.");
            }
            _fightingStanceLayer.Weight = 0f;

            _combatLayer = _animancer.Layers[2];

            // // Apply upper body mask to combat layer so legs keep animating while punching
            // if (upperBodyMask != null)
            // {
            //     _combatLayer.Mask = upperBodyMask;
            //     Debug.Log("[CombatBridge] Upper body mask applied to combat layer - legs will animate while punching!");
            // }
            // else
            // {
            //     Debug.LogWarning("[CombatBridge] Upper Body Mask is missing! Combat animations will affect the whole body (legs won't move while punching).");
            // }

            _combatLayer.Weight = 0f;
        }
        
        private void SetupLocomotion()
        {
            // 1. Try to use the configured Mixer Transition
            if (locomotion.IsValid())
            {
                var state = _locomotionLayer.Play(locomotion);
                if (state is LinearMixerState mixerState)
                {
                    _locomotionMixerState = mixerState;
                    Debug.Log("[CombatBridge] Using configured Locomotion Mixer.");
                }
                else
                {
                    Debug.LogError("[CombatBridge] Locomotion transition is not a LinearMixer!");
                }
            }
            // 2. Fallback to Legacy Clips if Mixer is invalid
            else if (idleClip != null && walkClip != null && runClip != null)
            {
                Debug.Log("[CombatBridge] Locomotion Mixer not configured. Auto-generating from Legacy Clips.");
                
                var transition = new LinearMixerTransition
                {
                    Animations = new AnimationClip[] { idleClip, walkClip, runClip },
                    Thresholds = new float[] { 0f, 0.5f, 1f },
                    FadeDuration = 0.25f // Re-added FadeDuration
                };
                
                _locomotionMixerState = (LinearMixerState)_locomotionLayer.Play(transition);
            }
            else
            {
                Debug.LogError("[CombatBridge] No Locomotion animations found! Please configure the Locomotion Mixer or assign Legacy Clips.");
            }
        }
        
        private void FindDummy()
        {
            if (targetDummy == null)
            {
                var dummy = GameObject.FindWithTag("Dummy");
                if (dummy != null) 
                {
                    targetDummy = dummy.transform;
                    Debug.Log($"[CombatBridge] Found Dummy at {targetDummy.position}");
                }
            }
        }
        
        #endregion
        
        #region Input Handlers
        
        public void OnMove(InputValue value) => _lastMoveInput = value.Get<Vector2>();
        
        public void OnPunch(InputValue value) 
        { 
            if (value.isPressed) _punchInput = true; 
        }
        
        public void OnKick(InputValue value) { if (value.isPressed) _kickInput = true; }
        public void OnDodge(InputValue value) { if (value.isPressed) _dodgeInput = true; }
        public void OnJump(InputValue value) { if (value.isPressed) _isJumping = true; }
        
        public void SetMoveSpeed(float speed) => _moveSpeed = speed;
        public void SetGrounded(bool grounded)
        {
            _isGrounded = grounded;
            if (grounded) _isJumping = false;
        }
        
        private void ClearInputFlags()
        {
            _punchInput = false;
            _kickInput = false;
            _dodgeInput = false;
        }
        
        #endregion
        
        #region Locomotion
        
        private void UpdateLocomotion()
        {
            // Allow locomotion update even during combat if we want to move
            // But we might want to dampen the effect on the upper body if we are punching
            // Since combat layer is on top (Layer 2), it will override anyway.
            
            float targetParam = Mathf.Clamp01(_moveSpeed);
            _currentLocomotionParam = Mathf.Lerp(_currentLocomotionParam, targetParam, locomotionBlendSpeed * Time.deltaTime);
            
            if (_locomotionMixerState != null)
            {
                _locomotionMixerState.Parameter = _currentLocomotionParam;
            }
        }
        
        #endregion
        
        #region Fighting Stance
        
        private void UpdateFightingStance()
        {
            float targetWeight = 0f;
            
            if (targetDummy != null && _combatState == CombatState.None)
            {
                float dist = Vector3.Distance(transform.position, targetDummy.position);
                if (dist < fightingStanceRange)
                {
                    targetWeight = 1f - Mathf.InverseLerp(1f, fightingStanceRange, dist);
                }
            }
            
            _stanceWeight = Mathf.Lerp(_stanceWeight, targetWeight, stanceBlendSpeed * Time.deltaTime);
            _fightingStanceLayer.Weight = _stanceWeight;
            
            if (_stanceWeight > 0.01f && fightingIdle.IsValid())
            {
                // Re-added IsPlayingClip check logic
                if (!_fightingStanceLayer.IsPlayingClip(fightingIdle.Clip))
                {
                    // Play with 0 fade - the layer weight handles the blend
                    _fightingStanceLayer.Play(fightingIdle, 0f);
                }
            }
        }
        
        #endregion
        
        #region Combat Input
        
        private void ProcessCombatInput()
        {
            if (!_isGrounded) return;
            
            if (_dodgeInput && _dodgeCooldownTimer <= 0f)
            {
                StartDodge();
                return;
            }
            
            if (_kickInput)
            {
                StartKick();
                return;
            }
            
            if (_punchInput)
            {
                StartPunch();
            }
        }
        
        #endregion
        
        #region Punch Combo
        
        private void StartPunch()
        {
            _combatState = CombatState.Punching;
            _comboIndex = 1;
            _comboQueued = false;
            _hitChecked = false;

            // Fade in the combat layer
            _combatLayer.StartFade(1f, layerFadeInDuration);

            // Raise combat event for stats tracking
            CombatEvents.RaiseAttackPerformed(AttackType.Punch);

            PlayAttack(punchJab);
        }
        
        private void ContinuePunch()
        {
            _comboQueued = false;
            _hitChecked = false;
            
            ClipTransition nextClip;
            
            // Cycle: 1 -> 2 -> 3 -> 3 (Repeat Combo)
            if (_comboIndex == 1)
            {
                _comboIndex = 2;
                nextClip = punchCross;
            }
            else if (_comboIndex == 2)
            {
                _comboIndex = 3;
                nextClip = punchCombo;
            }
            else
            {
                // Keep doing the combo animation if we keep clicking
                _comboIndex = 3; 
                nextClip = punchCombo;
            }
            
            PlayAttack(nextClip);
        }
        
        private void PlayAttack(ClipTransition clip)
        {
            if (!clip.IsValid())
            {
                Debug.LogError($"[CombatBridge] Attack animation missing!");
                EndCombat();
                return;
            }

            // Play with 0 fade duration - let the layer weight handle the blend
            var state = _combatLayer.Play(clip, 0f);
            state.Events(this).OnEnd = OnPunchEnd;
        }
        
        private void OnPunchEnd()
        {
            if (_comboQueued)
            {
                ContinuePunch();
            }
            else
            {
                EndCombat();
            }
        }
        
        #endregion
        
        #region Kick
        
        private void StartKick()
        {
            _combatState = CombatState.Kicking;
            _hitChecked = false;

            // Fade in combat layer
            _combatLayer.StartFade(1f, layerFadeInDuration);

            // Raise combat event for stats tracking
            CombatEvents.RaiseAttackPerformed(AttackType.Kick);

            if (kick.IsValid())
            {
                // Play with 0 fade - let layer weight handle the blend
                var state = _combatLayer.Play(kick, 0f);
                state.Events(this).OnEnd = EndCombat;
            }
            else
            {
                EndCombat();
            }
        }
        
        #endregion
        
        #region Dodge
        
        private void StartDodge()
        {
            _combatState = CombatState.Dodging;
            _dodgeCooldownTimer = dodgeCooldown;

            Vector2 dir = _lastMoveInput.magnitude > 0.1f ? _lastMoveInput.normalized : Vector2.down;
            Vector3 worldDir = transform.right * dir.x + transform.forward * dir.y;
            _dodgeVelocity = worldDir.normalized * dodgeSpeed;

            // Fade in combat layer
            _combatLayer.StartFade(1f, layerFadeInDuration);

            // Raise combat event for stats tracking
            CombatEvents.RaiseDashPerformed();

            PlayDirectionalDodge(dir);
        }
        
        private void PlayDirectionalDodge(Vector2 direction)
        {
            float x = direction.x;
            float y = direction.y;
            
            bool isForward = y > 0.5f;
            bool isBackward = y < -0.5f;
            bool isLeft = x < -0.5f;
            bool isRight = x > 0.5f;
            
            ClipTransition primary = null;
            ClipTransition secondary = null;
            float blendRatio = 0f;
            
            if (isForward && !isLeft && !isRight) primary = dodgeForward;
            else if (isBackward && !isLeft && !isRight) primary = dodgeBackward;
            else if (isLeft && !isForward && !isBackward) primary = dodgeLeft;
            else if (isRight && !isForward && !isBackward) primary = dodgeRight;
            else if (isForward && isLeft) { primary = dodgeForward; secondary = dodgeLeft; blendRatio = 0.5f; }
            else if (isForward && isRight) { primary = dodgeForward; secondary = dodgeRight; blendRatio = 0.5f; }
            else if (isBackward && isLeft) { primary = dodgeBackward; secondary = dodgeLeft; blendRatio = 0.5f; }
            else if (isBackward && isRight) { primary = dodgeBackward; secondary = dodgeRight; blendRatio = 0.5f; }
            else primary = dodgeBackward;
            
            if (primary == null || !primary.IsValid())
            {
                Debug.LogWarning("[CombatBridge] Dodge animation missing!");
                EndCombat();
                return;
            }

            if (secondary != null && secondary.IsValid() && blendRatio > 0f)
            {
                // Construct a temporary mixer for diagonal blending
                var dodgeTransition = new LinearMixerTransition
                {
                    Animations = new AnimationClip[] { primary.Clip, secondary.Clip },
                    Thresholds = new float[] { 0f, 1f },
                    FadeDuration = 0f  // No fade - let layer weight handle it
                };

                var mixerState = (LinearMixerState)_combatLayer.Play(dodgeTransition);
                mixerState.Parameter = blendRatio;
                mixerState.Events(this).OnEnd = EndCombat;
            }
            else
            {
                // Play with 0 fade - let layer weight handle the blend
                var state = _combatLayer.Play(primary, 0f);
                state.Events(this).OnEnd = EndCombat;
            }
        }
        
        #endregion
        
        #region Combat State Update
        
        private void UpdateCombatState()
        {
            var currentState = _combatLayer.CurrentState;
            if (currentState == null) return;
            
            float t = currentState.NormalizedTime;
            
            switch (_combatState)
            {
                case CombatState.Punching:
                    // Check for input (Triggered only, NOT held)
                    // We only want to queue if the user pressed the button AGAIN during this window
                    if (_punchInput && t >= comboWindowStart && t < comboWindowEnd)
                    {
                        _comboQueued = true;
                        // Consume input so we don't queue multiple times for one press
                        _punchInput = false; 
                    }
                    
                    if (!_hitChecked && t >= punchHitTime && t < punchHitTime + 0.1f)
                    {
                        PerformHitCheck(true);
                        _hitChecked = true;
                    }
                    break;
                    
                case CombatState.Kicking:
                    if (!_hitChecked && t >= kickHitTime && t < kickHitTime + 0.1f)
                    {
                        PerformHitCheck(false);
                        _hitChecked = true;
                    }
                    break;
                    
                case CombatState.Dodging:
                    if (_controller != null)
                    {
                        float speedCurve = 1f - t;
                        _controller.Move(_dodgeVelocity * speedCurve * Time.deltaTime);
                    }
                    break;
            }
        }
        
        private void EndCombat()
        {
            _combatState = CombatState.None;
            _comboIndex = 0;
            _comboQueued = false;
            _hitChecked = false;
            _dodgeVelocity = Vector3.zero;

            // Fade out to locomotion
            _combatLayer.StartFade(0f, layerFadeOutDuration);
        }
        
        #endregion
        
        #region Hit Detection
        
        private void PerformHitCheck(bool isPunch)
        {
            Vector3 origin = transform.position + Vector3.up * 1f;
            Vector3 center = origin + transform.forward * 0.5f;

            if (debugHitDetection)
            {
                Debug.Log($"[CombatBridge] Checking Hit... Origin: {center}, Range: {attackRange}");
            }

            Collider[] hits = Physics.OverlapSphere(center, attackRange, hitLayers);

            if (debugHitDetection && hits.Length > 0)
            {
                Debug.Log($"[CombatBridge] OverlapSphere found {hits.Length} colliders.");
            }

            bool hitLanded = false;
            AttackType attackType = isPunch ? AttackType.Punch : AttackType.Kick;

            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;
                if (hit.isTrigger) continue; // Ignore triggers usually
                
                Vector3 toTarget = (hit.transform.position - transform.position);
                toTarget.y = 0;
                
                float angle = Vector3.Angle(transform.forward, toTarget.normalized);
                
                if (debugHitDetection)
                {
                    Debug.Log($"[CombatBridge] Potential Target: {hit.name}, Angle: {angle}, Dist: {toTarget.magnitude}");
                }
                
                if (angle > attackAngle) continue;

                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Debug.Log($"[CombatBridge] HIT CONFIRMED on {hit.name}!");

                    damageable.TakeDamage(isPunch ? 10f : 15f, attackType, toTarget.normalized);

                    // Raise hit landed event
                    hitLanded = true;
                    CombatEvents.RaiseHitLanded(attackType, hit.transform.position);
                }
                else
                {
                    // Try finding on parent
                    damageable = hit.GetComponentInParent<IDamageable>();
                    if (damageable != null)
                    {
                        Debug.Log($"[CombatBridge] HIT CONFIRMED on Parent {hit.transform.parent.name}!");
                        damageable.TakeDamage(isPunch ? 10f : 15f, attackType, toTarget.normalized);

                        // Raise hit landed event
                        hitLanded = true;
                        CombatEvents.RaiseHitLanded(attackType, hit.transform.parent.position);
                    }
                }
            }

            // If no hit landed, raise hit missed event
            if (!hitLanded)
            {
                CombatEvents.RaiseHitMissed(attackType);
            }
        }
        
        #endregion
        
        #region Timers
        
        private void UpdateTimers()
        {
            if (_dodgeCooldownTimer > 0f)
                _dodgeCooldownTimer -= Time.deltaTime;
        }
        
        #endregion
        
        #region Public API
        
        public bool IsInCombatAction => _combatState != CombatState.None;
        public bool IsDodging => _combatState == CombatState.Dodging;
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 origin = transform.position + Vector3.up + transform.forward * 0.5f;
            Gizmos.DrawWireSphere(origin, attackRange);
            
            // Draw angle lines
            Vector3 left = Quaternion.Euler(0, -attackAngle, 0) * transform.forward;
            Vector3 right = Quaternion.Euler(0, attackAngle, 0) * transform.forward;
            Gizmos.DrawLine(origin, origin + left * attackRange);
            Gizmos.DrawLine(origin, origin + right * attackRange);
            
            Gizmos.color = Color.yellow;
            if (targetDummy != null)
            {
                Gizmos.DrawWireSphere(targetDummy.position, fightingStanceRange);
            }
        }
        
        #endregion
    }
}
