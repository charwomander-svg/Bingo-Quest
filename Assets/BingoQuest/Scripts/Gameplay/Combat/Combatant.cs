using BingoQuest.Gameplay.Objectives;
using System;
using UnityEngine;

namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Represents a combatant (player or enemy) with stats, abilities, and status effects.
    /// Core entity in the combat system.
    /// </summary>
    public class Combatant : MonoBehaviour
    {
        [SerializeField] private string combatantId;
        [SerializeField] private bool isPlayer = false;

        private CharacterStats stats;
        private ActionBar actionBar;
        private StatusEffectManager statusEffects;
        private DamageCalculator damageCalculator = new();
        private float nextAutoAttackTime;

        public string CombatantId => combatantId;
        public bool IsPlayer => isPlayer;
        public bool IsAlive => stats.IsAlive;
        public CharacterStats Stats => stats;
        public ActionBar ActionBar => actionBar;
        public StatusEffectManager StatusEffects => statusEffects;

        public event Action<int> OnHealthChanged;
        public event Action OnDefeated;
        public event Action<StatusEffectType> OnStatusEffectApplied;
        public event Action<Combatant, DamageResult> OnDamageTaken;
        public event Action<Combatant, DamageResult> OnDamageDealt;

        private void Start()
        {
            stats = new CharacterStats();
            actionBar = new ActionBar();
            statusEffects = new StatusEffectManager();
            nextAutoAttackTime = 0;

            // Auto-register for status effect display in case we want to visualize
            if (!isPlayer)
                gameObject.name = $"Enemy({combatantId})";
        }

        private void Update()
        {
            if (!IsAlive) return;

            // Cooldown reduction
            actionBar.ReduceAllCooldowns(Time.deltaTime);

            // Status effect ticking (DoT damage)
            float dotDamage = statusEffects.TickAndGetDamage(Time.deltaTime);
            if (dotDamage > 0)
            {
                TakeDamage((int)dotDamage, null, isObjective: false);
            }

            // Auto-attack timer (for AI)
            if (!isPlayer && nextAutoAttackTime <= 0)
            {
                // Would call combat system to find and attack nearest player
                nextAutoAttackTime = 2.0f;
            }
            nextAutoAttackTime -= Time.deltaTime;
        }

        /// <summary>Execute an ability if it's ready.</summary>
        public bool ExecuteAbility(AbilitySlot slot, Combatant target)
        {
            var ability = actionBar.GetAbility(slot);
            if (ability == null || !ability.TryExecute())
                return false;

            var abilityDef = ability.Definition;
            var damage = damageCalculator.CalculateDamage(stats, target.stats, abilityDef);

            // Apply damage
            target.TakeDamage(damage.FinalDamage, this, isObjective: true, wasCrit: damage.IsCritical);

            // Apply status effect if applicable
            if (abilityDef.AppliesStatusEffect && UnityEngine.Random.Range(0, 100) < abilityDef.StatusEffectChance)
            {
                target.ApplyStatusEffect(abilityDef.StatusEffectType, abilityDef.ElementType);
            }

            // Emit event for objective system
            if (damage.IsCritical)
                ObjectiveEventBus.Instance.Emit(ObjectiveEvent.CriticalHit(CombatantId, damage.FinalDamage));
            else
                ObjectiveEventBus.Instance.Emit(ObjectiveEvent.Damage(CombatantId, damage.FinalDamage, abilityDef.ElementType));

            ObjectiveEventBus.Instance.Emit(ObjectiveEvent.AbilityUsed(abilityDef.Id));

            OnDamageDealt?.Invoke(target, damage);
            return true;
        }

        /// <summary>Execute a basic auto-attack.</summary>
        public void AutoAttack(Combatant target)
        {
            var damage = damageCalculator.CalculateAutoAttack(stats, target.stats);
            target.TakeDamage(damage.FinalDamage, this, isObjective: true, wasCrit: damage.IsCritical);

            if (damage.IsCritical)
                ObjectiveEventBus.Instance.Emit(ObjectiveEvent.CriticalHit(CombatantId, damage.FinalDamage));
            else
                ObjectiveEventBus.Instance.Emit(ObjectiveEvent.Damage(CombatantId, damage.FinalDamage, ElementType.Physical));

            OnDamageDealt?.Invoke(target, damage);
        }

        /// <summary>Take damage from a source (with objective tracking).</summary>
        public void TakeDamage(int amount, Combatant attacker, bool isObjective = true, bool wasCrit = false)
        {
            if (!IsAlive) return;

            // Status effects reduce damage taken (Shock debuff)
            if (statusEffects.HasEffect(StatusEffectType.Shock))
                amount = (int)(amount * 1.25f); // Take 25% more damage when shocked

            stats.TakeDamage(amount);
            OnHealthChanged?.Invoke(stats.Health);
            
            var result = new DamageResult
            {
                FinalDamage = amount,
                IsCritical = wasCrit
            };
            OnDamageTaken?.Invoke(attacker, result);

            if (isObjective && attacker != null)
            {
                ObjectiveEventBus.Instance.Emit(ObjectiveEvent.Damage(attacker.CombatantId, amount));
            }

            if (!IsAlive)
            {
                OnDefeated?.Invoke();
                if (attacker != null)
                {
                    ObjectiveEventBus.Instance.Emit(ObjectiveEvent.Kill(CombatantId));
                    attacker.OnKill(this);
                }
            }
        }

        /// <summary>Apply a status effect to this combatant.</summary>
        public void ApplyStatusEffect(StatusEffectType type, ElementType element)
        {
            float duration = 3.0f; // Default duration
            float dps = 0;

            switch (type)
            {
                case StatusEffectType.Burn:
                    dps = 5.0f;
                    break;
                case StatusEffectType.Poison:
                    dps = 4.0f;
                    break;
                case StatusEffectType.Bleed:
                    dps = 3.0f;
                    break;
            }

            statusEffects.ApplyEffect(type, duration, dps);
            OnStatusEffectApplied?.Invoke(type);
            
            ObjectiveEventBus.Instance.Emit(ObjectiveEvent.StatusEffectApplied(type.ToString(), element));
        }

        /// <summary>Dodge an incoming attack.</summary>
        public bool AttemptDodge()
        {
            bool dodged = UnityEngine.Random.Range(0, 1.0f) < stats.DodgeChance;
            if (dodged)
            {
                ObjectiveEventBus.Instance.Emit(ObjectiveEvent.DodgeAction());
            }
            return dodged;
        }

        /// <summary>Called when this combatant defeats another.</summary>
        private void OnKill(Combatant defeated)
        {
            // Could add victory state, loot dropping, etc.
        }

        /// <summary>Heal this combatant.</summary>
        public void Heal(int amount)
        {
            stats.Heal(amount);
            OnHealthChanged?.Invoke(stats.Health);
        }

        /// <summary>Reset for new encounter.</summary>
        public void ResetForNewEncounter()
        {
            stats.ResetToMaxHealth();
            statusEffects.ClearAllEffects();
            nextAutoAttackTime = 0;

            foreach (var slot in System.Enum.GetValues(typeof(AbilitySlot)) as AbilitySlot[])
            {
                actionBar.GetAbility(slot)?.ResetCooldown();
            }
        }

        public override string ToString() =>
            $"{CombatantId}({(IsPlayer ? "Player" : "Enemy")}): {stats}";
    }
}
