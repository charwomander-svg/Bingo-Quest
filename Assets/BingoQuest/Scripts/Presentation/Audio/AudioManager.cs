using UnityEngine;
using System.Collections.Generic;

namespace BingoQuest.Presentation.Audio
{
    /// <summary>
    /// Centralized audio manager handling all sound effects, music, and ambience.
    /// Supports volume control per category, pooling, and spatial audio.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [System.Serializable]
        public class AudioCategory
        {
            public string categoryName;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(-3f, 3f)] public float pitch = 1f;
            public bool is3D = false;
            public int poolSize = 10;
        }

        [System.Serializable]
        public class AudioClipDefinition
        {
            public string clipName;
            public AudioClip clip;
            public string category = "SFX";
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0f, 1f)] public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
        }

        [SerializeField] private List<AudioCategory> categories = new();
        [SerializeField] private List<AudioClipDefinition> audioClips = new();
        [SerializeField] private bool debugLogging = false;

        private Dictionary<string, AudioCategory> categoryMap = new();
        private Dictionary<string, AudioClip> clipMap = new();
        private Dictionary<string, Queue<AudioSource>> audioSourcePools = new();

        private AudioListener audioListener;
        private static AudioManager instance;

        public static AudioManager Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<AudioManager>();
                    if (!instance)
                    {
                        Debug.LogError("AudioManager not found in scene!");
                    }
                }
                return instance;
            }
        }

        public float MasterVolume { get; set; } = 1f;

        private void Awake()
        {
            if (instance && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSystem();
        }

        private void InitializeAudioSystem()
        {
            audioListener = GetComponent<AudioListener>();
            if (!audioListener)
                audioListener = gameObject.AddComponent<AudioListener>();

            // Index categories
            foreach (var category in categories)
            {
                categoryMap[category.categoryName] = category;
            }

            // Index clips
            foreach (var clipDef in audioClips)
            {
                if (clipDef.clip)
                    clipMap[clipDef.clipName] = clipDef.clip;
            }

            // Create audio source pools
            foreach (var category in categories)
            {
                var pool = new Queue<AudioSource>();
                for (int i = 0; i < category.poolSize; i++)
                {
                    AudioSource source = gameObject.AddComponent<AudioSource>();
                    source.playOnAwake = false;
                    pool.Enqueue(source);
                }
                audioSourcePools[category.categoryName] = pool;
            }

            if (debugLogging)
                Debug.Log($"<b>AudioManager initialized:</b> {categories.Count} categories, {clipMap.Count} clips");
        }

        /// <summary>Play a sound effect by name.</summary>
        public AudioSource PlaySFX(string clipName, Vector3 position = default, float volumeMultiplier = 1f)
        {
            if (!clipMap.TryGetValue(clipName, out AudioClip clip))
            {
                Debug.LogWarning($"Audio clip '{clipName}' not found");
                return null;
            }

            return PlayAudioClip(clipName, clip, position, volumeMultiplier);
        }

        /// <summary>Play music track (loops by default).</summary>
        public AudioSource PlayMusic(string clipName, float fadeInDuration = 1f)
        {
            if (!clipMap.TryGetValue(clipName, out AudioClip clip))
            {
                Debug.LogWarning($"Music track '{clipName}' not found");
                return null;
            }

            AudioSource source = PlayAudioClip(clipName, clip, Vector3.zero, 1f);
            if (source)
            {
                source.loop = true;
                source.spatialBlend = 0f; // Music is always 2D

                if (fadeInDuration > 0)
                    StartCoroutine(FadeIn(source, fadeInDuration));
            }

            return source;
        }

        /// <summary>Play ambient sound (loops).</summary>
        public AudioSource PlayAmbience(string clipName, float volume = 0.5f)
        {
            if (!clipMap.TryGetValue(clipName, out AudioClip clip))
            {
                Debug.LogWarning($"Ambience '{clipName}' not found");
                return null;
            }

            AudioSource source = PlayAudioClip(clipName, clip, Vector3.zero, volume);
            if (source)
            {
                source.loop = true;
                source.spatialBlend = 0f;
            }

            return source;
        }

        /// <summary>Stop a playing audio source with optional fade.</summary>
        public void Stop(AudioSource source, float fadeDuration = 0f)
        {
            if (!source) return;

            if (fadeDuration > 0)
                StartCoroutine(FadeOut(source, fadeDuration));
            else
                source.Stop();
        }

        /// <summary>Stop all sounds in a category.</summary>
        public void StopCategory(string categoryName, float fadeDuration = 0f)
        {
            if (!audioSourcePools.TryGetValue(categoryName, out var pool))
                return;

            foreach (var source in pool)
            {
                if (source && source.isPlaying)
                    Stop(source, fadeDuration);
            }
        }

        /// <summary>Set volume for entire category.</summary>
        public void SetCategoryVolume(string categoryName, float volume)
        {
            if (categoryMap.TryGetValue(categoryName, out var category))
            {
                category.volume = Mathf.Clamp01(volume);
            }
        }

        /// <summary>Get current volume for category.</summary>
        public float GetCategoryVolume(string categoryName)
        {
            return categoryMap.TryGetValue(categoryName, out var category) ? category.volume : 1f;
        }

        private AudioSource PlayAudioClip(string clipName, AudioClip clip, Vector3 position, float volumeMultiplier)
        {
            if (!clip) return null;

            // Find clip definition for properties
            AudioClipDefinition clipDef = null;
            foreach (var def in audioClips)
            {
                if (def.clipName == clipName)
                {
                    clipDef = def;
                    break;
                }
            }

            string category = clipDef?.category ?? "SFX";
            if (!categoryMap.TryGetValue(category, out var categoryConfig))
            {
                Debug.LogWarning($"Audio category '{category}' not found");
                return null;
            }

            // Get or create audio source from pool
            AudioSource source = GetAudioSource(category);
            if (!source) return null;

            // Configure source
            source.clip = clip;
            source.volume = categoryConfig.volume * (clipDef?.volume ?? 1f) * volumeMultiplier * MasterVolume;
            source.pitch = categoryConfig.pitch;
            source.spatialBlend = clipDef?.spatialBlend ?? (categoryConfig.is3D ? 1f : 0f);

            // Set position for 3D audio
            if (source.spatialBlend > 0)
            {
                source.transform.position = position;
            }

            source.PlayOneShot(clip);

            if (debugLogging)
                Debug.Log($"Playing: {clipName} (cat: {category}, vol: {source.volume:F2})");

            return source;
        }

        private AudioSource GetAudioSource(string category)
        {
            if (!audioSourcePools.TryGetValue(category, out var pool))
                return null;

            // Return unused source, or create new one if needed
            while (pool.Count > 0)
            {
                AudioSource source = pool.Dequeue();
                if (source && !source.isPlaying)
                {
                    return source;
                }
            }

            // Pool exhausted, create temporary source
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            return newSource;
        }

        private System.Collections.IEnumerator FadeIn(AudioSource source, float duration)
        {
            float elapsed = 0f;
            float targetVolume = source.volume;
            source.volume = 0f;

            while (elapsed < duration && source && source.isPlaying)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            if (source)
                source.volume = targetVolume;
        }

        private System.Collections.IEnumerator FadeOut(AudioSource source, float duration)
        {
            float elapsed = 0f;
            float startVolume = source.volume;

            while (elapsed < duration && source && source.isPlaying)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            if (source)
            {
                source.volume = 0f;
                source.Stop();
            }
        }
    }

    /// <summary>
    /// Event-driven audio trigger for combat systems.
    /// Plays audio based on game events (damage, ability cast, enemy attack, etc).
    /// </summary>
    public class CombatAudioEvents : MonoBehaviour
    {
        [SerializeField] private string playerHitSFX = "Player_Hit";
        [SerializeField] private string enemyHitSFX = "Enemy_Hit";
        [SerializeField] private string abilityCastSFX = "Ability_Cast";
        [SerializeField] private string deathSFX = "Death";
        [SerializeField] private string levelUpSFX = "LevelUp";

        private AudioManager audioManager;

        private void Start()
        {
            audioManager = AudioManager.Instance;
        }

        /// <summary>Play sound when player takes damage.</summary>
        public void OnPlayerTakeDamage(int damage, Vector3 position)
        {
            if (audioManager)
                audioManager.PlaySFX(playerHitSFX, position);
        }

        /// <summary>Play sound when player deals damage to enemy.</summary>
        public void OnPlayerDealDamage(int damage, bool isCrit, Vector3 position)
        {
            if (audioManager)
            {
                string sfx = isCrit ? "Crit_Hit" : "Normal_Hit";
                audioManager.PlaySFX(sfx, position, isCrit ? 1.3f : 1f);
            }
        }

        /// <summary>Play sound when ability is cast.</summary>
        public void OnAbilityCast(string abilityType, Vector3 position)
        {
            if (audioManager)
                audioManager.PlaySFX($"{abilityType}_Cast", position);
        }

        /// <summary>Play sound when character dies.</summary>
        public void OnCharacterDeath(Vector3 position)
        {
            if (audioManager)
                audioManager.PlaySFX(deathSFX, position);
        }

        /// <summary>Play sound when character levels up.</summary>
        public void OnLevelUp(Vector3 position)
        {
            if (audioManager)
                audioManager.PlaySFX(levelUpSFX, position);
        }

        /// <summary>Play sound when status effect is applied.</summary>
        public void OnStatusEffectApplied(string statusType, Vector3 position)
        {
            if (audioManager)
                audioManager.PlaySFX($"Status_{statusType}", position, 0.7f);
        }
    }

    /// <summary>
    /// Footstep audio controller synchronized with animation events.
    /// </summary>
    public class FootstepAudio : MonoBehaviour
    {
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private string grassFootstepSFX = "Footstep_Grass";
        [SerializeField] private string stoneFootstepSFX = "Footstep_Stone";
        [SerializeField] private string metalFootstepSFX = "Footstep_Metal";
        [SerializeField][Range(0f, 1f)] private float footstepVolume = 0.3f;

        private string currentTerrainType = "Grass";

        private void Start()
        {
            if (!audioManager)
                audioManager = AudioManager.Instance;
        }

        /// <summary>Called from animation event when foot hits ground.</summary>
        public void PlayFootstep()
        {
            if (!audioManager) return;

            string sfxName = currentTerrainType switch
            {
                "Stone" => stoneFootstepSFX,
                "Metal" => metalFootstepSFX,
                "Grass" => grassFootstepSFX,
                _ => grassFootstepSFX
            };

            audioManager.PlaySFX(sfxName, transform.position, footstepVolume);
        }

        /// <summary>Set current terrain type (called when entering new ground).</summary>
        public void SetTerrainType(string terrainType)
        {
            currentTerrainType = terrainType;
        }

        /// <summary>Play multiple footsteps for running or charging.</summary>
        public void PlayFootstepSequence(int count)
        {
            for (int i = 0; i < count; i++)
            {
                PlayFootstep();
            }
        }
    }

    /// <summary>
    /// UI sound effects for menus, buttons, notifications, etc.
    /// </summary>
    public class UISoundEffects : MonoBehaviour
    {
        [SerializeField] private string buttonClickSFX = "UI_Click";
        [SerializeField] private string menuOpenSFX = "UI_Open";
        [SerializeField] private string menuCloseSFX = "UI_Close";
        [SerializeField] private string itemPickupSFX = "Item_Pickup";
        [SerializeField] private string errorSFX = "UI_Error";
        [SerializeField] private string notificationSFX = "UI_Notification";

        private AudioManager audioManager;

        private void Start()
        {
            audioManager = AudioManager.Instance;
        }

        public void OnButtonClick()
        {
            audioManager?.PlaySFX(buttonClickSFX);
        }

        public void OnMenuOpen()
        {
            audioManager?.PlaySFX(menuOpenSFX);
        }

        public void OnMenuClose()
        {
            audioManager?.PlaySFX(menuCloseSFX);
        }

        public void OnItemPickup()
        {
            audioManager?.PlaySFX(itemPickupSFX);
        }

        public void OnError()
        {
            audioManager?.PlaySFX(errorSFX);
        }

        public void OnNotification()
        {
            audioManager?.PlaySFX(notificationSFX);
        }
    }
}
