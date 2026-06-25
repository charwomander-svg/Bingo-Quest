namespace BingoQuest.Gameplay.Bingo
{
    /// <summary>Completed board patterns that trigger rewards.</summary>
    public enum PatternType
    {
        Row = 0,
        Column,
        DiagonalMain,
        DiagonalAnti,
        FullCard,
        FourCorners,
        CenterCross,
        TShape,
        LShape
    }
}
