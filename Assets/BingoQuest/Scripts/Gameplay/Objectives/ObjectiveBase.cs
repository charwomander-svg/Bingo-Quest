using UnityEngine;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Base class for common objective logic. Derive from this to implement specific objectives.
    /// </summary>
    public abstract class ObjectiveBase : IObjective
    {
        public string ObjectiveId { get; protected set; }
        public int CurrentProgress { get; protected set; }
        public int RequiredProgress { get; protected set; }
        public bool IsComplete { get; protected set; }
        public abstract ObjectiveCategory Category { get; }

        protected ObjectiveContext Context;

        public virtual void Initialize(ObjectiveContext context)
        {
            Context = context;
            CurrentProgress = 0;
            IsComplete = false;
            OnInitialize();
        }

        public virtual void UpdateProgress(in ObjectiveEvent evt)
        {
            if (IsComplete) return;
            
            OnProgressUpdate(in evt);
            
            if (CurrentProgress >= RequiredProgress)
            {
                IsComplete = true;
                Complete();
            }
        }

        public virtual void Complete()
        {
            OnComplete();
        }

        public virtual void Reset()
        {
            CurrentProgress = 0;
            IsComplete = false;
            OnReset();
        }

        protected virtual void OnInitialize() { }

        protected virtual void OnProgressUpdate(in ObjectiveEvent evt) { }

        protected virtual void OnComplete() { }

        protected virtual void OnReset() { }
    }
}
