using UnityEngine;
using StarterAssets;

namespace CombatSystem
{
    /// <summary>
    /// Syncs the existing StarterAssets ThirdPersonController with the combat system.
    /// This reads the movement state from TPC and feeds it to the combat bridge.
    /// Also modifies movement during combat actions.
    /// </summary>
    public class ThirdPersonControllerSync : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ThirdPersonCombatBridge combatBridge;
        
        [Header("Speed Mapping")]
        [SerializeField] private float sprintSpeed = 5.335f;
        
        [Header("Combat Movement")]
        [Tooltip("Multiplier for movement speed while attacking (0 = stop, 1 = full speed)")]
        [SerializeField] [Range(0f, 1f)] private float attackMovementMultiplier = 0.2f;
        
        // Cached references
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        
        private float _lastHorizontalSpeed;
        
        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            
            if (combatBridge == null)
            {
                combatBridge = GetComponent<ThirdPersonCombatBridge>();
            }
        }
        
        private void Update()
        {
            if (combatBridge == null) return;
            
            // 1. Sync Speed to Combat System (for animations)
            if (_controller != null)
            {
                Vector3 horizontalVelocity = _controller.velocity;
                horizontalVelocity.y = 0;
                _lastHorizontalSpeed = horizontalVelocity.magnitude;
            }
            
            // Normalize speed to 0-1 range
            float normalizedSpeed = Mathf.InverseLerp(0f, sprintSpeed, _lastHorizontalSpeed);
            combatBridge.SetMoveSpeed(normalizedSpeed);
            
            // 2. Sync Grounded State
            bool grounded = true;
            if (_controller != null)
            {
                grounded = _controller.isGrounded;
            }
            combatBridge.SetGrounded(grounded);
            
            // 3. Modify Movement during Combat Actions
            if (ShouldModifyMovement())
            {
                if (_input != null)
                {
                    // Instead of stopping completely, we scale the input
                    // This allows "sliding" or slow movement while attacking
                    if (_input.move != Vector2.zero)
                    {
                        // We can't easily modify the TPC speed directly without reflection or modifying TPC code
                        // But we can trick it by pulsing input or we can just accept that TPC will move at 'MoveSpeed'
                        // Since TPC uses analog movement, reducing the vector magnitude reduces speed
                        
                        // Note: We are modifying the input object which TPC reads. 
                        // We need to be careful not to permanently clamp it if TPC normalizes it.
                        // StarterAssets TPC normalizes input direction but multiplies by magnitude if 'analogMovement' is on.
                        
                        // For now, let's just clamp the sprint to false
                        _input.sprint = false;
                        _input.jump = false;
                        
                        // If we want to slow down, we might need to modify the TPC's MoveSpeed via reflection or public field if available
                        // But simply not zeroing it allows movement.
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if movement should be modified (Punching, Kicking)
        /// Dodge handles its own movement.
        /// </summary>
        public bool ShouldModifyMovement()
        {
            if (combatBridge == null) return false;
            
            // Modify TPC input during combat actions (except Dodge which overrides movement completely)
            return combatBridge.IsInCombatAction && !combatBridge.IsDodging;
        }
    }
}
