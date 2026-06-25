using BingoQuest.Core;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Complete by killing a specified number of enemies.
    /// Optionally restricted to a particular enemy type (SourceId prefix match).
    /// </summary>
    public sealed class KillCountObjective : ObjectiveBase
    {
        private readonly string _objectiveId;
        private readonly int _requiredCount;
        private readonly string _enemyTypeFilter; // empty = any enemy

        public KillCountObjective(string objectiveId, int requiredCount, string enemyTypeFilter = "")
        {
            _objectiveId = objectiveId;
            _requiredCount = requiredCount;
            _enemyTypeFilter = enemyTypeFilter;
        }

        public override string ObjectiveId => _objectiveId;

        public override string DisplayName =>
            string.IsNullOrEmpty(_enemyTypeFilter)
                ? $"Kill {_requiredCount} enemies"
                : $"Kill {_requiredCount} {_enemyTypeFilter}s";

        public override int RequiredProgress => _requiredCount;

        protected override int TryAccumulateProgress(in ObjectiveEvent evt)
        {
            if (evt.Type != ObjectiveEventType.EnemyKilled) return 0;
            if (!string.IsNullOrEmpty(_enemyTypeFilter) &&
                !evt.SourceId.StartsWith(_enemyTypeFilter, System.StringComparison.OrdinalIgnoreCase))
                return 0;
            return 1;
        }
    }
}
