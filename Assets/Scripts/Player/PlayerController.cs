using UnityEngine;
using UnityEngine.InputSystem;

namespace CombatDemo.Player
{
    /// <summary>
    /// Handles player movement (WASD) and input routing.
    /// Modular design - only handles movement, delegates combat to PlayerCombat.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 720f;
        [SerializeField] private float gravity = -9.81f;
        
        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;
        
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        
        private CharacterController _characterController;
        private PlayerCombat _playerCombat;
        private PlayerAnimator _playerAnimator;
        
        private Vector2 _moveInput;
        private Vector3 _velocity;
        private bool _isDashing;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private Vector3 _dashDirection;
        
        public bool IsMoving => _moveInput.magnitude > 0.1f;
        public bool IsDashing => _isDashing;
        public Vector3 MoveDirection { get; private set; }
        
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerCombat = GetComponent<PlayerCombat>();
            _playerAnimator = GetComponent<PlayerAnimator>();
            
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }
        
        private void Update()
        {
            UpdateCooldowns();
            
            if (_isDashing)
            {
                UpdateDash();
            }
            else
            {
                UpdateMovement();
            }
            
            ApplyGravity();
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
        
        public void OnPunch(InputAction.CallbackContext context)
        {
            if (context.performed && !_isDashing && _playerCombat != null)
            {
                _playerCombat.TryPunch();
            }
        }
        
        public void OnKick(InputAction.CallbackContext context)
        {
            if (context.performed && !_isDashing && _playerCombat != null)
            {
                _playerCombat.TryKick();
            }
        }
        
        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                TryDash();
            }
        }
        
        private void UpdateMovement()
        {
            if (_playerCombat != null && _playerCombat.IsAttacking) return;
            
            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            
            MoveDirection = (forward * _moveInput.y + right * _moveInput.x).normalized;
            
            if (MoveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(MoveDirection);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
                
                _characterController.Move(MoveDirection * moveSpeed * Time.deltaTime);
            }
            
            if (_playerAnimator != null)
            {
                _playerAnimator.SetMovement(MoveDirection.magnitude);
            }
        }
        
        private void TryDash()
        {
            if (_isDashing || _dashCooldownTimer > 0f) return;
            if (_playerCombat != null && _playerCombat.IsAttacking) return;
            
            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;
            
            _dashDirection = MoveDirection.magnitude > 0.1f ? MoveDirection : transform.forward;
            
            if (_playerAnimator != null)
            {
                _playerAnimator.TriggerDash();
            }
            
            Combat.CombatEvents.RaiseDashPerformed();
        }
        
        private void UpdateDash()
        {
            _dashTimer -= Time.deltaTime;
            
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                return;
            }
            
            _characterController.Move(_dashDirection * dashSpeed * Time.deltaTime);
        }
        
        private void UpdateCooldowns()
        {
            if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer -= Time.deltaTime;
            }
        }
        
        private void ApplyGravity()
        {
            if (_characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            
            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }
        
        public void LockMovement(bool locked)
        {
            if (locked)
            {
                _moveInput = Vector2.zero;
            }
        }
    }
}
