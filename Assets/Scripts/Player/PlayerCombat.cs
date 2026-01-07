using UnityEngine;
using CombatDemo.Combat;

namespace CombatDemo.Player
{
    /// <summary>
    /// Handles player combat actions (punch, kick).
    /// Manages attack states, cooldowns, and hit detection.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private float punchCooldown = 0.5f;
        [SerializeField] private float kickCooldown = 0.8f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackRadius = 0.5f;
        
        [Header("Attack Damage")]
        [SerializeField] private float punchDamage = 10f;
        [SerializeField] private float kickDamage = 15f;
        
        [Header("Attack Pushback")]
        [SerializeField] private float punchPushback = 2f;
        [SerializeField] private float kickPushback = 4f;
        
        [Header("References")]
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private LayerMask targetLayers;
        
        private PlayerAnimator _playerAnimator;
        private PlayerController _playerController;
        
        private float _punchCooldownTimer;
        private float _kickCooldownTimer;
        private bool _isAttacking;
        private AttackType _currentAttack;
        
        public bool IsAttacking => _isAttacking;
        public AttackType CurrentAttack => _currentAttack;
        
        private void Awake()
        {
            _playerAnimator = GetComponent<PlayerAnimator>();
            _playerController = GetComponent<PlayerController>();
            
            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }
        }
        
        private void Update()
        {
            UpdateCooldowns();
        }
        
        private void UpdateCooldowns()
        {
            if (_punchCooldownTimer > 0f)
                _punchCooldownTimer -= Time.deltaTime;
            
            if (_kickCooldownTimer > 0f)
                _kickCooldownTimer -= Time.deltaTime;
        }
        
        public void TryPunch()
        {
            if (_isAttacking || _punchCooldownTimer > 0f) return;
            
            StartAttack(AttackType.Punch);
            _punchCooldownTimer = punchCooldown;
        }
        
        public void TryKick()
        {
            if (_isAttacking || _kickCooldownTimer > 0f) return;
            
            StartAttack(AttackType.Kick);
            _kickCooldownTimer = kickCooldown;
        }
        
        private void StartAttack(AttackType type)
        {
            _isAttacking = true;
            _currentAttack = type;
            
            // Lock movement during attack
            if (_playerController != null)
            {
                _playerController.LockMovement(true);
            }
            
            // Trigger animation
            if (_playerAnimator != null)
            {
                _playerAnimator.TriggerAttack(type);
            }
            
            // Raise attack performed event
            CombatEvents.RaiseAttackPerformed(type);
        }
        
        /// <summary>
        /// Called by animation event at the moment of impact
        /// </summary>
        public void OnAttackHitFrame()
        {
            bool hitLanded = PerformHitDetection();
            CombatEvents.RaiseAttackResult(_currentAttack, hitLanded);
        }
        
        /// <summary>
        /// Called by animation event when attack animation ends
        /// </summary>
        public void OnAttackEnd()
        {
            _isAttacking = false;
            
            if (_playerController != null)
            {
                _playerController.LockMovement(false);
            }
        }
        
        private bool PerformHitDetection()
        {
            Vector3 origin = attackOrigin.position + attackOrigin.forward * 0.5f;
            Vector3 direction = attackOrigin.forward;
            
            // SphereCast for hit detection
            RaycastHit[] hits = Physics.SphereCastAll(
                origin, 
                attackRadius, 
                direction, 
                attackRange, 
                targetLayers
            );
            
            bool anyHit = false;
            
            foreach (RaycastHit hit in hits)
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    float damage = _currentAttack == AttackType.Punch ? punchDamage : kickDamage;
                    float pushback = _currentAttack == AttackType.Punch ? punchPushback : kickPushback;
                    
                    damageable.TakeDamage(damage, _currentAttack, transform.forward * pushback, hit.point);
                    
                    CombatEvents.RaiseHitLanded(_currentAttack, hit.point);
                    anyHit = true;
                }
            }
            
            if (!anyHit)
            {
                CombatEvents.RaiseHitMissed(_currentAttack);
            }
            
            return anyHit;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (attackOrigin == null) return;
            
            Gizmos.color = Color.red;
            Vector3 origin = attackOrigin.position + attackOrigin.forward * 0.5f;
            Gizmos.DrawWireSphere(origin, attackRadius);
            Gizmos.DrawLine(origin, origin + attackOrigin.forward * attackRange);
            Gizmos.DrawWireSphere(origin + attackOrigin.forward * attackRange, attackRadius);
        }
    }
    
    /// <summary>
    /// Interface for objects that can receive damage
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage, AttackType attackType, Vector3 pushback, Vector3 hitPoint);
    }
}
