using UnityEngine;
using CombatSystem;

namespace CombatDemo.Dummy
{
    /// <summary>
    /// Controls the training dummy's hit reactions and behavior.
    /// Implements IDamageable to receive damage from player attacks.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class DummyController : MonoBehaviour, IDamageable
    {
        [Header("Hit Reaction Settings")]
        [SerializeField] private float pushbackRecoverySpeed = 5f;
        [SerializeField] private float hitStunDuration = 0.5f;
        [SerializeField] private bool returnToOrigin = true;
        
        [Header("Animation Parameters")]
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string hitDirectionParam = "HitDirection";
        [SerializeField] private string hitTypeParam = "HitType";
        
        [Header("Visual Feedback")]
        [SerializeField] private float hitFlashDuration = 0.1f;
        [SerializeField] private Color hitFlashColor = Color.red;
        
        [Header("Audio")]
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private AudioSource audioSource;
        
        private Animator _animator;
        private CharacterController _characterController;
        private Renderer[] _renderers;
        private Color[] _originalColors;
        
        private Vector3 _originPosition;
        private Quaternion _originRotation;
        private Vector3 _currentPushback;
        private float _hitStunTimer;
        private bool _isStunned;
        
        // Cached hashes
        private int _hitHash;
        private int _hitDirHash;
        private int _hitTypeHash;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _characterController = GetComponent<CharacterController>();
            _renderers = GetComponentsInChildren<Renderer>();
            
            CacheOriginalColors();
            CacheAnimatorHashes();
            
            _originPosition = transform.position;
            _originRotation = transform.rotation;
            
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }
        
        private void CacheOriginalColors()
        {
            _originalColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i].material.HasProperty("_Color"))
                {
                    _originalColors[i] = _renderers[i].material.color;
                }
            }
        }
        
        private void CacheAnimatorHashes()
        {
            _hitHash = Animator.StringToHash(hitTrigger);
            _hitDirHash = Animator.StringToHash(hitDirectionParam);
            _hitTypeHash = Animator.StringToHash(hitTypeParam);
        }
        
        private void Update()
        {
            UpdateHitStun();
            UpdatePushback();
            
            if (returnToOrigin && !_isStunned && _currentPushback.magnitude < 0.01f)
            {
                ReturnToOrigin();
            }
        }
        
        private void UpdateHitStun()
        {
            if (_hitStunTimer > 0f)
            {
                _hitStunTimer -= Time.deltaTime;
                if (_hitStunTimer <= 0f)
                {
                    _isStunned = false;
                }
            }
        }
        
        private void UpdatePushback()
        {
            if (_currentPushback.magnitude > 0.01f)
            {
                if (_characterController != null)
                {
                    _characterController.Move(_currentPushback * Time.deltaTime);
                }
                else
                {
                    transform.position += _currentPushback * Time.deltaTime;
                }
                
                _currentPushback = Vector3.Lerp(_currentPushback, Vector3.zero, pushbackRecoverySpeed * Time.deltaTime);
            }
        }
        
        private void ReturnToOrigin()
        {
            float distance = Vector3.Distance(transform.position, _originPosition);
            if (distance > 0.1f)
            {
                Vector3 direction = (_originPosition - transform.position).normalized;
                float speed = Mathf.Min(pushbackRecoverySpeed, distance);
                
                if (_characterController != null)
                {
                    _characterController.Move(direction * speed * Time.deltaTime);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, _originPosition, speed * Time.deltaTime);
                }
            }
            
            transform.rotation = Quaternion.Slerp(transform.rotation, _originRotation, pushbackRecoverySpeed * Time.deltaTime);
        }
        
        public void TakeDamage(float damage, AttackType attackType, Vector3 pushDirection)
        {
            Debug.Log($"[DummyController] Took {damage} damage from {attackType}!");

            _isStunned = true;
            _hitStunTimer = hitStunDuration;
            _currentPushback = pushDirection;

            // Play hit animation
            PlayHitAnimation(attackType, pushDirection);

            // Visual feedback
            StartCoroutine(HitFlashCoroutine());

            // Audio feedback
            PlayHitSound();

            // Spawn hit effect at approximate hit point
            Vector3 hitPoint = transform.position + Vector3.up + pushDirection.normalized * 0.5f;
            SpawnHitEffect(hitPoint, attackType);
        }
        
        private void PlayHitAnimation(AttackType attackType, Vector3 pushback)
        {
            // Determine hit direction relative to dummy
            Vector3 localPushback = transform.InverseTransformDirection(pushback.normalized);
            float hitDirection = Mathf.Atan2(localPushback.x, localPushback.z) * Mathf.Rad2Deg;
            
            _animator.SetFloat(_hitDirHash, hitDirection);
            _animator.SetInteger(_hitTypeHash, (int)attackType);
            _animator.SetTrigger(_hitHash);
        }
        
        private System.Collections.IEnumerator HitFlashCoroutine()
        {
            // Flash to hit color
            foreach (Renderer rend in _renderers)
            {
                if (rend.material.HasProperty("_Color"))
                {
                    rend.material.color = hitFlashColor;
                }
            }
            
            yield return new WaitForSeconds(hitFlashDuration);
            
            // Return to original color
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i].material.HasProperty("_Color"))
                {
                    _renderers[i].material.color = _originalColors[i];
                }
            }
        }
        
        private void PlayHitSound()
        {
            if (audioSource != null && hitSounds != null && hitSounds.Length > 0)
            {
                AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
                audioSource.PlayOneShot(clip);
            }
        }
        
        private void SpawnHitEffect(Vector3 hitPoint, AttackType attackType)
        {
            // Override this in derived classes or use a VFX manager
            // Example: Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
        }
        
        /// <summary>
        /// Reset dummy to original state
        /// </summary>
        public void ResetDummy()
        {
            transform.position = _originPosition;
            transform.rotation = _originRotation;
            _currentPushback = Vector3.zero;
            _isStunned = false;
            _hitStunTimer = 0f;
        }
        
        /// <summary>
        /// Set new origin position
        /// </summary>
        public void SetOrigin(Vector3 position, Quaternion rotation)
        {
            _originPosition = position;
            _originRotation = rotation;
        }
    }
}
