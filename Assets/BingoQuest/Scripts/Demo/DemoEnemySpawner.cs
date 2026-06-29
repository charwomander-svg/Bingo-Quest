using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Combat;
using UnityEngine;

namespace BingoQuest.Demo
{
    public class DemoEnemySpawner : MonoBehaviour
    {
        [SerializeField] private float spawnInterval = 2.5f;
        [SerializeField] private int maxAliveEnemies = 8;
        [SerializeField] private float spawnRadius = 14f;

        private readonly List<Combatant> activeEnemies = new List<Combatant>();
        private Combatant player;
        private Action<Combatant> onEnemySpawned;
        private float timer;
        private bool active;

        public void Initialize(Combatant playerCombatant, Action<Combatant> enemySpawned)
        {
            player = playerCombatant;
            onEnemySpawned = enemySpawned;
        }

        public void Begin()
        {
            active = true;
            timer = 0f;
        }

        private void Update()
        {
            if (!active || player == null)
                return;

            CleanupDeadEnemies();
            timer -= Time.deltaTime;
            if (timer > 0f)
                return;

            timer = spawnInterval;
            if (activeEnemies.Count < maxAliveEnemies)
                SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;
            var position = player.transform.position + offset;
            position.y = 1f;

            var enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = "DemoEnemy";
            enemy.transform.position = position;
            enemy.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            var combatant = enemy.AddComponent<Combatant>();
            var brain = enemy.AddComponent<DemoEnemyBrain>();
            brain.Initialize(combatant, player);

            activeEnemies.Add(combatant);
            onEnemySpawned?.Invoke(combatant);
        }

        public Combatant GetNearestAliveEnemy(Vector3 position)
        {
            Combatant nearest = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < activeEnemies.Count; i++)
            {
                var enemy = activeEnemies[i];
                if (enemy == null || !enemy.IsAlive)
                    continue;

                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        public int GetAliveEnemyCount()
        {
            int alive = 0;
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                if (activeEnemies[i] != null && activeEnemies[i].IsAlive)
                    alive++;
            }

            return alive;
        }

        private void CleanupDeadEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = activeEnemies[i];
                if (enemy == null)
                {
                    activeEnemies.RemoveAt(i);
                    continue;
                }

                if (!enemy.IsAlive)
                {
                    Destroy(enemy.gameObject, 0.75f);
                    activeEnemies.RemoveAt(i);
                }
            }
        }
    }

    public class DemoEnemyBrain : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float attackRange = 1.7f;
        [SerializeField] private float attackCooldown = 1.4f;

        private Combatant combatant;
        private Combatant player;
        private float attackTimer;

        public void Initialize(Combatant self, Combatant playerTarget)
        {
            combatant = self;
            player = playerTarget;
            attackTimer = 0f;
        }

        private void Update()
        {
            if (combatant == null || player == null || !combatant.IsAlive || !player.IsAlive)
                return;

            var direction = player.transform.position - transform.position;
            direction.y = 0f;
            float distance = direction.magnitude;

            if (distance > attackRange)
            {
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;
                return;
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                attackTimer = attackCooldown;
                combatant.AutoAttack(player);
            }
        }
    }
}
