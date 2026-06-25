using System;

namespace BingoQuest.Gameplay.Combat
{
    /// <summary>
    /// Tracks a single status effect instance applied to a combat entity.
    /// Tick-based: callers advance time by calling <see cref="Tick"/>.
    /// </summary>
    public sealed class StatusEffect
    {
        public StatusEffectType Type { get; }
        public string SourceActorId { get; }
        public float TickDamage { get; }
        public float TickInterval { get; }
        public float TotalDuration { get; }

        private float _elapsed;
        private float _nextTickTime;

        public bool IsExpired => _elapsed >= TotalDuration;
        public float RemainingTime => Math.Max(0f, TotalDuration - _elapsed);

        /// <summary>Raised each time a damage tick fires.</summary>
        public event Action<StatusEffect, float>? OnTick;

        /// <summary>Raised when the effect expires naturally.</summary>
        public event Action<StatusEffect>? OnExpired;

        public StatusEffect(
            StatusEffectType type,
            string sourceActorId,
            float tickDamage,
            float tickInterval,
            float totalDuration)
        {
            Type = type;
            SourceActorId = sourceActorId ?? throw new ArgumentNullException(nameof(sourceActorId));
            TickDamage = tickDamage;
            TickInterval = tickInterval > 0 ? tickInterval : throw new ArgumentOutOfRangeException(nameof(tickInterval));
            TotalDuration = totalDuration > 0 ? totalDuration : throw new ArgumentOutOfRangeException(nameof(totalDuration));
            _nextTickTime = tickInterval;
        }

        /// <summary>
        /// Advances the status effect by <paramref name="deltaTime"/> seconds.
        /// Fires <see cref="OnTick"/> for each elapsed tick interval and
        /// <see cref="OnExpired"/> when the duration ends.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (IsExpired) return;

            _elapsed += deltaTime;

            while (_nextTickTime <= _elapsed && !IsExpired)
            {
                OnTick?.Invoke(this, TickDamage);
                _nextTickTime += TickInterval;
            }

            if (IsExpired)
                OnExpired?.Invoke(this);
        }

        /// <summary>
        /// Factory helpers for common status types.
        /// </summary>
        public static StatusEffect Burn(string sourceId, float dps = 10f, float duration = 4f) =>
            new(StatusEffectType.Burn, sourceId, dps, 1f, duration);

        public static StatusEffect Poison(string sourceId, float dps = 8f, float duration = 6f) =>
            new(StatusEffectType.Poison, sourceId, dps, 1f, duration);

        public static StatusEffect Bleed(string sourceId, float dps = 12f, float duration = 3f) =>
            new(StatusEffectType.Bleed, sourceId, dps, 0.5f, duration);

        public static StatusEffect Shock(string sourceId, float duration = 2f) =>
            new(StatusEffectType.Shock, sourceId, 5f, 0.5f, duration);
    }
}
