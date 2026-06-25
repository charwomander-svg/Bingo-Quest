namespace BingoQuest.Core
{
    /// <summary>Typed combat / gameplay events consumed by the objective system.</summary>
    public enum ObjectiveEventType
    {
        None = 0,
        EnemyKilled,
        CriticalHit,
        DodgeSuccess,
        AbilityUsed,
        StatusApplied,
        BossDefeated,
        TreasureOpened,
        ComboMilestone,
        DamageTaken,
        HealingDone,
        PlayerLevelUp,
        ItemCrafted,
        ItemEquipped
    }
}
