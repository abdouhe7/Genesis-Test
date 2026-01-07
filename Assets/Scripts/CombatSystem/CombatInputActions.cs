using UnityEngine;
using UnityEngine.InputSystem;

namespace CombatSystem
{
    /// <summary>
    /// Extends the StarterAssets input to include combat actions.
    /// Add this component alongside StarterAssetsInputs.
    /// </summary>
    public class CombatInputActions : MonoBehaviour
    {
        [Header("Combat Input Values")]
        public bool punch;
        public bool kick;
        public bool dodge;
        
        [Header("Settings")]
        [SerializeField] private bool cursorLocked = true;
        [SerializeField] private bool cursorInputForLook = true;
        
        // References to combat system
        private ThirdPersonCombatBridge _combatBridge;
        
        private void Awake()
        {
            _combatBridge = GetComponent<ThirdPersonCombatBridge>();
        }
        
        private void Start()
        {
            SetCursorState(cursorLocked);
        }
        
        #region Input Callbacks
        
        public void OnPunch(InputValue value)
        {
            PunchInput(value.isPressed);
            if (value.isPressed && _combatBridge != null)
            {
                _combatBridge.OnPunch(value);
            }
        }
        
        public void OnKick(InputValue value)
        {
            KickInput(value.isPressed);
            if (value.isPressed && _combatBridge != null)
            {
                _combatBridge.OnKick(value);
            }
        }
        
        public void OnDodge(InputValue value)
        {
            DodgeInput(value.isPressed);
            if (value.isPressed && _combatBridge != null)
            {
                _combatBridge.OnDodge(value);
            }
        }

        // Map Jump action (Space) to Dodge as requested
        public void OnJump(InputValue value)
        {
            DodgeInput(value.isPressed);
            if (value.isPressed && _combatBridge != null)
            {
                _combatBridge.OnDodge(value);
            }
        }
        
        #endregion
        
        #region Input Handlers
        
        public void PunchInput(bool newPunchState)
        {
            punch = newPunchState;
        }
        
        public void KickInput(bool newKickState)
        {
            kick = newKickState;
        }
        
        public void DodgeInput(bool newDodgeState)
        {
            dodge = newDodgeState;
        }
        
        #endregion
        
        #region Cursor
        
        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }
        
        #endregion
    }
}
