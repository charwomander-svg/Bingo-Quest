using BingoQuest.Core;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Complete by killing enemies using a specific element.
    /// </summary>
    public sealed class ElementalKillObjective : ObjectiveBase
    {
        private readonly string _objectiveId;
        private readonly int _requiredCount;
        private readonly ElementType _element;

        public ElementalKillObjective(string objectiveId, int requiredCount, ElementType element)
        {
            _objectiveId = objectiveId;
            _requiredCount = requiredCount;
            _element = element;
        }

        public override string ObjectiveId => _objectiveId;
        public override string DisplayName => $"Kill {_requiredCount} enemies with {_element}";
        public override int RequiredProgress => _requiredCount;

        protected override int TryAccumulateProgress(in ObjectiveEvent evt)
        {
            if (evt.Type != ObjectiveEventType.EnemyKilled) return 0;
            if (evt.Element != _element) return 0;
            return 1;
        }
    }
}
