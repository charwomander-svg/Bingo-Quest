using System;
using BingoQuest.Core;

namespace BingoQuest.Gameplay.Objectives
{
    /// <summary>
    /// Creates <see cref="IObjective"/> instances from <see cref="ObjectiveDefinition"/> descriptors.
    /// Applies context-driven scaling to required-progress counts.
    /// </summary>
    public static class ObjectiveFactory
    {
        /// <summary>
        /// Instantiates and initialises the appropriate objective for <paramref name="def"/>.
        /// </summary>
        public static IObjective Create(ObjectiveDefinition def, ObjectiveContext context)
        {
            if (def is null)    throw new ArgumentNullException(nameof(def));
            if (context is null) throw new ArgumentNullException(nameof(context));

            int scaledCount = ScaleCount(def.BaseRequiredCount, context);

            IObjective objective = def.Type switch
            {
                ObjectiveType.KillCount    => new KillCountObjective(def.ObjectiveId, scaledCount, def.Filter),
                ObjectiveType.ElementalKill => new ElementalKillObjective(def.ObjectiveId, scaledCount, def.Element),
                ObjectiveType.CritKill     => new CritKillObjective(def.ObjectiveId, scaledCount),
                ObjectiveType.StatusApply  => new StatusApplyObjective(def.ObjectiveId, scaledCount, def.Filter),
                ObjectiveType.DodgeSuccess => new DodgeSuccessObjective(def.ObjectiveId, scaledCount),
                ObjectiveType.AbilityUse   => new AbilityUseObjective(def.ObjectiveId, scaledCount, def.Filter),
                ObjectiveType.BossDefeat   => new BossDefeatObjective(def.ObjectiveId, def.Filter),
                _ => throw new NotSupportedException($"Unknown ObjectiveType: {def.Type}")
            };

            objective.Initialize(context);
            return objective;
        }

        // ── Scaling ───────────────────────────────────────────────────────────

        /// <summary>
        /// Scales the base count based on difficulty tier.
        /// Normal = base, each tier adds ~30%.
        /// </summary>
        private static int ScaleCount(int baseCount, ObjectiveContext context)
        {
            float multiplier = context.DifficultyTier switch
            {
                0 => 1.0f,
                1 => 1.3f,
                2 => 1.7f,
                _ => 2.2f   // Chaos and beyond
            };
            return Math.Max(1, (int)MathF.Round(baseCount * multiplier));
        }
    }
}
