using BingoQuest.Core;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Complete by applying a status effect a specified number of times.
    /// Can be restricted to a particular <see cref="Combat.StatusEffectType"/> via SourceId.
    /// </summary>
    public sealed class StatusApplyObjective : ObjectiveBase
    {
        private readonly string _objectiveId;
        private readonly int _requiredCount;
        private readonly string _statusFilter; // empty = any status

        public StatusApplyObjective(string objectiveId, int requiredCount, string statusFilter = "")
        {
            _objectiveId = objectiveId;
            _requiredCount = requiredCount;
            _statusFilter = statusFilter;
        }

        public override string ObjectiveId => _objectiveId;

        public override string DisplayName =>
            string.IsNullOrEmpty(_statusFilter)
                ? $"Apply {_requiredCount} status effects"
                : $"Apply {_statusFilter} {_requiredCount} times";

        public override int RequiredProgress => _requiredCount;

        protected override int TryAccumulateProgress(in ObjectiveEvent evt)
        {
            if (evt.Type != ObjectiveEventType.StatusApplied) return 0;
            if (!string.IsNullOrEmpty(_statusFilter) &&
                !evt.SourceId.Equals(_statusFilter, System.StringComparison.OrdinalIgnoreCase))
                return 0;
            return 1;
        }
    }
}
