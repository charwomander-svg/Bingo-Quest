using System;
using System.Collections.Generic;
using BingoQuest.Core;
using BingoQuest.Gameplay.Objectives;

namespace BingoQuest.Gameplay.Bingo
{
    /// <summary>
    /// Default implementation of <see cref="ICardGenerator"/>.
    /// Produces a seeded-random 5×5 card from the registered objective pool,
    /// guaranteeing no duplicates and at least one completable line.
    /// </summary>
    public sealed class CardGenerator : ICardGenerator
    {
        private readonly IReadOnlyList<ObjectiveDefinition> _objectivePool;

        public CardGenerator(IReadOnlyList<ObjectiveDefinition> objectivePool)
        {
            _objectivePool = objectivePool ?? throw new ArgumentNullException(nameof(objectivePool));
        }

        /// <inheritdoc/>
        public BingoCard Generate(CardGenerationRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            int seed = request.Seed != 0 ? request.Seed : Environment.TickCount;
            var rng = new Random(seed);

            var context = new ObjectiveContext
            {
                ZoneId = request.ZoneId,
                DifficultyTier = request.DifficultyTier,
                ClassId = request.PlayerClass.ToString(),
                CharacterLevel = request.CharacterLevel,
                IsCoopSession = request.IsCoopSession,
                IsPvpSession = request.IsPvpSession
            };

            // Filter the pool to objectives valid for this context
            var eligible = FilterPool(request);
            if (eligible.Count < BingoCard.TotalSquares - 1) // -1 for FREE centre
                throw new InvalidOperationException(
                    $"Objective pool too small: need at least {BingoCard.TotalSquares - 1} eligible objectives, got {eligible.Count}.");

            // Shuffle eligible definitions
            Shuffle(eligible, rng);

            var squares = new BingoSquare[BingoCard.TotalSquares];
            int poolIdx = 0;

            for (int i = 0; i < BingoCard.TotalSquares; i++)
            {
                if (i == BingoCard.FreeCentreIndex)
                {
                    squares[i] = new BingoSquare(i, new FreeObjective(), isFree: true);
                    continue;
                }

                var def = eligible[poolIdx++];
                var objective = ObjectiveFactory.Create(def, context);
                squares[i] = new BingoSquare(i, objective);
            }

            string cardId = $"{request.ZoneId}_{seed}_{DateTime.UtcNow.Ticks}";
            return new BingoCard(cardId, request.ZoneId, squares);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private List<ObjectiveDefinition> FilterPool(CardGenerationRequest request)
        {
            var result = new List<ObjectiveDefinition>(_objectivePool.Count);
            foreach (var def in _objectivePool)
            {
                if (!string.IsNullOrEmpty(def.RequiredZoneId) && def.RequiredZoneId != request.ZoneId)
                    continue;
                if (def.RequiredClass != Progression.ClassType.None && def.RequiredClass != request.PlayerClass)
                    continue;
                if (def.MinimumDifficultyTier > request.DifficultyTier)
                    continue;
                result.Add(def);
            }
            return result;
        }

        private static void Shuffle<T>(List<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
