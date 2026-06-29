using BingoQuest.Gameplay.Objectives;
using UnityEngine;

namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Data structure for ability definitions (typically authored in ScriptableObjects).
    /// </summary>
    public class AbilityDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public float Cooldown { get; set; } = 0.5f;
        public float DamageScale { get; set; } = 1.0f;
        public ElementType ElementType { get; set; } = ElementType.Physical;
        
        public bool AppliesStatusEffect { get; set; }
        public StatusEffectType StatusEffectType { get; set; }
        public int StatusEffectChance { get; set; } // 0-100
        
        public bool IsAOE { get; set; }
        public float AOERadius { get; set; } = 5.0f;
        
        public bool HasInvulnerabilityWindow { get; set; }
        public float InvulnerabilityDuration { get; set; }

        public override string ToString() => $"{Name} ({DamageScale}x DMG, {Cooldown}s CD)";
    }

    /// <summary>
    /// Core ability execution and cooldown management.
    /// </summary>
    public class Ability
    {
        public AbilityDefinition Definition { get; }
        public float RemainingCooldown { get; private set; }
        public bool IsReady => RemainingCooldown <= 0;

        public Ability(AbilityDefinition definition)
        {
            Definition = definition;
            RemainingCooldown = 0;
        }

        public bool TryExecute()
        {
            if (!IsReady)
                return false;

            RemainingCooldown = Definition.Cooldown;
            return true;
        }

        public void ReduceCooldown(float deltaTime)
        {
            if (RemainingCooldown > 0)
                RemainingCooldown = Mathf.Max(0, RemainingCooldown - deltaTime);
        }

        public void ResetCooldown() => RemainingCooldown = 0;

        public float CooldownPercent => Mathf.Clamp01(
            1.0f - (RemainingCooldown / Definition.Cooldown)
        );

        public override string ToString() =>
            $"{Definition.Name}: {(IsReady ? "READY" : $"{RemainingCooldown:F2}s")}";
    }

    /// <summary>
    /// Slot for active combat ability.
    /// </summary>
    public enum AbilitySlot
    {
        Primary = 0,
        Secondary = 1,
        Tertiary = 2,
        Ultimate = 3
    }

    /// <summary>
    /// Character action bar with 4 active abilities + class passive.
    /// </summary>
    public class ActionBar
    {
        private Ability[] abilities = new Ability[4];
        public string PassiveAbilityId { get; set; }

        public ActionBar() { }

        public void SetAbility(AbilitySlot slot, Ability ability)
        {
            abilities[(int)slot] = ability;
        }

        public Ability GetAbility(AbilitySlot slot) => abilities[(int)slot];

        public void ReduceAllCooldowns(float deltaTime)
        {
            for (int i = 0; i < 4; i++)
            {
                abilities[i]?.ReduceCooldown(deltaTime);
            }
        }

        public float GetCooldownPercent(AbilitySlot slot) =>
            GetAbility(slot)?.CooldownPercent ?? 0;
    }
}

