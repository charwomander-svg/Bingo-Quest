using BingoQuest.Core;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Complete by defeating a named boss.
    /// </summary>
    public sealed class BossDefeatObjective : ObjectiveBase
    {
        private readonly string _objectiveId;
        private readonly string _bossId; // empty = any boss

        public BossDefeatObjective(string objectiveId, string bossId = "")
        {
            _objectiveId = objectiveId;
            _bossId = bossId;
        }

        public override string ObjectiveId => _objectiveId;

        public override string DisplayName =>
            string.IsNullOrEmpty(_bossId) ? "Defeat the boss" : $"Defeat {_bossId}";

        public override int RequiredProgress => 1;

        protected override int TryAccumulateProgress(in ObjectiveEvent evt)
        {
            if (evt.Type != ObjectiveEventType.BossDefeated) return 0;
            if (!string.IsNullOrEmpty(_bossId) &&
                !evt.SourceId.Equals(_bossId, System.StringComparison.OrdinalIgnoreCase))
                return 0;
            return 1;
        }
    }
}
