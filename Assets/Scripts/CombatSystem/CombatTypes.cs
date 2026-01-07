using UnityEngine;

namespace CombatSystem
{
    public enum AttackType 
    { 
        Punch, 
        Kick 
    }

    public interface IDamageable
    {
        void TakeDamage(float damage, AttackType attackType, Vector3 pushDirection);
    }
}
