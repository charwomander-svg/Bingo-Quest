using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Objectives;
using UnityEngine;

namespace BingoQuest.Presentation.Graphics
{
    public enum VfxCue
    {
        DamageHit,
        CriticalHit,
        Dodge,
        EnemyDefeated,
        Burn,
        Freeze,
        Poison,
        BingoPattern,
        BingoFullCard
    }

    public readonly struct VfxRequest
    {
        public VfxRequest(VfxCue cue, Color tint, float scale = 1f, float duration = 1.5f)
        {
            Cue = cue;
            Tint = tint;
            Scale = scale;
            Duration = duration;
        }

        public VfxCue Cue { get; }
        public Color Tint { get; }
        public float Scale { get; }
        public float Duration { get; }
    }

    [Serializable]
    public class VfxBinding
    {
        public VfxCue Cue;
        public GameObject Prefab;
    }

    public static class GraphicsEventRouter
    {
        public static List<VfxRequest> FromDamage(DamageResult result, bool targetDefeated)
        {
            var requests = new List<VfxRequest>();
            if (result.WasDodged)
            {
                requests.Add(new VfxRequest(VfxCue.Dodge, new Color(0.8f, 0.8f, 0.8f, 1f), 0.9f, 0.6f));
                return requests;
            }

            var cue = result.IsCritical ? VfxCue.CriticalHit : VfxCue.DamageHit;
            var color = ColorForElement(result.Element);
            float scale = result.IsCritical ? 1.4f : 1f;
            requests.Add(new VfxRequest(cue, color, scale, 1.2f));

            if (targetDefeated)
                requests.Add(new VfxRequest(VfxCue.EnemyDefeated, new Color(1f, 0.7f, 0.2f, 1f), 1.2f, 1.6f));

            return requests;
        }

        public static VfxRequest FromStatus(StatusEffectType effect)
        {
            return effect switch
            {
                StatusEffectType.Burn => new VfxRequest(VfxCue.Burn, new Color(1f, 0.35f, 0.1f, 1f), 1f, 1.3f),
                StatusEffectType.Freeze => new VfxRequest(VfxCue.Freeze, new Color(0.5f, 0.85f, 1f, 1f), 1f, 1.3f),
                StatusEffectType.Poison => new VfxRequest(VfxCue.Poison, new Color(0.45f, 1f, 0.35f, 1f), 1f, 1.3f),
                _ => new VfxRequest(VfxCue.DamageHit, Color.white, 1f, 1f)
            };
        }

        public static VfxRequest FromPattern(BingoPattern pattern)
        {
            return pattern == BingoPattern.FullCard
                ? new VfxRequest(VfxCue.BingoFullCard, new Color(1f, 0.84f, 0.2f, 1f), 1.8f, 2.5f)
                : new VfxRequest(VfxCue.BingoPattern, new Color(0.45f, 1f, 0.65f, 1f), 1.2f, 1.5f);
        }

        private static Color ColorForElement(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => new Color(1f, 0.35f, 0.1f, 1f),
                ElementType.Frost => new Color(0.5f, 0.85f, 1f, 1f),
                ElementType.Lightning => new Color(1f, 0.95f, 0.3f, 1f),
                ElementType.Poison => new Color(0.45f, 1f, 0.35f, 1f),
                ElementType.Holy => new Color(1f, 0.98f, 0.75f, 1f),
                ElementType.Shadow => new Color(0.65f, 0.45f, 0.95f, 1f),
                _ => new Color(0.9f, 0.9f, 0.9f, 1f)
            };
        }
    }

    public class VfxController : MonoBehaviour
    {
        [SerializeField] private List<VfxBinding> bindings = new();

        public void Play(VfxRequest request, Vector3 worldPosition, Transform parent = null)
        {
            var prefab = ResolvePrefab(request.Cue);
            if (prefab == null)
                return;

            var instance = Instantiate(prefab, worldPosition, Quaternion.identity, parent);
            instance.transform.localScale = instance.transform.localScale * Mathf.Max(0.1f, request.Scale);
            ApplyTint(instance, request.Tint);
            Destroy(instance, Mathf.Max(0.1f, request.Duration));
        }

        private GameObject ResolvePrefab(VfxCue cue)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i] != null && bindings[i].Cue == cue)
                    return bindings[i].Prefab;
            }

            return null;
        }

        private static void ApplyTint(GameObject instance, Color tint)
        {
            var sprite = instance.GetComponentInChildren<SpriteRenderer>();
            if (sprite != null)
                sprite.color = tint;

            var particles = instance.GetComponentInChildren<ParticleSystem>();
            if (particles != null)
            {
                var main = particles.main;
                main.startColor = tint;
            }
        }
    }

    public class CombatGraphicsBridge : MonoBehaviour
    {
        [SerializeField] private Combatant combatant;
        [SerializeField] private VfxController vfxController;

        private void OnEnable()
        {
            if (combatant == null || vfxController == null)
                return;

            combatant.OnDamageTaken += OnDamageTaken;
            combatant.OnStatusEffectApplied += OnStatusEffectApplied;
            combatant.OnDefeated += OnDefeated;
        }

        private void OnDisable()
        {
            if (combatant == null)
                return;

            combatant.OnDamageTaken -= OnDamageTaken;
            combatant.OnStatusEffectApplied -= OnStatusEffectApplied;
            combatant.OnDefeated -= OnDefeated;
        }

        private void OnDamageTaken(Combatant source, DamageResult result)
        {
            var requests = GraphicsEventRouter.FromDamage(result, targetDefeated: false);
            for (int i = 0; i < requests.Count; i++)
                vfxController.Play(requests[i], transform.position, transform);
        }

        private void OnStatusEffectApplied(StatusEffectType effect)
        {
            var request = GraphicsEventRouter.FromStatus(effect);
            vfxController.Play(request, transform.position, transform);
        }

        private void OnDefeated()
        {
            var request = new VfxRequest(VfxCue.EnemyDefeated, new Color(1f, 0.7f, 0.2f, 1f), 1.2f, 1.6f);
            vfxController.Play(request, transform.position, transform);
        }
    }

    public class BingoGraphicsBridge : MonoBehaviour
    {
        [SerializeField] private BingoSystem bingoSystem;
        [SerializeField] private VfxController vfxController;
        [SerializeField] private Transform boardCenter;

        private void OnEnable()
        {
            if (bingoSystem == null)
                bingoSystem = BingoSystem.Instance;

            if (bingoSystem == null || vfxController == null)
                return;

            bingoSystem.OnPatternDetected += OnPatternDetected;
        }

        private void OnDisable()
        {
            if (bingoSystem != null)
                bingoSystem.OnPatternDetected -= OnPatternDetected;
        }

        private void OnPatternDetected(BingoPattern pattern)
        {
            var request = GraphicsEventRouter.FromPattern(pattern);
            var center = boardCenter != null ? boardCenter.position : transform.position;
            vfxController.Play(request, center, boardCenter);
        }
    }
}
