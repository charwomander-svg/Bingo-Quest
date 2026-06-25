using System;
using BingoQuest.Core;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Base class for all concrete objective implementations.
    /// Handles common state management; subclasses implement <see cref="TryAccumulateProgress"/>.
    /// </summary>
    public abstract class ObjectiveBase : IObjective
    {
        private int _currentProgress;
        protected ObjectiveContext? Context { get; private set; }

        // ── IObjective ────────────────────────────────────────────────────────

        public abstract string ObjectiveId { get; }
        public abstract string DisplayName { get; }

        public int CurrentProgress => _currentProgress;
        public abstract int RequiredProgress { get; }
        public bool IsComplete => _currentProgress >= RequiredProgress;

        public void Initialize(ObjectiveContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            OnInitialized(context);
        }

        public void UpdateProgress(in ObjectiveEvent evt)
        {
            if (IsComplete) return;
            int delta = TryAccumulateProgress(evt);
            if (delta <= 0) return;
            _currentProgress = Math.Min(_currentProgress + delta, RequiredProgress);
        }

        public void Complete()
        {
            _currentProgress = RequiredProgress;
        }

        public void Reset()
        {
            _currentProgress = 0;
            OnReset();
        }

        // ── Overridable hooks ─────────────────────────────────────────────────

        /// <summary>Called after <see cref="Initialize"/> stores the context.</summary>
        protected virtual void OnInitialized(ObjectiveContext context) { }

        /// <summary>Called after <see cref="Reset"/> clears progress.</summary>
        protected virtual void OnReset() { }

        /// <summary>
        /// Examine the event and return the amount of progress to add (0 if not applicable).
        /// Must not modify <see cref="CurrentProgress"/> directly.
        /// </summary>
        protected abstract int TryAccumulateProgress(in ObjectiveEvent evt);
    }
}
