using BingoQuest.Gameplay.Objectives;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Enumeration of possible status effects in combat.
    /// </summary>
    public enum StatusEffectType
    {
        Burn,      // DoT fire damage
        Freeze,    // Slows attack speed
        Poison,    // DoT toxic damage
        Bleed,     // DoT physical damage
        Shock,     // Increases damage taken
        Curse,     // Reduces stats temporarily
    }

    /// <summary>
    /// Active status effect on a character.
    /// </summary>
    public class StatusEffect
    {
        public StatusEffectType Type { get; set; }
        public int StackCount { get; set; } = 1;
        public float RemainingDuration { get; set; }
        public float DamagePerSecond { get; set; }
        public float StatModifier { get; set; } = 1.0f; // For slows, debuffs

        public bool IsExpired => RemainingDuration <= 0;

        public StatusEffect(StatusEffectType type, float duration, float dps = 0)
        {
            Type = type;
            RemainingDuration = duration;
            DamagePerSecond = dps;
        }

        public void Tick(float deltaTime)
        {
            RemainingDuration -= deltaTime;
        }

        public override string ToString() =>
            $"{Type} (Stacks: {StackCount}, Duration: {RemainingDuration:F1}s)";
    }

    /// <summary>
    /// Manages active status effects on a character.
    /// </summary>
    public class StatusEffectManager
    {
        private Dictionary<StatusEffectType, StatusEffect> activeEffects = new();

        public IReadOnlyDictionary<StatusEffectType, StatusEffect> ActiveEffects => activeEffects;

        public void ApplyEffect(StatusEffectType type, float duration, float dps = 0)
        {
            if (activeEffects.TryGetValue(type, out var existing))
            {
                // Stack existing effect (re-apply duration and add stacks)
                existing.StackCount++;
                existing.RemainingDuration = Mathf.Max(existing.RemainingDuration, duration);
                existing.DamagePerSecond = Mathf.Max(existing.DamagePerSecond, dps);
            }
            else
            {
                // New effect
                activeEffects[type] = new StatusEffect(type, duration, dps);
            }
        }

        public void RemoveEffect(StatusEffectType type)
        {
            activeEffects.Remove(type);
        }

        public void ClearAllEffects()
        {
            activeEffects.Clear();
        }

        public bool HasEffect(StatusEffectType type) => activeEffects.ContainsKey(type);

        public float GetStatModifier()
        {
            float modifier = 1.0f;
            foreach (var effect in activeEffects.Values)
            {
                // Freeze slows attack speed
                if (effect.Type == StatusEffectType.Freeze)
                    modifier *= Mathf.Min(0.5f, effect.StatModifier);

                // Curse reduces offensive power
                if (effect.Type == StatusEffectType.Curse)
                    modifier *= Mathf.Min(0.75f, effect.StatModifier);

                // Shock increases damage taken (handled elsewhere)
            }
            return modifier;
        }

        public float TickAndGetDamage(float deltaTime)
        {
            float totalDamage = 0;
            var expiredEffects = new List<StatusEffectType>();

            foreach (var (type, effect) in activeEffects)
            {
                effect.Tick(deltaTime);
                
                if (effect.DamagePerSecond > 0)
                    totalDamage += effect.DamagePerSecond * deltaTime;

                if (effect.IsExpired)
                    expiredEffects.Add(type);
            }

            foreach (var type in expiredEffects)
                activeEffects.Remove(type);

            return totalDamage;
        }

        public override string ToString()
        {
            if (activeEffects.Count == 0)
                return "No active effects";

            var sb = new System.Text.StringBuilder();
            foreach (var effect in activeEffects.Values)
                sb.AppendLine(effect.ToString());
            return sb.ToString();
        }
    }
}
