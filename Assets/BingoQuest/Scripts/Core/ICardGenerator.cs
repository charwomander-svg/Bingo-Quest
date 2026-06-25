using BingoQuest.Gameplay.Bingo;

namespace BingoQuest.Core
{
    /// <summary>Generates a populated <see cref="BingoCard"/> from a generation request.</summary>
    public interface ICardGenerator
    {
        /// <summary>
        /// Produces a fresh card with objectives appropriate to the request context.
        /// Guarantees:
        ///   - No duplicate objective IDs on the same card.
        ///   - No impossible combinations for the given zone/class/difficulty.
        ///   - At minimum one viable completion path exists.
        ///   - The centre square (index 12) is always FREE.
        /// </summary>
        BingoCard Generate(CardGenerationRequest request);
    }
}
