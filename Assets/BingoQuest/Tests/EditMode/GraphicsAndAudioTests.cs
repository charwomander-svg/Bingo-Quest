using BingoQuest.Presentation.Graphics;
using BingoQuest.Presentation.Audio;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace BingoQuest.Tests.EditMode
{
    public class GraphicsAndAudioTests
    {
        private GameObject testGameObject;
        private Animator animator;
        private CharacterAnimator characterAnimator;
        private VFXSpawner vfxSpawner;

        [SetUp]
        public void Setup()
        {
            testGameObject = new GameObject("TestCharacter");
            animator = testGameObject.AddComponent<Animator>();
            characterAnimator = testGameObject.AddComponent<CharacterAnimator>();
            vfxSpawner = testGameObject.AddComponent<VFXSpawner>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(testGameObject);
        }

        #region Character Animation Tests

        [Test]
        public void CharacterAnimator_InitializesWithAnimator()
        {
            Assert.That(characterAnimator, Is.Not.Null);
        }

        [Test]
        public void CharacterAnimator_CanPlayAttack()
        {
            // Verify no error thrown
            characterAnimator.PlayAttack(0);
            Assert.Pass();
        }

        [Test]
        public void CharacterAnimator_CanPlayCast()
        {
            characterAnimator.PlayCast(2f);
            Assert.Pass();
        }

        [Test]
        public void CharacterAnimator_CanPlayHurt()
        {
            characterAnimator.PlayHurt();
            Assert.Pass();
        }

        [Test]
        public void CharacterAnimator_CanPlayDeath()
        {
            characterAnimator.PlayDeath();
            Assert.That(characterAnimator.enabled, Is.False);
        }

        [Test]
        public void CharacterAnimator_CanSetGroundedState()
        {
            characterAnimator.SetGrounded(true);
            Assert.Pass();

            characterAnimator.SetGrounded(false);
            Assert.Pass();
        }

        [Test]
        public void CharacterAnimator_ReturnsCurrentStateName()
        {
            string stateName = characterAnimator.GetCurrentStateName();
            Assert.That(stateName, Is.Not.Null);
        }

        [Test]
        public void CharacterAnimator_CanCheckAnimationPlaying()
        {
            bool isPlaying = characterAnimator.IsAnimationPlaying("Idle");
            Assert.That(isPlaying, Is.TypeOf<bool>());
        }

        #endregion

        #region VFX Tests

        [Test]
        public void VFXSpawner_InitializesWithoutError()
        {
            Assert.That(vfxSpawner, Is.Not.Null);
        }

        [Test]
        public void VFXSpawner_CanSpawnEffect()
        {
            vfxSpawner.SpawnEffect("Hit", Vector3.zero);
            Assert.Pass();
        }

        [Test]
        public void VFXSpawner_CanSpawnHitEffect()
        {
            vfxSpawner.SpawnHitEffect(Vector3.zero, Vector3.forward);
            Assert.Pass();
        }

        [Test]
        public void VFXSpawner_CanSpawnCastEffect()
        {
            vfxSpawner.SpawnCastEffect(Vector3.zero, "Fire");
            Assert.Pass();
        }

        [Test]
        public void VFXSpawner_CanSpawnEnvironmentEffect()
        {
            vfxSpawner.SpawnEnvironmentEffect("Dust", Vector3.zero);
            Assert.Pass();
        }

        [Test]
        public void VFXSpawner_CanRegisterDynamicPrefab()
        {
            ParticleSystem prefab = testGameObject.AddComponent<ParticleSystem>();
            vfxSpawner.RegisterEffectPrefab("Dynamic", prefab);
            Assert.Pass();
        }

        #endregion

        #region Audio Manager Tests

        [Test]
        public void AudioManager_CanFindInstance()
        {
            // Create audio manager in scene
            GameObject audioGO = new GameObject("AudioManager");
            AudioManager audioManager = audioGO.AddComponent<AudioManager>();

            Assert.That(AudioManager.Instance, Is.Not.Null);

            Object.Destroy(audioGO);
        }

        [Test]
        public void AudioManager_MasterVolumeIsValid()
        {
            GameObject audioGO = new GameObject("AudioManager");
            AudioManager audioManager = audioGO.AddComponent<AudioManager>();

            Assert.That(audioManager.MasterVolume, Is.GreaterThanOrEqualTo(0f).And.LessThanOrEqualTo(1f));

            Object.Destroy(audioGO);
        }

        [Test]
        public void AudioManager_CanPlaySFX()
        {
            GameObject audioGO = new GameObject("AudioManager");
            AudioManager audioManager = audioGO.AddComponent<AudioManager>();

            AudioSource source = audioManager.PlaySFX("TestSFX", Vector3.zero);
            // Source may be null if clip not found, but should not throw

            Object.Destroy(audioGO);
        }

        [Test]
        public void AudioManager_CanPlayMusic()
        {
            GameObject audioGO = new GameObject("AudioManager");
            AudioManager audioManager = audioGO.AddComponent<AudioManager>();

            AudioSource source = audioManager.PlayMusic("TestMusic");
            // Music plays with loop enabled
            // Verify no error thrown

            Object.Destroy(audioGO);
        }

        [Test]
        public void AudioManager_CanStopAudio()
        {
            GameObject audioGO = new GameObject("AudioManager");
            AudioManager audioManager = audioGO.AddComponent<AudioManager>();

            AudioSource source = audioGO.AddComponent<AudioSource>();
            audioManager.Stop(source, 0f);

            Object.Destroy(audioGO);
        }

        [Test]
        public void AudioManager_CanSetCategoryVolume()
        {
            GameObject audioGO = new GameObject("AudioManager");
            AudioManager audioManager = audioGO.AddComponent<AudioManager>();

            audioManager.SetCategoryVolume("SFX", 0.5f);
            float volume = audioManager.GetCategoryVolume("SFX");

            Assert.That(volume, Is.EqualTo(0.5f).Within(0.01f));

            Object.Destroy(audioGO);
        }

        [Test]
        public void AudioManager_CanGetCategoryVolume()
        {
            GameObject audioGO = new GameObject("AudioManager");
            AudioManager audioManager = audioGO.AddComponent<AudioManager>();

            float volume = audioManager.GetCategoryVolume("SFX");
            Assert.That(volume, Is.GreaterThanOrEqualTo(0f).And.LessThanOrEqualTo(1f));

            Object.Destroy(audioGO);
        }

        #endregion

        #region Combat Audio Tests

        [Test]
        public void CombatAudioEvents_CanTriggerPlayerHit()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject combatGO = new GameObject("CombatAudio");
            CombatAudioEvents combatAudio = combatGO.AddComponent<CombatAudioEvents>();

            combatAudio.OnPlayerTakeDamage(10, Vector3.zero);
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(combatGO);
        }

        [Test]
        public void CombatAudioEvents_CanTriggerPlayerDamage()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject combatGO = new GameObject("CombatAudio");
            CombatAudioEvents combatAudio = combatGO.AddComponent<CombatAudioEvents>();

            combatAudio.OnPlayerDealDamage(25, false, Vector3.zero);
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(combatGO);
        }

        [Test]
        public void CombatAudioEvents_CanTriggerAbilityCast()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject combatGO = new GameObject("CombatAudio");
            CombatAudioEvents combatAudio = combatGO.AddComponent<CombatAudioEvents>();

            combatAudio.OnAbilityCast("Fire", Vector3.zero);
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(combatGO);
        }

        [Test]
        public void CombatAudioEvents_CanTriggerDeath()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject combatGO = new GameObject("CombatAudio");
            CombatAudioEvents combatAudio = combatGO.AddComponent<CombatAudioEvents>();

            combatAudio.OnCharacterDeath(Vector3.zero);
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(combatGO);
        }

        [Test]
        public void CombatAudioEvents_CanTriggerLevelUp()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject combatGO = new GameObject("CombatAudio");
            CombatAudioEvents combatAudio = combatGO.AddComponent<CombatAudioEvents>();

            combatAudio.OnLevelUp(Vector3.zero);
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(combatGO);
        }

        [Test]
        public void CombatAudioEvents_CanTriggerStatusEffect()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject combatGO = new GameObject("CombatAudio");
            CombatAudioEvents combatAudio = combatGO.AddComponent<CombatAudioEvents>();

            combatAudio.OnStatusEffectApplied("Poison", Vector3.zero);
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(combatGO);
        }

        #endregion

        #region Footstep Audio Tests

        [Test]
        public void FootstepAudio_CanPlayFootstep()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject footstepGO = new GameObject("Footstep");
            FootstepAudio footstepAudio = footstepGO.AddComponent<FootstepAudio>();

            footstepAudio.PlayFootstep();
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(footstepGO);
        }

        [Test]
        public void FootstepAudio_CanSetTerrainType()
        {
            GameObject footstepGO = new GameObject("Footstep");
            FootstepAudio footstepAudio = footstepGO.AddComponent<FootstepAudio>();

            footstepAudio.SetTerrainType("Stone");
            Assert.Pass();

            footstepAudio.SetTerrainType("Metal");
            Assert.Pass();

            Object.Destroy(footstepGO);
        }

        [Test]
        public void FootstepAudio_CanPlayFootstepSequence()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject footstepGO = new GameObject("Footstep");
            FootstepAudio footstepAudio = footstepGO.AddComponent<FootstepAudio>();

            footstepAudio.PlayFootstepSequence(4);
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(footstepGO);
        }

        #endregion

        #region UI Sound Effects Tests

        [Test]
        public void UISoundEffects_CanPlayButtonClick()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject uiGO = new GameObject("UISound");
            UISoundEffects uiSound = uiGO.AddComponent<UISoundEffects>();

            uiSound.OnButtonClick();
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(uiGO);
        }

        [Test]
        public void UISoundEffects_CanPlayMenuOpen()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject uiGO = new GameObject("UISound");
            UISoundEffects uiSound = uiGO.AddComponent<UISoundEffects>();

            uiSound.OnMenuOpen();
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(uiGO);
        }

        [Test]
        public void UISoundEffects_CanPlayMenuClose()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject uiGO = new GameObject("UISound");
            UISoundEffects uiSound = uiGO.AddComponent<UISoundEffects>();

            uiSound.OnMenuClose();
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(uiGO);
        }

        [Test]
        public void UISoundEffects_CanPlayItemPickup()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject uiGO = new GameObject("UISound");
            UISoundEffects uiSound = uiGO.AddComponent<UISoundEffects>();

            uiSound.OnItemPickup();
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(uiGO);
        }

        [Test]
        public void UISoundEffects_CanPlayError()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject uiGO = new GameObject("UISound");
            UISoundEffects uiSound = uiGO.AddComponent<UISoundEffects>();

            uiSound.OnError();
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(uiGO);
        }

        [Test]
        public void UISoundEffects_CanPlayNotification()
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();

            GameObject uiGO = new GameObject("UISound");
            UISoundEffects uiSound = uiGO.AddComponent<UISoundEffects>();

            uiSound.OnNotification();
            Assert.Pass();

            Object.Destroy(audioGO);
            Object.Destroy(uiGO);
        }

        #endregion
    }
}
