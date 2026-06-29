using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Objectives;
using BingoQuest.Presentation.Audio;
using BingoQuest.Presentation.Graphics;
using NUnit.Framework;

namespace BingoQuest.Tests.PlayMode
{
    public class PresentationEffectsTests
    {
        [Test]
        public void GraphicsRouter_CriticalFireDamage_UsesCriticalCueAndFireTint()
        {
            var result = new DamageResult
            {
                FinalDamage = 90,
                IsCritical = true,
                WasDodged = false,
                Element = ElementType.Fire
            };

            var requests = GraphicsEventRouter.FromDamage(result, targetDefeated: false);
            Assert.AreEqual(1, requests.Count);
            Assert.AreEqual(VfxCue.CriticalHit, requests[0].Cue);
            Assert.Greater(requests[0].Tint.r, 0.9f);
        }

        [Test]
        public void GraphicsRouter_FullCardPattern_UsesFullCardCue()
        {
            var request = GraphicsEventRouter.FromPattern(BingoPattern.FullCard);
            Assert.AreEqual(VfxCue.BingoFullCard, request.Cue);
            Assert.Greater(request.Scale, 1.5f);
        }

        [Test]
        public void AudioRouter_DodgedDamage_UsesDodgeCueOnly()
        {
            var result = new DamageResult
            {
                FinalDamage = 0,
                IsCritical = false,
                WasDodged = true,
                Element = ElementType.Physical
            };

            var requests = AudioEventRouter.FromDamage(result, targetDefeated: false);
            Assert.AreEqual(1, requests.Count);
            Assert.AreEqual(AudioCue.Dodge, requests[0].Cue);
        }

        [Test]
        public void AudioRouter_BurnStatus_MapsToBurnCue()
        {
            var request = AudioEventRouter.FromStatus(StatusEffectType.Burn);
            Assert.AreEqual(AudioCue.StatusBurn, request.Cue);
        }

        [Test]
        public void AudioRouter_FullCardPattern_UsesFanfareCue()
        {
            var request = AudioEventRouter.FromPattern(BingoPattern.FullCard);
            Assert.AreEqual(AudioCue.BingoFullCard, request.Cue);
        }
    }
}
