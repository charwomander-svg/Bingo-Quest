using BingoQuest.Core;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Placeholder objective used by the FREE centre square.
    /// Always reports complete and ignores all events.
    /// </summary>
    public sealed class FreeObjective : IObjective
    {
        public string ObjectiveId => "FREE";
        public string DisplayName => "FREE";
        public int CurrentProgress => 1;
        public int RequiredProgress => 1;
        public bool IsComplete => true;
        public void Initialize(ObjectiveContext context) { }
        public void UpdateProgress(in ObjectiveEvent evt) { }
        public void Complete() { }
        public void Reset() { }
    }
}
