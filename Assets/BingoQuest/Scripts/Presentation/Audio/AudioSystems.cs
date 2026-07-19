using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Objectives;
using UnityEngine;

namespace BingoQuest.Presentation.Audio
{
    public enum AudioCue
    {
        DamageHit,
        CriticalHit,
        Dodge,
        EnemyDefeated,
        StatusBurn,
        StatusFreeze,
        StatusPoison,
        BingoPattern,
        BingoFullCard,
        UiOpenCard,
        UiConfirm,
        AmbientLoop
    }

    public readonly struct AudioRequest
    {
        public AudioRequest(AudioCue cue, float volume = 1f, float pitch = 1f)
        {
            Cue = cue;
            Volume = volume;
            Pitch = pitch;
        }

        public AudioCue Cue { get; }
        public float Volume { get; }
        public float Pitch { get; }
    }

    [Serializable]
    public class AudioBinding
    {
        public AudioCue Cue;
        public AudioClip Clip;
    }

    public static class AudioEventRouter
    {
        public static List<AudioRequest> FromDamage(DamageResult result, bool targetDefeated)
        {
            var requests = new List<AudioRequest>();
            if (result.WasDodged)
            {
                requests.Add(new AudioRequest(AudioCue.Dodge, 0.8f, 1.05f));
                return requests;
            }

            requests.Add(result.IsCritical
                ? new AudioRequest(AudioCue.CriticalHit, 1f, 1.05f)
                : new AudioRequest(AudioCue.DamageHit, 0.9f, 1f));

            if (targetDefeated)
                requests.Add(new AudioRequest(AudioCue.EnemyDefeated, 1f, 1f));

            return requests;
        }

        public static AudioRequest FromStatus(StatusEffectType effect)
        {
            return effect switch
            {
                StatusEffectType.Burn => new AudioRequest(AudioCue.StatusBurn, 0.9f, 1f),
                StatusEffectType.Freeze => new AudioRequest(AudioCue.StatusFreeze, 0.9f, 1f),
                StatusEffectType.Poison => new AudioRequest(AudioCue.StatusPoison, 0.9f, 1f),
                _ => new AudioRequest(AudioCue.DamageHit, 0.7f, 1f)
            };
        }

        public static AudioRequest FromPattern(BingoPattern pattern)
        {
            return pattern == BingoPattern.FullCard
                ? new AudioRequest(AudioCue.BingoFullCard, 1f, 1f)
                : new AudioRequest(AudioCue.BingoPattern, 0.95f, 1f);
        }

        public static AudioRequest ForOpenCardUi() => new AudioRequest(AudioCue.UiOpenCard, 0.8f, 1f);
        public static AudioRequest ForConfirmUi() => new AudioRequest(AudioCue.UiConfirm, 0.8f, 1f);
    }

    public class AudioSystem : MonoBehaviour
    {
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private List<AudioBinding> bindings = new();
        [SerializeField] private float pitchVariance = 0.04f;

        public void Play(AudioRequest request)
        {
            if (sfxSource == null)
                return;

            var clip = ResolveClip(request.Cue);
            if (clip == null)
            {
                Debug.LogWarning($"No clip configured for cue {request.Cue}");
                return;
            }

            float variance = UnityEngine.Random.Range(-pitchVariance, pitchVariance);
            sfxSource.pitch = Mathf.Clamp(request.Pitch + variance, 0.5f, 2f);
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(request.Volume));
        }

        public void PlayMusicLoop(AudioCue cue, float volume = 0.55f)
        {
            if (musicSource == null)
                return;

            var clip = ResolveClip(cue);
            if (clip == null)
                return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = Mathf.Clamp01(volume);
            if (!musicSource.isPlaying)
                musicSource.Play();
        }

        private AudioClip ResolveClip(AudioCue cue)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i] != null && bindings[i].Cue == cue)
                    return bindings[i].Clip;
            }

            return null;
        }
    }

    public class CombatAudioBridge : MonoBehaviour
    {
        [SerializeField] private Combatant combatant;
        [SerializeField] private AudioSystem audioSystem;

        private void OnEnable()
        {
            if (combatant == null || audioSystem == null)
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
            var requests = AudioEventRouter.FromDamage(result, targetDefeated: false);
            for (int i = 0; i < requests.Count; i++)
                audioSystem.Play(requests[i]);
        }

        private void OnStatusEffectApplied(StatusEffectType effect)
        {
            audioSystem.Play(AudioEventRouter.FromStatus(effect));
        }

        private void OnDefeated()
        {
            audioSystem.Play(new AudioRequest(AudioCue.EnemyDefeated, 1f, 1f));
        }
    }

    public class BingoAudioBridge : MonoBehaviour
    {
        [SerializeField] private BingoSystem bingoSystem;
        [SerializeField] private AudioSystem audioSystem;

        private void OnEnable()
        {
            if (bingoSystem == null)
                bingoSystem = BingoSystem.Instance;

            if (bingoSystem == null || audioSystem == null)
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
            audioSystem.Play(AudioEventRouter.FromPattern(pattern));
        }
    }

    public class UiAudioController : MonoBehaviour
    {
        [SerializeField] private AudioSystem audioSystem;

        public void PlayOpenCard()
        {
            if (audioSystem != null)
                audioSystem.Play(AudioEventRouter.ForOpenCardUi());
        }

        public void PlayConfirm()
        {
            if (audioSystem != null)
                audioSystem.Play(AudioEventRouter.ForConfirmUi());
        }
    }
}
