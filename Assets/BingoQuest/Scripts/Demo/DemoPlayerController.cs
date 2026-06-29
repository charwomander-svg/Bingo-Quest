using System;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Objectives;
using UnityEngine;

namespace BingoQuest.Demo
{
    public class DemoPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;

        private Combatant combatant;
        private Func<Combatant> targetResolver;
        private Action openChestAction;
        private Action travelNextRegionAction;

        public void Initialize(Combatant source, Func<Combatant> resolveTarget, Action openChest, Action travelNextRegion = null)
        {
            combatant = source;
            targetResolver = resolveTarget;
            openChestAction = openChest;
            travelNextRegionAction = travelNextRegion;
        }

        private void Update()
        {
            if (combatant == null || !combatant.IsAlive)
                return;

            HandleMovement();
            HandleActions();
        }

        private void HandleMovement()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            var move = new Vector3(x, 0f, z).normalized;
            transform.position += move * moveSpeed * Time.deltaTime;
        }

        private void HandleActions()
        {
            var target = targetResolver?.Invoke();
            bool hasTarget = target != null && target.IsAlive;

            if (hasTarget && Input.GetKeyDown(KeyCode.Space))
                combatant.AutoAttack(target);

            if (hasTarget && Input.GetKeyDown(KeyCode.Q))
                combatant.ExecuteAbility(AbilitySlot.Primary, target);
            if (hasTarget && Input.GetKeyDown(KeyCode.W))
                combatant.ExecuteAbility(AbilitySlot.Secondary, target);
            if (hasTarget && Input.GetKeyDown(KeyCode.E))
                combatant.ExecuteAbility(AbilitySlot.Tertiary, target);
            if (hasTarget && Input.GetKeyDown(KeyCode.R))
                combatant.ExecuteAbility(AbilitySlot.Ultimate, target);

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                combatant.AttemptDodge();

            if (Input.GetKeyDown(KeyCode.L))
                openChestAction?.Invoke();

            if (Input.GetKeyDown(KeyCode.B))
                ObjectiveEventBus.Instance?.Emit(ObjectiveEvent.BossDefeated("demo_boss"));

            if (Input.GetKeyDown(KeyCode.N))
                travelNextRegionAction?.Invoke();
        }
    }
}
