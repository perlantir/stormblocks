using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace StormBlocks.Core
{
    public enum CellOccupant
    {
        Empty = 0,
        Block = 1,
        Storm = 2
    }

    public enum StormPhase
    {
        Calm = 0,
        Strategic = 1,
        Panic = 2
    }

    public enum GameModeId
    {
        EndlessStorm = 0,
        DailyStorm = 1,
        StormTrail = 2,
        TempestTrial = 3,
        Practice = 4
    }

    public enum LineKind
    {
        Row = 0,
        Column = 1
    }

    public struct GridPosition : IEquatable<GridPosition>
    {
        public readonly int X;
        public readonly int Y;

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridPosition other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return X.ToString(CultureInfo.InvariantCulture) + "," + Y.ToString(CultureInfo.InvariantCulture);
        }

        public static GridPosition operator +(GridPosition left, GridPosition right)
        {
            return new GridPosition(left.X + right.X, left.Y + right.Y);
        }
    }

    public struct BoardCell
    {
        public CellOccupant Occupant;
        public bool HasSurvivor;
        public bool IsStormWarning;
        public string PieceId;

        public bool BlocksPlacement
        {
            get { return Occupant == CellOccupant.Block || Occupant == CellOccupant.Storm; }
        }

        public bool CountsAsFilledForLine
        {
            get { return Occupant == CellOccupant.Block || Occupant == CellOccupant.Storm; }
        }
    }

    public sealed class PieceDefinition
    {
        private readonly GridPosition[] _cells;

        public PieceDefinition(string id, IEnumerable<GridPosition> cells)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Piece id is required.", "id");
            }

            var normalized = Normalize(cells);
            if (normalized.Length == 0)
            {
                throw new ArgumentException("Piece must contain at least one cell.", "cells");
            }

            Id = id;
            _cells = normalized;
        }

        public string Id { get; private set; }

        public IReadOnlyList<GridPosition> Cells
        {
            get { return _cells; }
        }

        public PieceDefinition RotateClockwise()
        {
            var rotated = new List<GridPosition>(_cells.Length);
            for (int i = 0; i < _cells.Length; i++)
            {
                var cell = _cells[i];
                rotated.Add(new GridPosition(cell.Y, -cell.X));
            }

            return new PieceDefinition(Id + "_r", rotated);
        }

        private static GridPosition[] Normalize(IEnumerable<GridPosition> cells)
        {
            if (cells == null)
            {
                throw new ArgumentNullException("cells");
            }

            var unique = new HashSet<GridPosition>();
            var list = new List<GridPosition>();
            int minX = int.MaxValue;
            int minY = int.MaxValue;

            foreach (var cell in cells)
            {
                if (!unique.Add(cell))
                {
                    continue;
                }

                list.Add(cell);
                if (cell.X < minX)
                {
                    minX = cell.X;
                }

                if (cell.Y < minY)
                {
                    minY = cell.Y;
                }
            }

            if (list.Count == 0)
            {
                return new GridPosition[0];
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = new GridPosition(list[i].X - minX, list[i].Y - minY);
            }

            list.Sort(delegate(GridPosition a, GridPosition b)
            {
                int yCompare = a.Y.CompareTo(b.Y);
                return yCompare != 0 ? yCompare : a.X.CompareTo(b.X);
            });

            return list.ToArray();
        }
    }

    public sealed class ScoringConfig
    {
        public int ClearedCell = 10;
        public int LineClear = 100;
        public int SurvivorRescued = 25;
        public int StormTileDestroyed = 50;
        public int ClutchSave = 500;
        public int PerfectSet = 250;
        public int[] ComboMultipliers = new[] { 1, 2, 3, 5, 8 };

        public int GetComboMultiplier(int linesCleared)
        {
            if (ComboMultipliers == null || ComboMultipliers.Length == 0)
            {
                return 1;
            }

            if (linesCleared <= 1)
            {
                return ComboMultipliers[0];
            }

            int index = Math.Min(linesCleared - 1, ComboMultipliers.Length - 1);
            return ComboMultipliers[index];
        }
    }

    public sealed class StormRulesConfig
    {
        public int BoardSize = 8;
        public int CalmSpreadEveryPlacements = 4;
        public int StrategicSpreadEveryPlacements = 3;
        public int PanicSpreadEveryPlacements = 2;
        public int NearDeathDistanceToCamp = 2;
        public bool WarningBeforeSpread = true;
        public bool PushbackAutomatic = true;
        public int InitialStormRingThickness = 1;
        public int BaseStormSpreadCells = 4;
        public int ComboBonusPushbackRadius = 1;
        public int ClutchBonusPushbackRadius = 1;

        public int GetSpreadInterval(StormPhase phase)
        {
            if (phase == StormPhase.Panic)
            {
                return Math.Max(1, PanicSpreadEveryPlacements);
            }

            if (phase == StormPhase.Strategic)
            {
                return Math.Max(1, StrategicSpreadEveryPlacements);
            }

            return Math.Max(1, CalmSpreadEveryPlacements);
        }
    }

    public sealed class RunConfig
    {
        public GameModeId Mode = GameModeId.EndlessStorm;
        public StormRulesConfig StormRules = new StormRulesConfig();
        public ScoringConfig Scoring = new ScoringConfig();
        public int StrategicPhasePlacement = 18;
        public int PanicPhasePlacement = 42;
        public int QueueSize = 3;

        public StormPhase GetPhase(int placements)
        {
            if (placements >= PanicPhasePlacement)
            {
                return StormPhase.Panic;
            }

            if (placements >= StrategicPhasePlacement)
            {
                return StormPhase.Strategic;
            }

            return StormPhase.Calm;
        }
    }

    public sealed class BoardState
    {
        private readonly BoardCell[,] _cells;

        public BoardState(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            Size = size;
            _cells = new BoardCell[size, size];
        }

        public int Size { get; private set; }

        public bool InBounds(GridPosition position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < Size && position.Y < Size;
        }

        public BoardCell GetCell(GridPosition position)
        {
            RequireInBounds(position);
            return _cells[position.X, position.Y];
        }

        public void SetCell(GridPosition position, BoardCell cell)
        {
            RequireInBounds(position);
            _cells[position.X, position.Y] = cell;
        }

        public void SetOccupant(GridPosition position, CellOccupant occupant, string pieceId)
        {
            var cell = GetCell(position);
            cell.Occupant = occupant;
            cell.PieceId = pieceId ?? string.Empty;
            if (occupant == CellOccupant.Storm)
            {
                cell.IsStormWarning = false;
                cell.PieceId = string.Empty;
            }

            SetCell(position, cell);
        }

        public void SetSurvivor(GridPosition position, bool hasSurvivor)
        {
            var cell = GetCell(position);
            cell.HasSurvivor = hasSurvivor;
            SetCell(position, cell);
        }

        public void SetStormWarning(GridPosition position, bool isWarning)
        {
            var cell = GetCell(position);
            if (cell.Occupant != CellOccupant.Storm)
            {
                cell.IsStormWarning = isWarning;
                SetCell(position, cell);
            }
        }

        public bool IsCampVisualCell(GridPosition position)
        {
            int lower = Size / 2 - 1;
            int upper = Size / 2;
            return position.X >= lower && position.X <= upper && position.Y >= lower && position.Y <= upper;
        }

        public int DistanceToCamp(GridPosition position)
        {
            int lower = Size / 2 - 1;
            int upper = Size / 2;
            int dx = position.X < lower ? lower - position.X : position.X > upper ? position.X - upper : 0;
            int dy = position.Y < lower ? lower - position.Y : position.Y > upper ? position.Y - upper : 0;
            return Math.Max(dx, dy);
        }

        public int CountStormTiles()
        {
            int count = 0;
            ForEachPosition(delegate(GridPosition position)
            {
                if (GetCell(position).Occupant == CellOccupant.Storm)
                {
                    count++;
                }
            });

            return count;
        }

        public int CountWarnings()
        {
            int count = 0;
            ForEachPosition(delegate(GridPosition position)
            {
                if (GetCell(position).IsStormWarning)
                {
                    count++;
                }
            });

            return count;
        }

        public void ClearWarnings()
        {
            ForEachPosition(delegate(GridPosition position)
            {
                var cell = GetCell(position);
                if (cell.IsStormWarning)
                {
                    cell.IsStormWarning = false;
                    SetCell(position, cell);
                }
            });
        }

        public void ClearCell(GridPosition position)
        {
            RequireInBounds(position);
            _cells[position.X, position.Y] = new BoardCell();
        }

        public BoardState Clone()
        {
            var clone = new BoardState(Size);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    clone._cells[x, y] = _cells[x, y];
                }
            }

            return clone;
        }

        public void ForEachPosition(Action<GridPosition> action)
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    action(new GridPosition(x, y));
                }
            }
        }

        private void RequireInBounds(GridPosition position)
        {
            if (!InBounds(position))
            {
                throw new ArgumentOutOfRangeException("position", "Position is outside the board: " + position);
            }
        }
    }

    public sealed class DeterministicRandom
    {
        private ulong _state;

        public DeterministicRandom(ulong seed)
        {
            _state = seed == 0UL ? 0x9E3779B97F4A7C15UL : seed;
        }

        private DeterministicRandom(ulong seed, bool rawState)
        {
            _state = seed;
        }

        public ulong State
        {
            get { return _state; }
        }

        public DeterministicRandom Clone()
        {
            return new DeterministicRandom(_state, true);
        }

        public uint NextUInt()
        {
            _state += 0x9E3779B97F4A7C15UL;
            ulong z = _state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return (uint)((z ^ (z >> 31)) >> 32);
        }

        public int NextInt(int exclusiveMax)
        {
            if (exclusiveMax <= 0)
            {
                throw new ArgumentOutOfRangeException("exclusiveMax");
            }

            return (int)(NextUInt() % (uint)exclusiveMax);
        }
    }

    public static class DailySeed
    {
        public static ulong FromDate(DateTime date, int seasonVersion, int rulesVersion, string salt)
        {
            string key = date.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                + "|season:" + seasonVersion.ToString(CultureInfo.InvariantCulture)
                + "|rules:" + rulesVersion.ToString(CultureInfo.InvariantCulture)
                + "|" + (salt ?? "storm-blocks");
            return StableHash64(key);
        }

        public static ulong StableHash64(string value)
        {
            unchecked
            {
                const ulong offset = 14695981039346656037UL;
                const ulong prime = 1099511628211UL;
                ulong hash = offset;
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= prime;
                }

                return hash;
            }
        }
    }

    public static class DefaultPieceLibrary
    {
        public static IReadOnlyList<PieceDefinition> Create()
        {
            return new[]
            {
                new PieceDefinition("single", new[] { new GridPosition(0, 0) }),
                new PieceDefinition("line2", new[] { new GridPosition(0, 0), new GridPosition(1, 0) }),
                new PieceDefinition("line3", new[] { new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(2, 0) }),
                new PieceDefinition("line4", new[] { new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(2, 0), new GridPosition(3, 0) }),
                new PieceDefinition("square2", new[] { new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(0, 1), new GridPosition(1, 1) }),
                new PieceDefinition("l3", new[] { new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(1, 1) }),
                new PieceDefinition("l4", new[] { new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2), new GridPosition(1, 2) }),
                new PieceDefinition("t4", new[] { new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(2, 0), new GridPosition(1, 1) }),
                new PieceDefinition("s4", new[] { new GridPosition(1, 0), new GridPosition(2, 0), new GridPosition(0, 1), new GridPosition(1, 1) }),
                new PieceDefinition("plus5", new[] { new GridPosition(1, 0), new GridPosition(0, 1), new GridPosition(1, 1), new GridPosition(2, 1), new GridPosition(1, 2) })
            };
        }
    }

    public struct ClearedLine
    {
        public readonly LineKind Kind;
        public readonly int Index;

        public ClearedLine(LineKind kind, int index)
        {
            Kind = kind;
            Index = index;
        }
    }

    public sealed class ScoreBreakdown
    {
        public int ClearedCellsScore;
        public int LineClearScore;
        public int SurvivorScore;
        public int StormScore;
        public int ClutchScore;
        public int PerfectSetScore;
        public int ComboMultiplier = 1;

        public int Total
        {
            get
            {
                int baseScore = ClearedCellsScore + LineClearScore + SurvivorScore + StormScore;
                return baseScore * ComboMultiplier + ClutchScore + PerfectSetScore;
            }
        }
    }

    public sealed class ClearResolution
    {
        public readonly List<ClearedLine> Lines = new List<ClearedLine>();
        public readonly List<GridPosition> ClearedCells = new List<GridPosition>();
        public readonly List<GridPosition> StormTilesDestroyed = new List<GridPosition>();
        public readonly List<GridPosition> SurvivorsRescuedAt = new List<GridPosition>();
        public bool AutomaticPushbackTriggered;
        public bool ClutchSave;
        public ScoreBreakdown Score = new ScoreBreakdown();
    }

    public sealed class StormSpreadResult
    {
        public readonly List<GridPosition> WarningCells = new List<GridPosition>();
        public readonly List<GridPosition> SpreadCells = new List<GridPosition>();
        public bool SpreadResolved;
    }

    public sealed class PlacementResult
    {
        public bool Success;
        public string FailureReason = string.Empty;
        public readonly List<GridPosition> PlacedCells = new List<GridPosition>();
        public ClearResolution Clear = new ClearResolution();
        public StormSpreadResult StormSpread = new StormSpreadResult();
        public bool PerfectSet;
        public bool GameOver;
        public string GameOverReason = string.Empty;

        public static PlacementResult Failed(string reason)
        {
            return new PlacementResult
            {
                Success = false,
                FailureReason = reason ?? "Placement failed."
            };
        }
    }

    public static class PlacementRules
    {
        public static bool CanPlace(BoardState board, PieceDefinition piece, GridPosition origin)
        {
            string reason;
            List<GridPosition> cells;
            return TryGetPlacementCells(board, piece, origin, out cells, out reason);
        }

        public static bool TryGetPlacementCells(BoardState board, PieceDefinition piece, GridPosition origin, out List<GridPosition> absoluteCells, out string failureReason)
        {
            if (board == null)
            {
                throw new ArgumentNullException("board");
            }

            if (piece == null)
            {
                throw new ArgumentNullException("piece");
            }

            absoluteCells = new List<GridPosition>(piece.Cells.Count);
            for (int i = 0; i < piece.Cells.Count; i++)
            {
                GridPosition absolute = origin + piece.Cells[i];
                if (!board.InBounds(absolute))
                {
                    failureReason = "Piece would leave the board.";
                    return false;
                }

                if (board.GetCell(absolute).BlocksPlacement)
                {
                    failureReason = "Piece collides with an occupied or storm cell.";
                    return false;
                }

                absoluteCells.Add(absolute);
            }

            failureReason = string.Empty;
            return true;
        }

        public static bool HasAnyValidMove(BoardState board, IEnumerable<PieceDefinition> pieces)
        {
            foreach (var piece in pieces)
            {
                for (int y = 0; y < board.Size; y++)
                {
                    for (int x = 0; x < board.Size; x++)
                    {
                        if (CanPlace(board, piece, new GridPosition(x, y)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public static class ClearResolver
    {
        public static ClearResolution Resolve(BoardState board, StormRulesConfig stormConfig, ScoringConfig scoring, bool perfectSet)
        {
            if (board == null)
            {
                throw new ArgumentNullException("board");
            }

            var result = new ClearResolution();
            bool nearDeathBeforeClear = StormResolver.IsNearDeath(board, stormConfig);
            bool[] clearRows = new bool[board.Size];
            bool[] clearColumns = new bool[board.Size];

            for (int y = 0; y < board.Size; y++)
            {
                bool full = true;
                for (int x = 0; x < board.Size; x++)
                {
                    if (!board.GetCell(new GridPosition(x, y)).CountsAsFilledForLine)
                    {
                        full = false;
                        break;
                    }
                }

                if (full)
                {
                    clearRows[y] = true;
                    result.Lines.Add(new ClearedLine(LineKind.Row, y));
                }
            }

            for (int x = 0; x < board.Size; x++)
            {
                bool full = true;
                for (int y = 0; y < board.Size; y++)
                {
                    if (!board.GetCell(new GridPosition(x, y)).CountsAsFilledForLine)
                    {
                        full = false;
                        break;
                    }
                }

                if (full)
                {
                    clearColumns[x] = true;
                    result.Lines.Add(new ClearedLine(LineKind.Column, x));
                }
            }

            if (result.Lines.Count == 0)
            {
                if (perfectSet)
                {
                    result.Score.PerfectSetScore = scoring.PerfectSet;
                }

                return result;
            }

            var cellsToClear = new HashSet<GridPosition>();
            for (int y = 0; y < board.Size; y++)
            {
                if (clearRows[y])
                {
                    for (int x = 0; x < board.Size; x++)
                    {
                        cellsToClear.Add(new GridPosition(x, y));
                    }
                }
            }

            for (int x = 0; x < board.Size; x++)
            {
                if (clearColumns[x])
                {
                    for (int y = 0; y < board.Size; y++)
                    {
                        cellsToClear.Add(new GridPosition(x, y));
                    }
                }
            }

            foreach (var position in cellsToClear)
            {
                var cell = board.GetCell(position);
                result.ClearedCells.Add(position);
                if (cell.Occupant == CellOccupant.Storm)
                {
                    result.StormTilesDestroyed.Add(position);
                }

                if (cell.HasSurvivor)
                {
                    result.SurvivorsRescuedAt.Add(position);
                }
            }

            result.AutomaticPushbackTriggered = stormConfig.PushbackAutomatic && result.StormTilesDestroyed.Count > 0;
            result.ClutchSave = result.AutomaticPushbackTriggered && nearDeathBeforeClear;

            var bonusPushbackCells = new HashSet<GridPosition>();
            if (result.AutomaticPushbackTriggered)
            {
                int radius = result.Lines.Count > 1 ? stormConfig.ComboBonusPushbackRadius : 0;
                if (result.ClutchSave)
                {
                    radius += stormConfig.ClutchBonusPushbackRadius;
                }

                if (radius > 0)
                {
                    foreach (var position in cellsToClear)
                    {
                        AddAdjacentStormCells(board, position, radius, bonusPushbackCells, cellsToClear);
                    }
                }
            }

            foreach (var position in bonusPushbackCells)
            {
                result.StormTilesDestroyed.Add(position);
                cellsToClear.Add(position);
            }

            foreach (var position in cellsToClear)
            {
                board.ClearCell(position);
            }

            result.Score.ClearedCellsScore = result.ClearedCells.Count * scoring.ClearedCell;
            result.Score.LineClearScore = result.Lines.Count * scoring.LineClear;
            result.Score.SurvivorScore = result.SurvivorsRescuedAt.Count * scoring.SurvivorRescued;
            result.Score.StormScore = result.StormTilesDestroyed.Count * scoring.StormTileDestroyed;
            result.Score.ComboMultiplier = scoring.GetComboMultiplier(result.Lines.Count);
            result.Score.ClutchScore = result.ClutchSave ? scoring.ClutchSave : 0;
            result.Score.PerfectSetScore = perfectSet ? scoring.PerfectSet : 0;
            return result;
        }

        private static void AddAdjacentStormCells(BoardState board, GridPosition origin, int radius, HashSet<GridPosition> output, HashSet<GridPosition> alreadyClearing)
        {
            for (int y = origin.Y - radius; y <= origin.Y + radius; y++)
            {
                for (int x = origin.X - radius; x <= origin.X + radius; x++)
                {
                    var position = new GridPosition(x, y);
                    if (!board.InBounds(position) || alreadyClearing.Contains(position))
                    {
                        continue;
                    }

                    if (Math.Abs(position.X - origin.X) + Math.Abs(position.Y - origin.Y) > radius)
                    {
                        continue;
                    }

                    if (board.GetCell(position).Occupant == CellOccupant.Storm)
                    {
                        output.Add(position);
                    }
                }
            }
        }
    }

    public static class StormResolver
    {
        public static void InitializeEdgeStorm(BoardState board, StormRulesConfig config)
        {
            int thickness = Math.Max(0, config.InitialStormRingThickness);
            for (int y = 0; y < board.Size; y++)
            {
                for (int x = 0; x < board.Size; x++)
                {
                    if (x < thickness || y < thickness || x >= board.Size - thickness || y >= board.Size - thickness)
                    {
                        board.SetOccupant(new GridPosition(x, y), CellOccupant.Storm, string.Empty);
                    }
                }
            }
        }

        public static bool IsNearDeath(BoardState board, StormRulesConfig config)
        {
            int minDistance = int.MaxValue;
            board.ForEachPosition(delegate(GridPosition position)
            {
                if (board.GetCell(position).Occupant == CellOccupant.Storm)
                {
                    minDistance = Math.Min(minDistance, board.DistanceToCamp(position));
                }
            });

            return minDistance <= config.NearDeathDistanceToCamp;
        }

        public static bool HasStormReachedCamp(BoardState board)
        {
            bool reached = false;
            board.ForEachPosition(delegate(GridPosition position)
            {
                if (board.IsCampVisualCell(position) && board.GetCell(position).Occupant == CellOccupant.Storm)
                {
                    reached = true;
                }
            });

            return reached;
        }

        public static StormSpreadResult UpdateWarnings(BoardState board, StormRulesConfig config, ulong seed, int turn)
        {
            var result = new StormSpreadResult();
            board.ClearWarnings();

            if (!config.WarningBeforeSpread)
            {
                return result;
            }

            var targets = GetSpreadTargets(board, config, seed, turn);
            foreach (var position in targets)
            {
                board.SetStormWarning(position, true);
                result.WarningCells.Add(position);
            }

            return result;
        }

        public static StormSpreadResult ResolveSpread(BoardState board, StormRulesConfig config, ulong seed, int turn)
        {
            var result = new StormSpreadResult();
            List<GridPosition> targets = new List<GridPosition>();

            board.ForEachPosition(delegate(GridPosition position)
            {
                if (board.GetCell(position).IsStormWarning)
                {
                    targets.Add(position);
                }
            });

            if (targets.Count == 0)
            {
                targets = GetSpreadTargets(board, config, seed, turn);
            }

            board.ClearWarnings();
            foreach (var position in targets)
            {
                var cell = board.GetCell(position);
                cell.Occupant = CellOccupant.Storm;
                cell.HasSurvivor = false;
                cell.IsStormWarning = false;
                cell.PieceId = string.Empty;
                board.SetCell(position, cell);
                result.SpreadCells.Add(position);
            }

            result.SpreadResolved = result.SpreadCells.Count > 0;
            return result;
        }

        public static List<GridPosition> GetSpreadTargets(BoardState board, StormRulesConfig config, ulong seed, int turn)
        {
            var candidates = new HashSet<GridPosition>();
            board.ForEachPosition(delegate(GridPosition position)
            {
                if (board.GetCell(position).Occupant != CellOccupant.Storm)
                {
                    return;
                }

                AddCandidate(board, candidates, new GridPosition(position.X + 1, position.Y));
                AddCandidate(board, candidates, new GridPosition(position.X - 1, position.Y));
                AddCandidate(board, candidates, new GridPosition(position.X, position.Y + 1));
                AddCandidate(board, candidates, new GridPosition(position.X, position.Y - 1));
            });

            var list = new List<GridPosition>(candidates);
            list.Sort(delegate(GridPosition a, GridPosition b)
            {
                int distanceCompare = board.DistanceToCamp(a).CompareTo(board.DistanceToCamp(b));
                if (distanceCompare != 0)
                {
                    return distanceCompare;
                }

                ulong hashA = StableSpreadHash(seed, turn, a);
                ulong hashB = StableSpreadHash(seed, turn, b);
                int hashCompare = hashA.CompareTo(hashB);
                if (hashCompare != 0)
                {
                    return hashCompare;
                }

                int yCompare = a.Y.CompareTo(b.Y);
                return yCompare != 0 ? yCompare : a.X.CompareTo(b.X);
            });

            int max = Math.Min(Math.Max(1, config.BaseStormSpreadCells), list.Count);
            if (list.Count > max)
            {
                list.RemoveRange(max, list.Count - max);
            }

            return list;
        }

        private static void AddCandidate(BoardState board, HashSet<GridPosition> candidates, GridPosition position)
        {
            if (!board.InBounds(position))
            {
                return;
            }

            var cell = board.GetCell(position);
            if (cell.Occupant == CellOccupant.Empty)
            {
                candidates.Add(position);
            }
        }

        private static ulong StableSpreadHash(ulong seed, int turn, GridPosition position)
        {
            unchecked
            {
                ulong value = seed;
                value ^= (ulong)(uint)turn * 0x9E3779B97F4A7C15UL;
                value ^= (ulong)(uint)(position.X + 31) * 0xBF58476D1CE4E5B9UL;
                value ^= (ulong)(uint)(position.Y + 97) * 0x94D049BB133111EBUL;
                return DailySeed.StableHash64(value.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    public sealed class StormRunState
    {
        public BoardState Board;
        public ulong Seed;
        public DeterministicRandom Random;
        public RunConfig Config;
        public readonly List<PieceDefinition> Queue = new List<PieceDefinition>();
        public int Score;
        public int SurvivorsRescued;
        public int StormTilesDestroyed;
        public int BestCombo;
        public int ClutchSaves;
        public int Placements;
        public int PlacementsSinceStormSpread;
        public bool IsGameOver;
        public string GameOverReason = string.Empty;

        public StormPhase Phase
        {
            get { return Config.GetPhase(Placements); }
        }
    }

    public sealed class StormRunEngine
    {
        private readonly IReadOnlyList<PieceDefinition> _pieceLibrary;

        public StormRunEngine(IReadOnlyList<PieceDefinition> pieceLibrary)
        {
            if (pieceLibrary == null || pieceLibrary.Count == 0)
            {
                throw new ArgumentException("A non-empty piece library is required.", "pieceLibrary");
            }

            _pieceLibrary = pieceLibrary;
        }

        public StormRunState StartRun(RunConfig config, ulong seed)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var board = new BoardState(config.StormRules.BoardSize);
            StormResolver.InitializeEdgeStorm(board, config.StormRules);

            var state = new StormRunState
            {
                Board = board,
                Seed = seed,
                Random = new DeterministicRandom(seed),
                Config = config
            };

            RefillQueue(state);
            StormResolver.UpdateWarnings(state.Board, config.StormRules, state.Seed, state.Placements + 1);
            return state;
        }

        public PlacementResult TryPlacePiece(StormRunState state, int queueIndex, GridPosition origin)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (state.IsGameOver)
            {
                return PlacementResult.Failed("Run is already over.");
            }

            if (queueIndex < 0 || queueIndex >= state.Queue.Count)
            {
                return PlacementResult.Failed("Queue index is invalid.");
            }

            PieceDefinition piece = state.Queue[queueIndex];
            List<GridPosition> placedCells;
            string reason;
            if (!PlacementRules.TryGetPlacementCells(state.Board, piece, origin, out placedCells, out reason))
            {
                return PlacementResult.Failed(reason);
            }

            var result = new PlacementResult { Success = true };
            foreach (var position in placedCells)
            {
                state.Board.SetOccupant(position, CellOccupant.Block, piece.Id);
                result.PlacedCells.Add(position);
            }

            state.Queue.RemoveAt(queueIndex);
            state.Placements++;
            state.PlacementsSinceStormSpread++;
            result.PerfectSet = state.Queue.Count == 0;

            result.Clear = ClearResolver.Resolve(state.Board, state.Config.StormRules, state.Config.Scoring, result.PerfectSet);
            ApplyClearStats(state, result.Clear);

            if (result.PerfectSet)
            {
                RefillQueue(state);
            }

            int spreadInterval = state.Config.StormRules.GetSpreadInterval(state.Phase);
            if (state.PlacementsSinceStormSpread >= spreadInterval)
            {
                result.StormSpread = StormResolver.ResolveSpread(state.Board, state.Config.StormRules, state.Seed, state.Placements);
                state.PlacementsSinceStormSpread = 0;
            }
            else if (state.Config.StormRules.WarningBeforeSpread && state.PlacementsSinceStormSpread == spreadInterval - 1)
            {
                result.StormSpread = StormResolver.UpdateWarnings(state.Board, state.Config.StormRules, state.Seed, state.Placements + 1);
            }
            else
            {
                state.Board.ClearWarnings();
            }

            EvaluateGameOver(state);
            result.GameOver = state.IsGameOver;
            result.GameOverReason = state.GameOverReason;
            return result;
        }

        private void RefillQueue(StormRunState state)
        {
            while (state.Queue.Count < state.Config.QueueSize)
            {
                int index = state.Random.NextInt(_pieceLibrary.Count);
                state.Queue.Add(_pieceLibrary[index]);
            }
        }

        private static void ApplyClearStats(StormRunState state, ClearResolution clear)
        {
            state.Score += clear.Score.Total;
            state.SurvivorsRescued += clear.SurvivorsRescuedAt.Count;
            state.StormTilesDestroyed += clear.StormTilesDestroyed.Count;
            if (clear.Lines.Count > state.BestCombo)
            {
                state.BestCombo = clear.Lines.Count;
            }

            if (clear.ClutchSave)
            {
                state.ClutchSaves++;
            }
        }

        private void EvaluateGameOver(StormRunState state)
        {
            if (StormResolver.HasStormReachedCamp(state.Board))
            {
                state.IsGameOver = true;
                state.GameOverReason = "Storm reached camp.";
                return;
            }

            if (!PlacementRules.HasAnyValidMove(state.Board, state.Queue))
            {
                state.IsGameOver = true;
                state.GameOverReason = "No available piece can be placed.";
            }
        }
    }

    public sealed class StormRunSnapshot
    {
        public ulong Seed;
        public int Score;
        public int SurvivorsRescued;
        public int StormTilesDestroyed;
        public int BestCombo;
        public int ClutchSaves;
        public int Placements;
        public int PlacementsSinceStormSpread;
        public bool IsGameOver;
        public string GameOverReason = string.Empty;
        public string[] QueueIds = new string[0];
        public string BoardPayload = string.Empty;

        public static StormRunSnapshot FromState(StormRunState state)
        {
            var snapshot = new StormRunSnapshot
            {
                Seed = state.Seed,
                Score = state.Score,
                SurvivorsRescued = state.SurvivorsRescued,
                StormTilesDestroyed = state.StormTilesDestroyed,
                BestCombo = state.BestCombo,
                ClutchSaves = state.ClutchSaves,
                Placements = state.Placements,
                PlacementsSinceStormSpread = state.PlacementsSinceStormSpread,
                IsGameOver = state.IsGameOver,
                GameOverReason = state.GameOverReason,
                BoardPayload = EncodeBoard(state.Board)
            };

            snapshot.QueueIds = new string[state.Queue.Count];
            for (int i = 0; i < state.Queue.Count; i++)
            {
                snapshot.QueueIds[i] = state.Queue[i].Id;
            }

            return snapshot;
        }

        public string ToPayload()
        {
            return string.Join("|", new[]
            {
                Seed.ToString(CultureInfo.InvariantCulture),
                Score.ToString(CultureInfo.InvariantCulture),
                SurvivorsRescued.ToString(CultureInfo.InvariantCulture),
                StormTilesDestroyed.ToString(CultureInfo.InvariantCulture),
                BestCombo.ToString(CultureInfo.InvariantCulture),
                ClutchSaves.ToString(CultureInfo.InvariantCulture),
                Placements.ToString(CultureInfo.InvariantCulture),
                PlacementsSinceStormSpread.ToString(CultureInfo.InvariantCulture),
                IsGameOver ? "1" : "0",
                Escape(GameOverReason),
                string.Join(",", QueueIds),
                BoardPayload
            });
        }

        public static StormRunSnapshot FromPayload(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 12)
            {
                throw new FormatException("Storm run payload has an invalid field count.");
            }

            return new StormRunSnapshot
            {
                Seed = ulong.Parse(parts[0], CultureInfo.InvariantCulture),
                Score = int.Parse(parts[1], CultureInfo.InvariantCulture),
                SurvivorsRescued = int.Parse(parts[2], CultureInfo.InvariantCulture),
                StormTilesDestroyed = int.Parse(parts[3], CultureInfo.InvariantCulture),
                BestCombo = int.Parse(parts[4], CultureInfo.InvariantCulture),
                ClutchSaves = int.Parse(parts[5], CultureInfo.InvariantCulture),
                Placements = int.Parse(parts[6], CultureInfo.InvariantCulture),
                PlacementsSinceStormSpread = int.Parse(parts[7], CultureInfo.InvariantCulture),
                IsGameOver = parts[8] == "1",
                GameOverReason = Unescape(parts[9]),
                QueueIds = parts[10].Length == 0 ? new string[0] : parts[10].Split(','),
                BoardPayload = parts[11]
            };
        }

        public static BoardState DecodeBoard(string payload)
        {
            string[] parts = payload.Split(':');
            if (parts.Length != 2)
            {
                throw new FormatException("Board payload is invalid.");
            }

            int size = int.Parse(parts[0], CultureInfo.InvariantCulture);
            string cells = parts[1];
            if (cells.Length != size * size)
            {
                throw new FormatException("Board payload cell count does not match size.");
            }

            var board = new BoardState(size);
            for (int i = 0; i < cells.Length; i++)
            {
                int x = i % size;
                int y = i / size;
                var position = new GridPosition(x, y);
                char token = cells[i];
                var cell = new BoardCell();
                if (token == 'B' || token == 'b')
                {
                    cell.Occupant = CellOccupant.Block;
                }
                else if (token == 'S')
                {
                    cell.Occupant = CellOccupant.Storm;
                }
                else if (token == 'W')
                {
                    cell.IsStormWarning = true;
                }

                if (token == 'b' || token == 'v')
                {
                    cell.HasSurvivor = true;
                }

                board.SetCell(position, cell);
            }

            return board;
        }

        private static string EncodeBoard(BoardState board)
        {
            var builder = new StringBuilder();
            builder.Append(board.Size.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            for (int y = 0; y < board.Size; y++)
            {
                for (int x = 0; x < board.Size; x++)
                {
                    var cell = board.GetCell(new GridPosition(x, y));
                    if (cell.Occupant == CellOccupant.Storm)
                    {
                        builder.Append('S');
                    }
                    else if (cell.Occupant == CellOccupant.Block)
                    {
                        builder.Append(cell.HasSurvivor ? 'b' : 'B');
                    }
                    else if (cell.HasSurvivor)
                    {
                        builder.Append('v');
                    }
                    else if (cell.IsStormWarning)
                    {
                        builder.Append('W');
                    }
                    else
                    {
                        builder.Append('.');
                    }
                }
            }

            return builder.ToString();
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("%", "%25").Replace("|", "%7C").Replace(",", "%2C");
        }

        private static string Unescape(string value)
        {
            return (value ?? string.Empty).Replace("%2C", ",").Replace("%7C", "|").Replace("%25", "%");
        }
    }
}
