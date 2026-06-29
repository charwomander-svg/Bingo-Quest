using UnityEngine;
using System.Collections.Generic;

namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Centralized ability tuning configuration with per-slot cooldowns, damage scaling, and resource costs.
    /// Enables balance without touching code—designers adjust via Inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "BingoQuest/Ability Config", fileName = "AbilityConfig")]
    public class AbilityConfig : ScriptableObject
    {
        [System.Serializable]
        public class SlotTuning
        {
            [SerializeField] public string SlotName = "Primary";
            [SerializeField][Range(0.1f, 10f)] public float BaseCooldown = 1.0f;
            [SerializeField][Range(0.5f, 3f)] public float DamageMultiplier = 1.0f;
            [SerializeField][Range(0f, 100f)] public int ResourceCost = 0;
            [SerializeField][Range(0f, 1f)] public float CritChanceBonus = 0f;
            [SerializeField] public bool CanBeCastWhileMoving = true;
        }

        [Header("Ability Slot Configuration")]
        [SerializeField] private SlotTuning primarySlot = new() { SlotName = "Primary", BaseCooldown = 0.8f, DamageMultiplier = 1.0f };
        [SerializeField] private SlotTuning secondarySlot = new() { SlotName = "Secondary", BaseCooldown = 2.0f, DamageMultiplier = 1.2f };
        [SerializeField] private SlotTuning tertiarySlot = new() { SlotName = "Tertiary", BaseCooldown = 3.5f, DamageMultiplier = 1.5f };
        [SerializeField] private SlotTuning ultimateSlot = new() { SlotName = "Ultimate", BaseCooldown = 8.0f, DamageMultiplier = 2.5f };

        [Header("Global Ability Modifiers")]
        [SerializeField][Range(0.5f, 2f)] private float globalCooldownMultiplier = 1.0f;
        [SerializeField][Range(0.5f, 2f)] private float globalDamageMultiplier = 1.0f;
        [SerializeField][Range(0f, 1f)] private float haste_cooldown_reduction = 0f; // Passive haste bonus

        [Header("Resource Management")]
        [SerializeField] private int maxResourcePool = 100;
        [SerializeField][Range(0.1f, 10f)] private float resourceRegenPerSecond = 5f;
        [SerializeField] private int resourceCostPerAbilityOverride = 0; // 0 = use per-ability; > 0 = override

        [Header("Status Effect Tuning")]
        [SerializeField][Range(0f, 100f)] private float statusEffectChanceBias = 0f; // +% to all status effect chances
        [SerializeField][Range(0.5f, 2f)] private float statusEffectDurationMultiplier = 1.0f;

        [Header("Cooldown Reset Rules")]
        [SerializeField] private bool resetCooldownsOnKill = false;
        [SerializeField] private bool resetCooldownsOnCritical = false;
        [SerializeField][Range(0f, 1f)] private float cooldownResetChance = 0f; // Random cooldown reset per kill

        [Header("Difficulty Modifiers")]
        [SerializeField][Range(0.5f, 2f)] private float easyCooldownMultiplier = 0.7f;
        [SerializeField][Range(0.5f, 2f)] private float normalCooldownMultiplier = 1.0f;
        [SerializeField][Range(0.5f, 2f)] private float hardCooldownMultiplier = 1.3f;
        [SerializeField][Range(0.5f, 2f)] private float nightmareCooldownMultiplier = 1.6f;

        public SlotTuning GetSlotTuning(AbilitySlot slot) => slot switch
        {
            AbilitySlot.Primary => primarySlot,
            AbilitySlot.Secondary => secondarySlot,
            AbilitySlot.Tertiary => tertiarySlot,
            AbilitySlot.Ultimate => ultimateSlot,
            _ => primarySlot
        };

        public float GetCooldown(AbilitySlot slot, float baseCooldown = -1f)
        {
            var tuning = GetSlotTuning(slot);
            float cooldown = baseCooldown > 0 ? baseCooldown : tuning.BaseCooldown;
            
            // Apply global and haste modifiers
            cooldown *= globalCooldownMultiplier;
            cooldown *= (1f - haste_cooldown_reduction);
            
            return Mathf.Max(0.1f, cooldown); // Minimum 0.1s
        }

        public float GetDamage(AbilitySlot slot, float baseDamage = 1f)
        {
            var tuning = GetSlotTuning(slot);
            return baseDamage * tuning.DamageMultiplier * globalDamageMultiplier;
        }

        public int GetResourceCost(AbilitySlot slot)
        {
            if (resourceCostPerAbilityOverride > 0)
                return resourceCostPerAbilityOverride;
            
            return GetSlotTuning(slot).ResourceCost;
        }

        public float GetCritChanceBonus(AbilitySlot slot)
        {
            return GetSlotTuning(slot).CritChanceBonus;
        }

        public bool CanCastWhileMoving(AbilitySlot slot)
        {
            return GetSlotTuning(slot).CanBeCastWhileMoving;
        }

        public int MaxResourcePool => maxResourcePool;
        public float ResourceRegenPerSecond => resourceRegenPerSecond;
        public float GlobalCooldownMultiplier => globalCooldownMultiplier;
        public float GlobalDamageMultiplier => globalDamageMultiplier;
        public float HasteBonus => haste_cooldown_reduction;
        public float StatusEffectChanceBias => Mathf.Clamp01(statusEffectChanceBias / 100f);
        public float StatusEffectDurationMultiplier => statusEffectDurationMultiplier;

        public bool ResetCooldownsOnKill => resetCooldownsOnKill;
        public bool ResetCooldownsOnCritical => resetCooldownsOnCritical;
        public float CooldownResetChance => Mathf.Clamp01(cooldownResetChance);

        /// <summary>Get cooldown multiplier for a difficulty mode.</summary>
        public float GetDifficultyCooldownMultiplier(Progression.DifficultyMode mode) => mode switch
        {
            Progression.DifficultyMode.Easy => easyCooldownMultiplier,
            Progression.DifficultyMode.Normal => normalCooldownMultiplier,
            Progression.DifficultyMode.Hard => hardCooldownMultiplier,
            Progression.DifficultyMode.Nightmare => nightmareCooldownMultiplier,
            _ => normalCooldownMultiplier
        };

        /// <summary>Calculate final cooldown with all modifiers applied.</summary>
        public float CalculateFinalCooldown(
            AbilitySlot slot,
            float baseCooldown,
            Progression.DifficultyMode difficulty = Progression.DifficultyMode.Normal)
        {
            float cooldown = GetCooldown(slot, baseCooldown);
            float difficultyMult = GetDifficultyCooldownMultiplier(difficulty);
            return cooldown * difficultyMult;
        }

        /// <summary>Validate configuration is sensible.</summary>
        public bool IsValid()
        {
            if (maxResourcePool <= 0) return false;
            if (resourceRegenPerSecond < 0) return false;
            if (primarySlot == null || secondarySlot == null || tertiarySlot == null || ultimateSlot == null) return false;
            
            // At least ultimate should have meaningful cooldown
            if (ultimateSlot.BaseCooldown <= 2f) return false;
            
            return true;
        }

        /// <summary>Get summary of all slot configurations.</summary>
        public override string ToString()
        {
            return $"AbilityConfig\n" +
                $"  Primary: {primarySlot.BaseCooldown:F1}s CD, {primarySlot.DamageMultiplier:F2}x DMG, {primarySlot.ResourceCost} cost\n" +
                $"  Secondary: {secondarySlot.BaseCooldown:F1}s CD, {secondarySlot.DamageMultiplier:F2}x DMG, {secondarySlot.ResourceCost} cost\n" +
                $"  Tertiary: {tertiarySlot.BaseCooldown:F1}s CD, {tertiarySlot.DamageMultiplier:F2}x DMG, {tertiarySlot.ResourceCost} cost\n" +
                $"  Ultimate: {ultimateSlot.BaseCooldown:F1}s CD, {ultimateSlot.DamageMultiplier:F2}x DMG, {ultimateSlot.ResourceCost} cost\n" +
                $"  Global: {globalCooldownMultiplier:F2}x CD, {globalDamageMultiplier:F2}x DMG";
        }
    }
}
