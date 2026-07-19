using System;
using System.Collections.Generic;
using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Objectives;

namespace BingoQuest.Gameplay.Social
{
    public sealed class GhostBoardService
    {
        private readonly Dictionary<string, List<GhostRunRecord>> _runsByBoard = new();

        public void SubmitRun(GhostRunRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.PlayerId) || string.IsNullOrWhiteSpace(record.RegionId))
                throw new ArgumentException("Ghost run record is invalid.");

            var boardKey = BuildBoardKey(record.RegionId, record.CardSeed);
            if (!_runsByBoard.TryGetValue(boardKey, out var runs))
            {
                runs = new List<GhostRunRecord>();
                _runsByBoard[boardKey] = runs;
            }

            runs.Add(record);
            runs.Sort((a, b) => a.CompletionSeconds.CompareTo(b.CompletionSeconds));
        }

        public IReadOnlyList<GhostRunRecord> GetLeaderboard(string regionId, int cardSeed, int top = 10)
        {
            var boardKey = BuildBoardKey(regionId, cardSeed);
            if (!_runsByBoard.TryGetValue(boardKey, out var runs))
                return Array.Empty<GhostRunRecord>();

            int take = Math.Max(0, Math.Min(top, runs.Count));
            return runs.GetRange(0, take);
        }

        public GhostRunRecord GetBestGhost(string regionId, int cardSeed)
        {
            var boardKey = BuildBoardKey(regionId, cardSeed);
            if (!_runsByBoard.TryGetValue(boardKey, out var runs) || runs.Count == 0)
                return null;

            return runs[0];
        }

        private static string BuildBoardKey(string regionId, int cardSeed) => $"{regionId}:{cardSeed}";
    }

    public sealed class GuildBoardService
    {
        public GuildBoardState CreateBoard(string guildId, int size = 20)
        {
            if (string.IsNullOrWhiteSpace(guildId))
                throw new ArgumentException("Guild id is required.");
            if (size < 3)
                throw new ArgumentOutOfRangeException(nameof(size), "Guild board size must be at least 3.");

            var state = new GuildBoardState
            {
                GuildId = guildId,
                Size = size
            };

            state.Milestones.Add(new GuildMilestone { MilestoneId = "line-1", Label = "First Line", RequiredLines = 1 });
            state.Milestones.Add(new GuildMilestone { MilestoneId = "line-3", Label = "Triple Line", RequiredLines = 3 });
            state.Milestones.Add(new GuildMilestone { MilestoneId = "line-6", Label = "War Board", RequiredLines = 6 });
            return state;
        }

        public void ContributeSquare(GuildBoardState state, string memberId, int row, int col)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (string.IsNullOrWhiteSpace(memberId))
                throw new ArgumentException("Member id is required.");
            if (row < 0 || row >= state.Size || col < 0 || col >= state.Size)
                throw new ArgumentOutOfRangeException($"Square ({row},{col}) is outside the guild board.");

            var key = $"{row}:{col}";
            if (!state.CompletedSquares.Add(key))
                return;

            if (!state.Contributions.TryGetValue(memberId, out var contribution))
            {
                contribution = new GuildMemberContribution { MemberId = memberId };
                state.Contributions[memberId] = contribution;
            }

            contribution.SquaresContributed++;

            int currentLines = CountCompletedLines(state);
            foreach (var milestone in state.Milestones)
            {
                if (!milestone.IsUnlocked && currentLines >= milestone.RequiredLines)
                {
                    milestone.IsUnlocked = true;
                    contribution.PatternsContributed++;
                }
            }
        }

        public int CountCompletedLines(GuildBoardState state)
        {
            int lines = 0;

            for (int row = 0; row < state.Size; row++)
            {
                bool complete = true;
                for (int col = 0; col < state.Size; col++)
                {
                    if (!state.CompletedSquares.Contains($"{row}:{col}"))
                    {
                        complete = false;
                        break;
                    }
                }

                if (complete)
                    lines++;
            }

            for (int col = 0; col < state.Size; col++)
            {
                bool complete = true;
                for (int row = 0; row < state.Size; row++)
                {
                    if (!state.CompletedSquares.Contains($"{row}:{col}"))
                    {
                        complete = false;
                        break;
                    }
                }

                if (complete)
                    lines++;
            }

            return lines;
        }
    }

    public sealed class PvpBingoWarService
    {
        public PvpBingoMatchState CreateMatch(string playerOneId, string playerTwoId, IReadOnlyList<string> objectiveIds, int gridSize = 5)
        {
            if (string.IsNullOrWhiteSpace(playerOneId) || string.IsNullOrWhiteSpace(playerTwoId))
                throw new ArgumentException("Both player ids are required.");
            if (objectiveIds == null || objectiveIds.Count != gridSize * gridSize)
                throw new ArgumentException("Objective count must equal gridSize * gridSize.");

            var state = new PvpBingoMatchState
            {
                GridSize = gridSize,
                PlayerOne = new PvpPlayerProgress { PlayerId = playerOneId, CompletedObjectives = new bool[objectiveIds.Count] },
                PlayerTwo = new PvpPlayerProgress { PlayerId = playerTwoId, CompletedObjectives = new bool[objectiveIds.Count] }
            };

            state.ObjectiveIds.AddRange(objectiveIds);
            return state;
        }

        public void CompleteObjective(PvpBingoMatchState state, string playerId, string objectiveId)
        {
            if (state == null || !string.IsNullOrWhiteSpace(state.WinnerPlayerId))
                return;

            var progress = GetPlayerProgress(state, playerId);
            if (progress == null)
                throw new ArgumentException("Player is not part of this match.");

            int index = state.ObjectiveIds.IndexOf(objectiveId);
            if (index < 0)
                throw new ArgumentException("Objective does not exist in this match.");

            progress.CompletedObjectives[index] = true;
            EvaluateWin(state, progress);
        }

        private void EvaluateWin(PvpBingoMatchState state, PvpPlayerProgress progress)
        {
            var patterns = DetectPatterns(progress.CompletedObjectives, state.GridSize);
            foreach (var pattern in patterns)
                progress.CompletedPatterns.Add(pattern);

            if (patterns.Count > 0)
            {
                state.WinnerPlayerId = progress.PlayerId;
                state.WinningPattern = patterns[0];
            }
        }

        private static List<BingoPattern> DetectPatterns(bool[] completed, int gridSize)
        {
            var patterns = new List<BingoPattern>();

            for (int row = 0; row < gridSize; row++)
            {
                bool full = true;
                for (int col = 0; col < gridSize; col++)
                {
                    if (!completed[(row * gridSize) + col])
                    {
                        full = false;
                        break;
                    }
                }

                if (full)
                {
                    patterns.Add(BingoPattern.Row);
                    break;
                }
            }

            for (int col = 0; col < gridSize; col++)
            {
                bool full = true;
                for (int row = 0; row < gridSize; row++)
                {
                    if (!completed[(row * gridSize) + col])
                    {
                        full = false;
                        break;
                    }
                }

                if (full)
                {
                    patterns.Add(BingoPattern.Column);
                    break;
                }
            }

            bool left = true;
            for (int i = 0; i < gridSize; i++)
            {
                if (!completed[(i * gridSize) + i])
                {
                    left = false;
                    break;
                }
            }

            if (left)
                patterns.Add(BingoPattern.DiagonalLeft);

            bool right = true;
            for (int i = 0; i < gridSize; i++)
            {
                if (!completed[(i * gridSize) + (gridSize - 1 - i)])
                {
                    right = false;
                    break;
                }
            }

            if (right)
                patterns.Add(BingoPattern.DiagonalRight);

            return patterns;
        }

        private static PvpPlayerProgress GetPlayerProgress(PvpBingoMatchState state, string playerId)
        {
            if (state.PlayerOne.PlayerId == playerId) return state.PlayerOne;
            if (state.PlayerTwo.PlayerId == playerId) return state.PlayerTwo;
            return null;
        }
    }

    public sealed class CoopSynergyService
    {
        private readonly Dictionary<string, int> _revivesByPlayer = new();
        private readonly Dictionary<string, int> _teamComboChains = new();

        public void RecordRevive(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
                throw new ArgumentException("Player id is required.");

            if (!_revivesByPlayer.ContainsKey(playerId))
                _revivesByPlayer[playerId] = 0;

            _revivesByPlayer[playerId]++;
        }

        public void RecordCombo(string teamId)
        {
            if (string.IsNullOrWhiteSpace(teamId))
                throw new ArgumentException("Team id is required.");

            if (!_teamComboChains.ContainsKey(teamId))
                _teamComboChains[teamId] = 0;

            _teamComboChains[teamId]++;
        }

        public int GetPlayerRevives(string playerId) =>
            _revivesByPlayer.TryGetValue(playerId, out var count) ? count : 0;

        public int GetTeamComboCount(string teamId) =>
            _teamComboChains.TryGetValue(teamId, out var count) ? count : 0;

        public ObjectiveEvent CreateCoopObjectiveEvent(ObjectiveEventType type, string sourceId, int amount = 1) =>
            new ObjectiveEvent(type, sourceId, amount, ElementType.Physical, isCritical: false, inCoop: true, inPvp: false);
    }
}
