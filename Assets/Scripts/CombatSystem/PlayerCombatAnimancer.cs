using UnityEngine;
using UnityEngine.InputSystem;
using Animancer;
using System.Collections.Generic;

namespace CombatSystem
{
    /// <summary>
    /// Main combat animation controller using Animancer.
    /// Handles locomotion, fighting stance, combos, kicks, and dodges.
    /// Designed for smooth, responsive gameplay feel.
    /// </summary>
    [RequireComponent(typeof(AnimancerComponent))]
    public class PlayerCombatAnimancer : MonoBehaviour
    {
        #region Animation References
        
        [Header("=== LOCOMOTION ANIMATIONS ===")]
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip walkClip;
        [SerializeField] private AnimationClip runClip;
        [SerializeField] private AnimationClip jumpClip;
        [SerializeField] private AnimationClip inAirClip;
        [SerializeField] private AnimationClip landClip;
        
        [Header("=== COMBAT STANCE ===")]
        [SerializeField] private AnimationClip fightingIdleClip;
        [SerializeField] private AvatarMask upperBodyMask;
        
        [Header("=== PUNCH COMBO ===")]
        [SerializeField] private AnimationClip punchJabClip;
        [SerializeField] private AnimationClip punchCrossClip;
        [SerializeField] private AnimationClip punchComboClip;
        
        [Header("=== KICK ===")]
        [SerializeField] private AnimationClip kickClip;
        
        [Header("=== DODGE ANIMATIONS ===")]
        [SerializeField] private AnimationClip dodgeForwardClip;
        [SerializeField] private AnimationClip dodgeBackwardClip;
        [SerializeField] private AnimationClip dodgeLeftClip;
        [SerializeField] private AnimationClip dodgeRightClip;
        
        #endregion
        
        #region Settings
        
        [Header("=== LOCOMOTION SETTINGS ===")]
        [SerializeField] private float walkThreshold = 0.1f;
        [SerializeField] private float runThreshold = 0.5f;
        [SerializeField] private float locomotionFadeDuration = 0.25f;
        
        [Header("=== FIGHTING STANCE SETTINGS ===")]
        [SerializeField] private float fightingStanceDistance = 5f;
        [SerializeField] private float stanceBlendSpeed = 3f;
        [SerializeField] private Transform targetDummy;
        
        [Header("=== COMBO SETTINGS ===")]
        [SerializeField] private float comboInputWindow = 0.6f; // Window to input next attack (% of animation)
        [SerializeField] private float comboFadeDuration = 0.1f;
        [SerializeField] private float returnToIdleFade = 0.25f;
        [SerializeField] private float hitDetectionTime = 0.3f; // When in animation to check hit
        
        [Header("=== KICK SETTINGS ===")]
        [SerializeField] private float kickFadeDuration = 0.1f;
        [SerializeField] private float kickHitTime = 0.4f;
        
        [Header("=== DODGE SETTINGS ===")]
        [SerializeField] private float dodgeFadeDuration = 0.1f;
        [SerializeField] private float dodgeCooldown = 0.5f;
        [SerializeField] private float dodgeMovementSpeed = 8f;
        
        [Header("=== HIT DETECTION ===")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackAngle = 60f;
        [SerializeField] private LayerMask targetLayer;
        
        #endregion
        
        #region Runtime State
        
        private AnimancerComponent _animancer;
        private CharacterController _characterController;
        
        // Layers
        private AnimancerLayer _baseLayer;
        private AnimancerLayer _fightingStanceLayer;
        private AnimancerLayer _combatActionLayer;
        
        // Locomotion mixer
        private LinearMixerState _locomotionMixer;
        
        // Combat state
        private enum CombatState { None, Punching, Kicking, Dodging }
        private CombatState _currentCombatState = CombatState.None;
        
        // Combo tracking
        private int _comboStep = 0; // 0=none, 1=jab, 2=cross, 3=combo
        private bool _comboInputQueued = false;
        private bool _hitCheckedThisAttack = false;
        
        // Dodge
        private float _dodgeCooldownTimer = 0f;
        private Vector2 _dodgeDirection;
        private bool _isDodging = false;
        
        // Input
        private Vector2 _moveInput;
        private bool _punchPressed;
        private bool _kickPressed;
        private bool _dodgePressed;
        private float _currentSpeed;
        
        // Fighting stance
        private float _currentStanceWeight = 0f;
        
        // Grounded state (from CharacterController or external)
        private bool _isGrounded = true;
        private bool _wasGrounded = true;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _animancer = GetComponent<AnimancerComponent>();
            _characterController = GetComponent<CharacterController>();
            
            SetupAnimancerLayers();
            SetupLocomotionMixer();
        }
        
        private void Start()
        {
            // Find dummy if not assigned
            if (targetDummy == null)
            {
                GameObject dummy = GameObject.FindGameObjectWithTag("Dummy");
                if (dummy != null) targetDummy = dummy.transform;
            }
        }
        
        private void Update()
        {
            UpdateTimers();
            UpdateFightingStanceWeight();
            
            if (_currentCombatState == CombatState.None)
            {
                UpdateLocomotion();
                ProcessCombatInput();
            }
            else
            {
                UpdateCombatAction();
            }
            
            // Track grounded state
            _wasGrounded = _isGrounded;
            if (_characterController != null)
            {
                _isGrounded = _characterController.isGrounded;
            }
        }
        
        #endregion
        
        #region Setup
        
        private void SetupAnimancerLayers()
        {
            // Layer 0: Base locomotion
            _baseLayer = _animancer.Layers[0];
            
            // Layer 1: Fighting stance (upper body only)
            _fightingStanceLayer = _animancer.Layers[1];
            if (upperBodyMask != null)
            {
                // Animancer API: use the Mask property instead of SetMask
                _fightingStanceLayer.Mask = upperBodyMask;
            }
            _fightingStanceLayer.Weight = 0f;
            
            // Layer 2: Combat actions (full body override)
            _combatActionLayer = _animancer.Layers[2];
            _combatActionLayer.Weight = 0f;
        }
        
        private void SetupLocomotionMixer()
        {
            // Create a linear mixer for smooth idle -> walk -> run blending using a transition
            var locomotionTransition = new LinearMixerTransition
            {
                Animations = new AnimationClip[] { idleClip, walkClip, runClip },
                Thresholds = new float[] { 0f, 0.5f, 1f }
            };

            _locomotionMixer = (LinearMixerState)_baseLayer.Play(locomotionTransition);
        }
        
        #endregion
        
        #region Input (Call these from PlayerInput component or your input handler)
        
        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
        
        public void OnPunch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _punchPressed = true;
            }
        }
        
        public void OnKick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _kickPressed = true;
            }
        }
        
        public void OnDodge(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _dodgePressed = true;
            }
        }
        
        // Alternative: Set speed directly from ThirdPersonController
        public void SetSpeed(float speed)
        {
            _currentSpeed = speed;
        }
        
        public void SetGrounded(bool grounded)
        {
            _isGrounded = grounded;
        }
        
        #endregion
        
        #region Locomotion
        
        private void UpdateLocomotion()
        {
            // Calculate speed from input or use externally set speed
            float targetSpeed = _moveInput.magnitude;
            if (_currentSpeed > 0.01f)
            {
                targetSpeed = _currentSpeed;
            }
            
            // Normalize to 0-1 range for mixer
            float normalizedSpeed = Mathf.Clamp01(targetSpeed);
            
            // Smooth the parameter for buttery locomotion
            _locomotionMixer.Parameter = Mathf.Lerp(
                _locomotionMixer.Parameter,
                normalizedSpeed,
                Time.deltaTime * 10f
            );
        }
        
        #endregion
        
        #region Fighting Stance
        
        private void UpdateFightingStanceWeight()
        {
            float targetWeight = 0f;
            
            if (targetDummy != null)
            {
                float distance = Vector3.Distance(transform.position, targetDummy.position);
                
                if (distance <= fightingStanceDistance)
                {
                    // Smooth falloff based on distance
                    targetWeight = 1f - Mathf.Clamp01((distance - 1f) / (fightingStanceDistance - 1f));
                }
            }
            
            // Smooth transition
            _currentStanceWeight = Mathf.Lerp(_currentStanceWeight, targetWeight, Time.deltaTime * stanceBlendSpeed);
            
            // Apply to layer
            _fightingStanceLayer.Weight = _currentStanceWeight;
            
            // Play fighting idle on stance layer if weight > 0
            if (_currentStanceWeight > 0.01f && fightingIdleClip != null)
            {
                if (!_fightingStanceLayer.IsPlayingClip(fightingIdleClip))
                {
                    _fightingStanceLayer.Play(fightingIdleClip);
                }
            }
        }
        
        #endregion
        
        #region Combat Input Processing
        
        private void ProcessCombatInput()
        {
            // Priority: Dodge > Kick > Punch
            if (_dodgePressed && _dodgeCooldownTimer <= 0f)
            {
                StartDodge();
                _dodgePressed = false;
                return;
            }
            
            if (_kickPressed)
            {
                StartKick();
                _kickPressed = false;
                return;
            }
            
            if (_punchPressed)
            {
                StartPunchCombo();
                _punchPressed = false;
                return;
            }
            
            // Clear unused inputs
            _dodgePressed = false;
            _kickPressed = false;
            _punchPressed = false;
        }
        
        #endregion
        
        #region Punch Combo System
        
        private void StartPunchCombo()
        {
            _currentCombatState = CombatState.Punching;
            _comboStep = 1;
            _comboInputQueued = false;
            _hitCheckedThisAttack = false;
            
            // Activate combat layer
            _combatActionLayer.Weight = 1f;
            
            // Play jab
            var state = _combatActionLayer.Play(punchJabClip, comboFadeDuration);
            state.Events(this).OnEnd += () => OnPunchAnimationEnd();
            
            //state.Events()
        }
        
        private void ContinueCombo()
        {
            _comboInputQueued = false;
            _hitCheckedThisAttack = false;
            
            AnimationClip nextClip = null;
            
            switch (_comboStep)
            {
                case 1:
                    _comboStep = 2;
                    nextClip = punchCrossClip;
                    break;
                case 2:
                    _comboStep = 3;
                    nextClip = punchComboClip;
                    break;
                case 3:
                    // Stay at combo, just repeat
                    nextClip = punchComboClip;
                    break;
            }
            
            if (nextClip != null)
            {
                var state = _combatActionLayer.Play(nextClip, comboFadeDuration);
                state.Events(this).OnEnd += () => OnPunchAnimationEnd();
            }
        }
        
        private void OnPunchAnimationEnd()
        {
            if (_comboInputQueued)
            {
                ContinueCombo();
            }
            else
            {
                EndCombatAction();
            }
        }
        
        #endregion
        
        #region Kick
        
        private void StartKick()
        {
            _currentCombatState = CombatState.Kicking;
            _hitCheckedThisAttack = false;
            
            _combatActionLayer.Weight = 1f;
            
            var state = _combatActionLayer.Play(kickClip, kickFadeDuration);
            state.Events(this).OnEnd += () => EndCombatAction();
        }
        
        #endregion
        
        #region Dodge System
        
        private void StartDodge()
        {
            _currentCombatState = CombatState.Dodging;
            _isDodging = true;
            _dodgeCooldownTimer = dodgeCooldown;
            
            // Store dodge direction
            _dodgeDirection = _moveInput.magnitude > 0.1f ? _moveInput.normalized : Vector2.down;
            
            _combatActionLayer.Weight = 1f;
            
            // Determine which dodge animation(s) to blend
            PlayDirectionalDodge(_dodgeDirection);
        }
        
        private void PlayDirectionalDodge(Vector2 direction)
        {
            // Normalize direction
            float x = direction.x;
            float y = direction.y;
            
            // For pure directions, play single animation
            // For diagonals, we'll use a mixer
            
            bool isForward = y > 0.5f;
            bool isBackward = y < -0.5f;
            bool isLeft = x < -0.5f;
            bool isRight = x > 0.5f;
            
            AnimationClip primaryClip = null;
            AnimationClip secondaryClip = null;
            float blendRatio = 0f;
            
            // Determine clips based on direction
            if (isForward && !isLeft && !isRight)
            {
                primaryClip = dodgeForwardClip;
            }
            else if (isBackward && !isLeft && !isRight)
            {
                primaryClip = dodgeBackwardClip;
            }
            else if (isLeft && !isForward && !isBackward)
            {
                primaryClip = dodgeLeftClip;
            }
            else if (isRight && !isForward && !isBackward)
            {
                primaryClip = dodgeRightClip;
            }
            else if (isForward && isLeft) // Forward-Left diagonal
            {
                primaryClip = dodgeForwardClip;
                secondaryClip = dodgeLeftClip;
                blendRatio = 0.5f;
            }
            else if (isForward && isRight) // Forward-Right diagonal
            {
                primaryClip = dodgeForwardClip;
                secondaryClip = dodgeRightClip;
                blendRatio = 0.5f;
            }
            else if (isBackward && isLeft) // Backward-Left diagonal
            {
                primaryClip = dodgeBackwardClip;
                secondaryClip = dodgeLeftClip;
                blendRatio = 0.5f;
            }
            else if (isBackward && isRight) // Backward-Right diagonal
            {
                primaryClip = dodgeBackwardClip;
                secondaryClip = dodgeRightClip;
                blendRatio = 0.5f;
            }
            else
            {
                // Default to backward
                primaryClip = dodgeBackwardClip;
            }
            
            // Play the animation(s)
            if (secondaryClip != null && blendRatio > 0f)
            {
                // Create a manual blend for diagonal dodges
                // Use a mixer transition for a 2-way directional dodge blend
                var dodgeTransition = new LinearMixerTransition
                {
                    Animations = new AnimationClip[] { primaryClip, secondaryClip },
                    Thresholds = new float[] { 0f, 1f }
                };

                var mixerState = (LinearMixerState)_combatActionLayer.Play(dodgeTransition, dodgeFadeDuration);
                mixerState.Parameter = blendRatio;
                mixerState.Events(this).OnEnd += () => EndDodge();
            }
            else
            {
                var state = _combatActionLayer.Play(primaryClip, dodgeFadeDuration);
                state.Events(this).OnEnd += () => EndDodge();
            }
        }
        
        private void EndDodge()
        {
            _isDodging = false;
            EndCombatAction();
        }
        
        #endregion
        
        #region Combat Action Updates
        
        private void UpdateCombatAction()
        {
            var currentState = _combatActionLayer.CurrentState;
            if (currentState == null) return;
            
            float normalizedTime = currentState.NormalizedTime;
            
            switch (_currentCombatState)
            {
                case CombatState.Punching:
                    UpdatePunchingState(normalizedTime);
                    break;
                    
                case CombatState.Kicking:
                    UpdateKickingState(normalizedTime);
                    break;
                    
                case CombatState.Dodging:
                    UpdateDodgingState(normalizedTime);
                    break;
            }
        }
        
        private void UpdatePunchingState(float normalizedTime)
        {
            // Check for combo input during window
            if (_punchPressed && normalizedTime >= (1f - comboInputWindow) && normalizedTime < 0.95f)
            {
                _comboInputQueued = true;
                _punchPressed = false;
            }
            
            // Hit detection at specific time
            if (!_hitCheckedThisAttack && normalizedTime >= hitDetectionTime && normalizedTime < hitDetectionTime + 0.1f)
            {
                PerformHitDetection(AttackType.Punch);
                _hitCheckedThisAttack = true;
            }
        }
        
        private void UpdateKickingState(float normalizedTime)
        {
            // Hit detection
            if (!_hitCheckedThisAttack && normalizedTime >= kickHitTime && normalizedTime < kickHitTime + 0.1f)
            {
                PerformHitDetection(AttackType.Kick);
                _hitCheckedThisAttack = true;
            }
        }
        
        private void UpdateDodgingState(float normalizedTime)
        {
            // Apply dodge movement
            if (_characterController != null && _isDodging)
            {
                Vector3 moveDir = transform.TransformDirection(new Vector3(_dodgeDirection.x, 0f, _dodgeDirection.y));
                
                // Movement curve - fast at start, slow at end
                float speedMultiplier = Mathf.Lerp(1f, 0.2f, normalizedTime);
                _characterController.Move(moveDir * dodgeMovementSpeed * speedMultiplier * Time.deltaTime);
            }
        }
        
        private void EndCombatAction()
        {
            _currentCombatState = CombatState.None;
            _comboStep = 0;
            _comboInputQueued = false;
            _hitCheckedThisAttack = false;
            
            // Fade out combat layer
            _combatActionLayer.StartFade(0f, returnToIdleFade);
        }
        
        #endregion
        
        #region Hit Detection
        
        private void PerformHitDetection(AttackType attackType)
        {
            // Find targets in range
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, attackRange, targetLayer);
            
            foreach (var hit in hits)
            {
                // Check angle
                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToTarget);
                
                if (angle <= attackAngle)
                {
                    // Hit confirmed!
                    IDamageable damageable = hit.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        float damage = attackType == AttackType.Punch ? 10f : 15f;
                        Vector3 pushDirection = dirToTarget;
                        
                        damageable.TakeDamage(damage, attackType, pushDirection);
                        
                        // Emit event for stats tracking
                        OnHitLanded?.Invoke(attackType);
                    }
                }
            }
        }
        
        #endregion
        
        #region Timers
        
        private void UpdateTimers()
        {
            if (_dodgeCooldownTimer > 0f)
            {
                _dodgeCooldownTimer -= Time.deltaTime;
            }
        }
        
        #endregion
        
        #region Events
        
        public System.Action<AttackType> OnHitLanded;
        public System.Action<AttackType> OnAttackStarted;
        
        #endregion
        
        #region Debug Visualization
        
        private void OnDrawGizmosSelected()
        {
            // Attack range
            Gizmos.color = Color.red;
            Vector3 attackOrigin = transform.position + transform.forward * 0.5f;
            Gizmos.DrawWireSphere(attackOrigin, attackRange);
            
            // Attack angle
            Vector3 leftDir = Quaternion.Euler(0, -attackAngle, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0, attackAngle, 0) * transform.forward;
            Gizmos.DrawLine(attackOrigin, attackOrigin + leftDir * attackRange);
            Gizmos.DrawLine(attackOrigin, attackOrigin + rightDir * attackRange);
            
            // Fighting stance range
            if (targetDummy != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(targetDummy.position, fightingStanceDistance);
            }
        }
        
        #endregion
    }
}
