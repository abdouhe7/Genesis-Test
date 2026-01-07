using UnityEngine;
using CombatDemo.Combat;
using CombatSystem; // For AttackType enum

namespace CombatDemo.Player
{
    /// <summary>
    /// Handles all player animations.
    /// Decoupled from logic - receives commands and triggers appropriate animations.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string punchTrigger = "Punch";
        [SerializeField] private string kickTrigger = "Kick";
        [SerializeField] private string dashTrigger = "Dash";
        [SerializeField] private string hitTrigger = "Hit";
        
        [Header("Animation Smoothing")]
        [SerializeField] private float movementSmoothTime = 0.1f;
        
        private Animator _animator;
        private float _currentSpeed;
        private float _speedVelocity;
        
        // Cached parameter hashes for performance
        private int _speedHash;
        private int _punchHash;
        private int _kickHash;
        private int _dashHash;
        private int _hitHash;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            CacheParameterHashes();
        }
        
        private void CacheParameterHashes()
        {
            _speedHash = Animator.StringToHash(speedParam);
            _punchHash = Animator.StringToHash(punchTrigger);
            _kickHash = Animator.StringToHash(kickTrigger);
            _dashHash = Animator.StringToHash(dashTrigger);
            _hitHash = Animator.StringToHash(hitTrigger);
        }
        
        /// <summary>
        /// Set movement speed for blend tree
        /// </summary>
        public void SetMovement(float speed)
        {
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, speed, ref _speedVelocity, movementSmoothTime);
            _animator.SetFloat(_speedHash, _currentSpeed);
        }
        
        /// <summary>
        /// Trigger attack animation based on type
        /// </summary>
        public void TriggerAttack(AttackType type)
        {
            switch (type)
            {
                case AttackType.Punch:
                    _animator.SetTrigger(_punchHash);
                    break;
                case AttackType.Kick:
                    _animator.SetTrigger(_kickHash);
                    break;
            }
        }
        
        /// <summary>
        /// Trigger dash animation
        /// </summary>
        public void TriggerDash()
        {
            _animator.SetTrigger(_dashHash);
        }
        
        /// <summary>
        /// Trigger hit reaction animation
        /// </summary>
        public void TriggerHitReaction()
        {
            _animator.SetTrigger(_hitHash);
        }
        
        /// <summary>
        /// Get current animation state info
        /// </summary>
        public AnimatorStateInfo GetCurrentState(int layer = 0)
        {
            return _animator.GetCurrentAnimatorStateInfo(layer);
        }
        
        /// <summary>
        /// Check if currently in a specific state
        /// </summary>
        public bool IsInState(string stateName, int layer = 0)
        {
            return _animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName);
        }
    }
}
