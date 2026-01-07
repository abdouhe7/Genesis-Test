using System;
using UnityEngine;
using CombatSystem;

namespace CombatDemo.Combat
{
    /// <summary>
    /// Central event system for all combat-related events.
    /// Uses C# events for loose coupling between components.
    /// </summary>
    public static class CombatEvents
    {
        // Attack events
        public static event Action<AttackType> OnAttackPerformed;
        public static event Action<AttackType, bool> OnAttackResult; // bool = hit landed
        public static event Action OnDashPerformed;
        
        // Hit events
        public static event Action<AttackType, Vector3> OnHitLanded;
        public static event Action<AttackType> OnHitMissed;
        
        // Stats update event (for UI/network)
        public static event Action<CombatStatsData> OnStatsUpdated;

        public static void RaiseAttackPerformed(AttackType type)
        {
            OnAttackPerformed?.Invoke(type);
        }

        public static void RaiseAttackResult(AttackType type, bool hitLanded)
        {
            OnAttackResult?.Invoke(type, hitLanded);
        }

        public static void RaiseDashPerformed()
        {
            OnDashPerformed?.Invoke();
        }

        public static void RaiseHitLanded(AttackType type, Vector3 hitPoint)
        {
            OnHitLanded?.Invoke(type, hitPoint);
        }

        public static void RaiseHitMissed(AttackType type)
        {
            OnHitMissed?.Invoke(type);
        }

        public static void RaiseStatsUpdated(CombatStatsData stats)
        {
            OnStatsUpdated?.Invoke(stats);
        }

        /// <summary>
        /// Clear all event subscribers (call on scene unload)
        /// </summary>
        public static void ClearAll()
        {
            OnAttackPerformed = null;
            OnAttackResult = null;
            OnDashPerformed = null;
            OnHitLanded = null;
            OnHitMissed = null;
            OnStatsUpdated = null;
        }
    }

    // Note: AttackType enum is defined in CombatSystem.CombatTypes.cs
    // We use that enum here to avoid duplication

    [Serializable]
    public class CombatStatsData
    {
        public int totalAttacks;
        public int punchCount;
        public int kickCount;
        public int hitsLanded;
        public int hitsMissed;
        public int dashCount;
        public float hitRate; // Percentage 0-100
        public float sessionDuration;
        public string timestamp;

        public CombatStatsData()
        {
            timestamp = DateTime.UtcNow.ToString("o");
        }

        public void CalculateHitRate()
        {
            hitRate = totalAttacks > 0 ? (hitsLanded / (float)totalAttacks) * 100f : 0f;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
