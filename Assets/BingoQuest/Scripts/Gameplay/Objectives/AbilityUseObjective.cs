using BingoQuest.Core;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Complete by using abilities a specified number of times.
    /// Can be restricted to a particular ability by abilityId prefix.
    /// </summary>
    public sealed class AbilityUseObjective : ObjectiveBase
    {
        private readonly string _objectiveId;
        private readonly int _requiredCount;
        private readonly string _abilityFilter; // empty = any ability

        public AbilityUseObjective(string objectiveId, int requiredCount, string abilityFilter = "")
        {
            _objectiveId = objectiveId;
            _requiredCount = requiredCount;
            _abilityFilter = abilityFilter;
        }

        public override string ObjectiveId => _objectiveId;

        public override string DisplayName =>
            string.IsNullOrEmpty(_abilityFilter)
                ? $"Use abilities {_requiredCount} times"
                : $"Use {_abilityFilter} {_requiredCount} times";

        public override int RequiredProgress => _requiredCount;

        protected override int TryAccumulateProgress(in ObjectiveEvent evt)
        {
            if (evt.Type != ObjectiveEventType.AbilityUsed) return 0;
            if (!string.IsNullOrEmpty(_abilityFilter) &&
                !evt.SourceId.StartsWith(_abilityFilter, System.StringComparison.OrdinalIgnoreCase))
                return 0;
            return 1;
        }
    }
}
