using BingoQuest.Core;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Complete by landing a specified number of critical hits.
    /// </summary>
    public sealed class CritKillObjective : ObjectiveBase
    {
        private readonly string _objectiveId;
        private readonly int _requiredCount;

        public CritKillObjective(string objectiveId, int requiredCount)
        {
            _objectiveId = objectiveId;
            _requiredCount = requiredCount;
        }

        public override string ObjectiveId => _objectiveId;
        public override string DisplayName => $"Land {_requiredCount} critical hits";
        public override int RequiredProgress => _requiredCount;

        protected override int TryAccumulateProgress(in ObjectiveEvent evt)
        {
            // Accept both EnemyKilled and CriticalHit events that are flagged as critical
            if (evt.IsCritical &&
                (evt.Type == ObjectiveEventType.CriticalHit || evt.Type == ObjectiveEventType.EnemyKilled))
                return 1;
            return 0;
        }
    }
}
