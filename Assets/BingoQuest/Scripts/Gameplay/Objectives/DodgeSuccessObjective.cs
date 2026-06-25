using BingoQuest.Core;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Complete by successfully dodging a specified number of times.
    /// </summary>
    public sealed class DodgeSuccessObjective : ObjectiveBase
    {
        private readonly string _objectiveId;
        private readonly int _requiredCount;

        public DodgeSuccessObjective(string objectiveId, int requiredCount)
        {
            _objectiveId = objectiveId;
            _requiredCount = requiredCount;
        }

        public override string ObjectiveId => _objectiveId;
        public override string DisplayName => $"Dodge {_requiredCount} times";
        public override int RequiredProgress => _requiredCount;

        protected override int TryAccumulateProgress(in ObjectiveEvent evt)
        {
            return evt.Type == ObjectiveEventType.DodgeSuccess ? 1 : 0;
        }
    }
}
