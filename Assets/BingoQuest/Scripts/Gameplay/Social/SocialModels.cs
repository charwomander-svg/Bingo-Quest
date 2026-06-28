using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Bingo;

namespace BingoQuest.Gameplay.Social
{
    public sealed class GhostRunRecord
    {
        public string PlayerId { get; set; }
        public string ClassId { get; set; }
        public string RegionId { get; set; }
        public int CardSeed { get; set; }
        public double CompletionSeconds { get; set; }
        public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;
        public List<string> CompletionOrder { get; set; } = new();
    }

    public sealed class GuildMemberContribution
    {
        public string MemberId { get; set; }
        public int SquaresContributed { get; set; }
        public int PatternsContributed { get; set; }
    }

    public sealed class GuildMilestone
    {
        public string MilestoneId { get; set; }
        public string Label { get; set; }
        public bool IsUnlocked { get; set; }
        public int RequiredLines { get; set; }
    }

    public sealed class GuildBoardState
    {
        public string GuildId { get; set; }
        public int Size { get; set; } = 20;
        public HashSet<string> CompletedSquares { get; } = new();
        public Dictionary<string, GuildMemberContribution> Contributions { get; } = new();
        public List<GuildMilestone> Milestones { get; } = new();
    }

    public sealed class PvpPlayerProgress
    {
        public string PlayerId { get; set; }
        public bool[] CompletedObjectives { get; set; }
        public HashSet<BingoPattern> CompletedPatterns { get; } = new();
    }

    public sealed class PvpBingoMatchState
    {
        public Guid MatchId { get; } = Guid.NewGuid();
        public List<string> ObjectiveIds { get; } = new();
        public int GridSize { get; set; } = 5;
        public PvpPlayerProgress PlayerOne { get; set; }
        public PvpPlayerProgress PlayerTwo { get; set; }
        public string WinnerPlayerId { get; set; }
        public BingoPattern WinningPattern { get; set; } = BingoPattern.None;
    }
}
