namespace BingoQuest.Gameplay.Progression
{
    /// <summary>Playable character classes available in Bingo Quest.</summary>
    public enum ClassType
    {
        None = 0,
        Warrior,
        Mage,
        Ranger,
        Rogue,
        Cleric,
        Necromancer,    // Hidden – unlock via meta progression
        BingoKnight     // Hidden – unlock by completing meta bingo goal
    }
}
