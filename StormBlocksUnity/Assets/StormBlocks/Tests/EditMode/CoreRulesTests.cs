using System;
using System.Collections.Generic;
using NUnit.Framework;
using StormBlocks.Core;

namespace StormBlocks.Tests.EditMode
{
    public sealed class CoreRulesTests
    {
        [Test]
        public void PlacementRejectsOutOfBoundsAndCollisions()
        {
            var board = new BoardState(8);
            var line3 = new PieceDefinition("line3", new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(2, 0)
            });

            Assert.IsFalse(PlacementRules.CanPlace(board, line3, new GridPosition(6, 0)));

            board.SetOccupant(new GridPosition(1, 1), CellOccupant.Block, "single");
            Assert.IsFalse(PlacementRules.CanPlace(board, line3, new GridPosition(0, 1)));

            board.SetOccupant(new GridPosition(4, 4), CellOccupant.Storm, string.Empty);
            Assert.IsFalse(PlacementRules.CanPlace(board, line3, new GridPosition(3, 4)));
        }

        [Test]
        public void RowClearRemovesFilledRowAndScoresCells()
        {
            var board = new BoardState(8);
            for (int x = 0; x < 8; x++)
            {
                board.SetOccupant(new GridPosition(x, 2), CellOccupant.Block, "test");
            }

            var result = ClearResolver.Resolve(board, new StormRulesConfig(), new ScoringConfig(), false);

            Assert.AreEqual(1, result.Lines.Count);
            Assert.AreEqual(LineKind.Row, result.Lines[0].Kind);
            Assert.AreEqual(8, result.ClearedCells.Count);
            Assert.AreEqual(180, result.Score.Total);
            for (int x = 0; x < 8; x++)
            {
                Assert.AreEqual(CellOccupant.Empty, board.GetCell(new GridPosition(x, 2)).Occupant);
            }
        }

        [Test]
        public void ColumnClearRescuesSurvivors()
        {
            var board = new BoardState(8);
            for (int y = 0; y < 8; y++)
            {
                board.SetOccupant(new GridPosition(3, y), CellOccupant.Block, "test");
            }

            board.SetSurvivor(new GridPosition(3, 5), true);

            var result = ClearResolver.Resolve(board, new StormRulesConfig(), new ScoringConfig(), false);

            Assert.AreEqual(1, result.Lines.Count);
            Assert.AreEqual(LineKind.Column, result.Lines[0].Kind);
            Assert.AreEqual(1, result.SurvivorsRescuedAt.Count);
            Assert.AreEqual(205, result.Score.Total);
        }

        [Test]
        public void MultiLineClearAppliesComboMultiplierWithoutDoubleCountingIntersection()
        {
            var board = new BoardState(8);
            for (int x = 0; x < 8; x++)
            {
                board.SetOccupant(new GridPosition(x, 1), CellOccupant.Block, "row");
            }

            for (int y = 0; y < 8; y++)
            {
                board.SetOccupant(new GridPosition(4, y), CellOccupant.Block, "column");
            }

            var result = ClearResolver.Resolve(board, new StormRulesConfig(), new ScoringConfig(), false);

            Assert.AreEqual(2, result.Lines.Count);
            Assert.AreEqual(15, result.ClearedCells.Count);
            Assert.AreEqual(2, result.Score.ComboMultiplier);
            Assert.AreEqual(700, result.Score.Total);
        }

        [Test]
        public void StormPushbackIsAutomaticWhenClearIntersectsStorm()
        {
            var board = new BoardState(8);
            for (int x = 0; x < 7; x++)
            {
                board.SetOccupant(new GridPosition(x, 3), CellOccupant.Block, "row");
            }

            board.SetOccupant(new GridPosition(7, 3), CellOccupant.Storm, string.Empty);

            var result = ClearResolver.Resolve(board, new StormRulesConfig(), new ScoringConfig(), false);

            Assert.IsTrue(result.AutomaticPushbackTriggered);
            Assert.AreEqual(1, result.StormTilesDestroyed.Count);
            Assert.AreEqual(CellOccupant.Empty, board.GetCell(new GridPosition(7, 3)).Occupant);
            Assert.AreEqual(230, result.Score.Total);
        }

        [Test]
        public void ClutchSaveTriggersWhenPushbackHappensNearCamp()
        {
            var board = new BoardState(8);
            for (int x = 0; x < 8; x++)
            {
                board.SetOccupant(new GridPosition(x, 2), x == 4 ? CellOccupant.Storm : CellOccupant.Block, "row");
            }

            var config = new StormRulesConfig { NearDeathDistanceToCamp = 2 };
            var result = ClearResolver.Resolve(board, config, new ScoringConfig(), false);

            Assert.IsTrue(result.AutomaticPushbackTriggered);
            Assert.IsTrue(result.ClutchSave);
            Assert.AreEqual(730, result.Score.Total);
        }

        [Test]
        public void StormWarningsAndSpreadAreDeterministic()
        {
            var config = new StormRulesConfig { BaseStormSpreadCells = 4 };
            var first = new BoardState(8);
            var second = new BoardState(8);
            StormResolver.InitializeEdgeStorm(first, config);
            StormResolver.InitializeEdgeStorm(second, config);

            var firstWarnings = StormResolver.UpdateWarnings(first, config, 1234UL, 5);
            var secondWarnings = StormResolver.UpdateWarnings(second, config, 1234UL, 5);

            AssertPositionsMatch(firstWarnings.WarningCells, secondWarnings.WarningCells);
            Assert.AreEqual(4, first.CountWarnings());

            var firstSpread = StormResolver.ResolveSpread(first, config, 1234UL, 5);
            var secondSpread = StormResolver.ResolveSpread(second, config, 1234UL, 5);

            AssertPositionsMatch(firstSpread.SpreadCells, secondSpread.SpreadCells);
            Assert.AreEqual(0, first.CountWarnings());
        }

        [Test]
        public void DailySeedIsStableForSameInputsAndChangesByDate()
        {
            ulong mayFive = DailySeed.FromDate(new DateTime(2026, 5, 5), 1, 1, "storm-blocks");
            ulong mayFiveAgain = DailySeed.FromDate(new DateTime(2026, 5, 5), 1, 1, "storm-blocks");
            ulong maySix = DailySeed.FromDate(new DateTime(2026, 5, 6), 1, 1, "storm-blocks");

            Assert.AreEqual(mayFive, mayFiveAgain);
            Assert.AreNotEqual(mayFive, maySix);
        }

        [Test]
        public void RunEngineProducesSameQueueAndStormForSameSeed()
        {
            var config = new RunConfig
            {
                StormRules = new StormRulesConfig { InitialStormRingThickness = 0, WarningBeforeSpread = false },
                QueueSize = 3
            };
            var engine = new StormRunEngine(DefaultPieceLibrary.Create());

            var first = engine.StartRun(config, 99UL);
            var second = engine.StartRun(config, 99UL);

            AssertQueueMatch(first.Queue, second.Queue);

            var firstResult = engine.TryPlacePiece(first, 0, new GridPosition(0, 0));
            var secondResult = engine.TryPlacePiece(second, 0, new GridPosition(0, 0));

            Assert.IsTrue(firstResult.Success);
            Assert.IsTrue(secondResult.Success);
            Assert.AreEqual(first.Score, second.Score);
            AssertQueueMatch(first.Queue, second.Queue);
        }

        [Test]
        public void ValidMoveDetectionFindsNoMoveOnFullBoard()
        {
            var board = new BoardState(8);
            board.ForEachPosition(delegate(GridPosition position)
            {
                board.SetOccupant(position, CellOccupant.Block, "fill");
            });

            Assert.IsFalse(PlacementRules.HasAnyValidMove(board, DefaultPieceLibrary.Create()));
        }

        [Test]
        public void RunEndsWhenStormReachesCamp()
        {
            var engine = new StormRunEngine(new[]
            {
                new PieceDefinition("single", new[] { new GridPosition(0, 0) })
            });
            var state = engine.StartRun(new RunConfig
            {
                QueueSize = 1,
                StormRules = new StormRulesConfig
                {
                    InitialStormRingThickness = 0,
                    WarningBeforeSpread = false
                }
            }, 8080UL);

            state.Board.SetOccupant(new GridPosition(3, 3), CellOccupant.Storm, string.Empty);

            var result = engine.TryPlacePiece(state, 0, new GridPosition(0, 0));

            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.GameOver);
            Assert.AreEqual("Storm reached camp.", result.GameOverReason);
            Assert.IsTrue(state.IsGameOver);
        }

        [Test]
        public void RunEndsWhenNoQueuedPieceCanBePlaced()
        {
            var horizontalTwo = new PieceDefinition("h2", new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0)
            });
            var engine = new StormRunEngine(new[] { horizontalTwo });
            var state = engine.StartRun(new RunConfig
            {
                QueueSize = 1,
                StormRules = new StormRulesConfig
                {
                    BoardSize = 4,
                    InitialStormRingThickness = 0,
                    WarningBeforeSpread = false
                }
            }, 9090UL);

            FillBoardExcept(state.Board, new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(3, 0),
                new GridPosition(0, 1),
                new GridPosition(2, 1),
                new GridPosition(1, 2),
                new GridPosition(3, 2),
                new GridPosition(0, 3),
                new GridPosition(2, 3)
            });

            var result = engine.TryPlacePiece(state, 0, new GridPosition(0, 0));

            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.GameOver);
            Assert.AreEqual("No available piece can be placed.", result.GameOverReason);
            Assert.IsTrue(state.IsGameOver);
        }

        [Test]
        public void SaveSnapshotRoundTripsRunSummaryAndBoard()
        {
            var config = new RunConfig { StormRules = new StormRulesConfig { InitialStormRingThickness = 0 } };
            var engine = new StormRunEngine(DefaultPieceLibrary.Create());
            var state = engine.StartRun(config, 13579UL);
            state.Board.SetSurvivor(new GridPosition(2, 2), true);

            var snapshot = StormRunSnapshot.FromState(state);
            var payload = snapshot.ToPayload();
            var restored = StormRunSnapshot.FromPayload(payload);
            var restoredBoard = StormRunSnapshot.DecodeBoard(restored.BoardPayload);

            Assert.AreEqual(snapshot.Seed, restored.Seed);
            Assert.AreEqual(snapshot.QueueIds.Length, restored.QueueIds.Length);
            Assert.IsTrue(restoredBoard.GetCell(new GridPosition(2, 2)).HasSurvivor);
        }

        private static void AssertPositionsMatch(IReadOnlyList<GridPosition> first, IReadOnlyList<GridPosition> second)
        {
            Assert.AreEqual(first.Count, second.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i], second[i]);
            }
        }

        private static void AssertQueueMatch(IReadOnlyList<PieceDefinition> first, IReadOnlyList<PieceDefinition> second)
        {
            Assert.AreEqual(first.Count, second.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].Id, second[i].Id);
            }
        }

        private static void FillBoardExcept(BoardState board, IEnumerable<GridPosition> emptyCells)
        {
            var empty = new HashSet<GridPosition>(emptyCells);
            board.ForEachPosition(delegate(GridPosition position)
            {
                if (!empty.Contains(position))
                {
                    board.SetOccupant(position, CellOccupant.Block, "fill");
                }
            });
        }
    }
}
