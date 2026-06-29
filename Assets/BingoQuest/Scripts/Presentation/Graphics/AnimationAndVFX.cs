using UnityEngine;
using System.Collections.Generic;

namespace BingoQuest.Presentation.Graphics
{
    /// <summary>
    /// Centralized character animation controller managing all locomotion and combat states.
    /// Works with Animator parameters to drive state machine.
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float animationBlendSpeed = 0.1f;
        [SerializeField] private float footstepVolume = 0.3f;

        private float currentSpeed;
        private bool isGrounded = true;
        private Vector3 lastPosition;

        private const float SpeedSmoothing = 0.1f;

        // Animator parameter hashes (cached for performance)
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int DirectionHash = Animator.StringToHash("Direction");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int CastHash = Animator.StringToHash("Cast");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int DieHash = Animator.StringToHash("Die");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");

        private void Start()
        {
            if (!animator)
                animator = GetComponent<Animator>();

            lastPosition = transform.position;
        }

        private void Update()
        {
            UpdateLocomotion();
        }

        /// <summary>Update movement animations based on current velocity.</summary>
        private void UpdateLocomotion()
        {
            Vector3 displacement = transform.position - lastPosition;
            float speed = displacement.magnitude / Time.deltaTime;

            // Smooth speed for natural animation blending
            currentSpeed = Mathf.Lerp(currentSpeed, speed, SpeedSmoothing);

            animator.SetFloat(SpeedHash, currentSpeed);
            animator.SetBool(IsMovingHash, currentSpeed > 0.1f);

            // Optional: set direction for 8-directional locomotion
            Vector3 direction = displacement.normalized;
            if (direction.sqrMagnitude > 0.01f)
            {
                animator.SetFloat(DirectionHash, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg);
            }

            animator.SetBool(GroundedHash, isGrounded);
            lastPosition = transform.position;
        }

        /// <summary>Play attack animation for the given ability slot.</summary>
        public void PlayAttack(int slotIndex = 0)
        {
            // Attack states: "Attack0" (primary), "Attack1" (secondary), etc.
            animator.SetInteger(AttackHash, slotIndex);
            animator.SetTrigger("AttackTrigger");
        }

        /// <summary>Play casting animation for ability.</summary>
        public void PlayCast(float castDuration)
        {
            animator.SetFloat(CastHash, castDuration);
            animator.SetTrigger("CastTrigger");
        }

        /// <summary>Play hurt/stagger animation (brief knockback effect).</summary>
        public void PlayHurt()
        {
            animator.SetTrigger(HurtHash);
        }

        /// <summary>Play death animation.</summary>
        public void PlayDeath()
        {
            animator.SetTrigger(DieHash);
            // Disable movement after death
            enabled = false;
        }

        /// <summary>Set grounded state for jump/fall animations.</summary>
        public void SetGrounded(bool grounded)
        {
            isGrounded = grounded;
        }

        /// <summary>Get current animation state name.</summary>
        public string GetCurrentStateName()
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return animator.GetLayerName(0) + "." + stateInfo.shortNameHash;
        }

        /// <summary>Check if an animation is currently playing.</summary>
        public bool IsAnimationPlaying(string stateName)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }
    }

    /// <summary>
    /// Particle effect spawner for combat VFX (hits, abilities, environmental effects).
    /// </summary>
    public class VFXSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class VFXDefinition
        {
            public string effectName;
            public ParticleSystem prefab;
            [Range(0f, 5f)] public float lifespan = 2f;
        }

        [SerializeField] private List<VFXDefinition> effectDefinitions = new();
        private Dictionary<string, ParticleSystem> effectPrefabs = new();
        private Transform effectContainer;

        private void Start()
        {
            // Create container for pooled effects
            effectContainer = new GameObject("[VFX Container]").transform;
            effectContainer.SetParent(transform);

            // Index prefabs by name
            foreach (var def in effectDefinitions)
            {
                if (def.prefab)
                    effectPrefabs[def.effectName] = def.prefab;
            }
        }

        /// <summary>Spawn an effect at position with optional rotation.</summary>
        public void SpawnEffect(string effectName, Vector3 position, Quaternion rotation = default)
        {
            if (!effectPrefabs.TryGetValue(effectName, out ParticleSystem prefab))
            {
                Debug.LogWarning($"VFX '{effectName}' not found in definitions");
                return;
            }

            ParticleSystem instance = Instantiate(prefab, position, rotation, effectContainer);
            instance.Play();

            // Auto-destroy after lifespan
            float lifespan = prefab.main.duration + prefab.main.startLifetime.constantMax;
            Destroy(instance.gameObject, lifespan);
        }

        /// <summary>Spawn hit effect at target with direction.</summary>
        public void SpawnHitEffect(Vector3 position, Vector3 hitDirection)
        {
            Quaternion rot = Quaternion.LookRotation(hitDirection);
            SpawnEffect("Hit", position, rot);
        }

        /// <summary>Spawn ability cast effect.</summary>
        public void SpawnCastEffect(Vector3 position, string abilityType = "Generic")
        {
            SpawnEffect($"Cast_{abilityType}", position);
        }

        /// <summary>Spawn environment effect (dust, debris, etc).</summary>
        public void SpawnEnvironmentEffect(string effectType, Vector3 position)
        {
            SpawnEffect($"Environment_{effectType}", position);
        }

        /// <summary>Register a dynamic effect prefab at runtime.</summary>
        public void RegisterEffectPrefab(string name, ParticleSystem prefab)
        {
            effectPrefabs[name] = prefab;
        }
    }

    /// <summary>
    /// Screen-space VFX for damage numbers, status indicators, etc.
    /// </summary>
    public class UIEffectLayer : MonoBehaviour
    {
        [SerializeField] private Canvas worldCanvas;
        [SerializeField] private TextMesh damageNumberPrefab;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float floatDuration = 1f;

        private Transform effectContainer;

        private void Start()
        {
            if (!worldCanvas)
                worldCanvas = GetComponentInChildren<Canvas>();

            effectContainer = new GameObject("[UI Effects]").transform;
            effectContainer.SetParent(worldCanvas.transform);
        }

        /// <summary>Spawn floating damage number at world position.</summary>
        public void SpawnDamageNumber(int damage, Vector3 worldPos, bool isCrit = false)
        {
            if (!damageNumberPrefab)
                return;

            TextMesh text = Instantiate(damageNumberPrefab, effectContainer);
            text.text = damage.ToString();
            text.color = isCrit ? new Color(1f, 0.5f, 0f) : Color.white; // Orange for crits

            // Position in world space (convert to UI coordinates)
            if (worldCanvas.renderMode == RenderMode.WorldSpace)
            {
                text.transform.position = worldPos + Vector3.up * 1f;
            }

            StartCoroutine(FloatAndFade(text.gameObject, floatDuration, floatSpeed));
        }

        /// <summary>Spawn status effect indicator (poison, burning, etc).</summary>
        public void SpawnStatusIndicator(string statusType, Vector3 worldPos)
        {
            // Create simple UI indicator
            GameObject indicator = new GameObject($"Status_{statusType}");
            indicator.transform.SetParent(effectContainer);

            Image image = indicator.AddComponent<Image>();
            image.color = GetStatusColor(statusType);

            RectTransform rect = indicator.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(32, 32);

            StartCoroutine(FloatAndFade(indicator, 2f, 1f));
        }

        private Color GetStatusColor(string statusType)
        {
            return statusType switch
            {
                "Poison" => new Color(0.5f, 1f, 0f),      // Green
                "Burning" => new Color(1f, 0.5f, 0f),     // Orange
                "Frozen" => new Color(0f, 0.8f, 1f),      // Cyan
                "Stunned" => new Color(1f, 1f, 0f),       // Yellow
                _ => Color.white
            };
        }

        private System.Collections.IEnumerator FloatAndFade(GameObject obj, float duration, float speed)
        {
            float elapsed = 0f;
            Vector3 startPos = obj.transform.position;
            CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>() ?? obj.AddComponent<CanvasGroup>();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Float upward
                obj.transform.position = startPos + Vector3.up * (speed * elapsed);

                // Fade out
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

                yield return null;
            }

            Destroy(obj);
        }
    }

    /// <summary>
    /// Tracer effect for projectiles and ranged attacks.
    /// </summary>
    public class ProjectileTracer : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private float speed = 20f;

        private Vector3 targetPosition;
        private float travelTime;
        private bool isActive;

        private void Start()
        {
            if (!lineRenderer)
                lineRenderer = GetComponent<LineRenderer>();
            if (!trailRenderer)
                trailRenderer = GetComponent<TrailRenderer>();
        }

        private void Update()
        {
            if (!isActive) return;

            travelTime -= Time.deltaTime;
            if (travelTime <= 0f)
            {
                OnImpact();
                return;
            }

            // Interpolate toward target
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Update line renderer to show arc
            if (lineRenderer)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, targetPosition);
            }
        }

        /// <summary>Fire projectile from origin to target.</summary>
        public void Fire(Vector3 origin, Vector3 target)
        {
            transform.position = origin;
            targetPosition = target;
            travelTime = Vector3.Distance(origin, target) / speed;
            isActive = true;

            if (trailRenderer)
                trailRenderer.Clear();
        }

        private void OnImpact()
        {
            isActive = false;
            if (lineRenderer)
                lineRenderer.enabled = false;

            // Trigger impact effect
            // (Usually called from ProjectileController or combat system)
            Destroy(gameObject, 1f);
        }
    }
}
