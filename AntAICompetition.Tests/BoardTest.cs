using System;
using System.Collections.Generic;
using AntAICompetition.Models;
using AntAICompetition.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AntAICompetition.Tests
{
    [TestClass]
    public class BoardTest
    {
        [TestMethod]
        public void TestEnemyAntsDieOnHorizontalSwap()
        {
            var game = new Game();
            var board = new Board(new Map(){FogOfWar = 10, Height = 30, Width = 30, HillLocations = new List<Hill>(), WallLocations = new List<Cell>()});
            board.GetCell(0, 0).Ant = new Ant() { Id = 1, Owner = "player1", X = 0, Y = 0 };
            board.GetCell(1, 0).Ant = new Ant() { Id = 2, Owner = "player2", X = 1, Y = 0 };

            board.QueueUpdateForPlayer("player1", 
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>() { new MoveAntRequest() { AntId = 1, Direction = "right" } }
                });
            board.QueueUpdateForPlayer("player2",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>() { new MoveAntRequest() { AntId = 2, Direction = "left" } }
                });
            board.Update(game);

            // There should be no ants on the board
            Assert.AreEqual(0, board.Ants.Count);

        }
        [TestMethod]
        public void TestEnemyAntsDieOnVerticalSwap()
        {
            var game = new Game();
            var board = new Board(new Map() { FogOfWar = 10, Height = 30, Width = 30, HillLocations = new List<Hill>(), WallLocations = new List<Cell>() });
            board.GetCell(0, 0).Ant = new Ant() { Id = 1, Owner = "player1", X = 0, Y = 0 };
            board.GetCell(0, 1).Ant = new Ant() { Id = 2, Owner = "player2", X = 0, Y = 1 };

            board.QueueUpdateForPlayer("player1",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>() { new MoveAntRequest() { AntId = 1, Direction = "down" } }
                });
            board.QueueUpdateForPlayer("player2",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>() { new MoveAntRequest() { AntId = 2, Direction = "up" } }
                });
            board.Update(game);

            // There should be no ants on the board
            Assert.AreEqual(0, board.Ants.Count);
        }

        [TestMethod]
        public void TestEnemyAntsDieOnSameCell()
        {
            var game = new Game();
            var board = new Board(new Map() { FogOfWar = 10, Height = 30, Width = 30, HillLocations = new List<Hill>(), WallLocations = new List<Cell>() });
            board.GetCell(0, 0).Ant = new Ant() { Id = 1, Owner = "player1", X = 0, Y = 0 };
            board.GetCell(2, 0).Ant = new Ant() { Id = 2, Owner = "player2", X = 2, Y = 0 };

            board.QueueUpdateForPlayer("player1",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>() { new MoveAntRequest() { AntId = 1, Direction = "right" } }
                });
            board.QueueUpdateForPlayer("player2",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>() { new MoveAntRequest() { AntId = 2, Direction = "left" } }
                });
            board.Update(game);

            // There should be no ants on the board
            Assert.AreEqual(0, board.Ants.Count);
        }

        [TestMethod]
        public void TestEnemyAntsDoNotDieWhenMovingAway()
        {
            var game = new Game();
            var board = new Board(new Map() { FogOfWar = 10, Height = 30, Width = 30, HillLocations = new List<Hill>(), WallLocations = new List<Cell>() });
            board.GetCell(0, 0).Ant = new Ant() { Id = 1, Owner = "player1", X = 0, Y = 0 };
            board.GetCell(1, 0).Ant = new Ant() { Id = 2, Owner = "player2", X = 1, Y = 0 };

            board.QueueUpdateForPlayer("player1",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>() { new MoveAntRequest() { AntId = 1, Direction = "left" } }
                });
            board.QueueUpdateForPlayer("player2",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>() { new MoveAntRequest() { AntId = 2, Direction = "right" } }
                });
            board.Update(game);

            // There should be no ants on the board
            Assert.AreEqual(2, board.Ants.Count);
        }

        [TestMethod]
        public void TestEnemyAntsDoNotDieWhenMovingSame()
        {
            var game = new Game();
            var board = new Board(new Map() { FogOfWar = 10, Height = 30, Width = 30, HillLocations = new List<Hill>(), WallLocations = new List<Cell>() });
            board.GetCell(0, 0).Ant = new Ant() { Id = 1, Owner = "player1", X = 0, Y = 0 };
            board.GetCell(1, 0).Ant = new Ant() { Id = 2, Owner = "player2", X = 1, Y = 0 };

            board.QueueUpdateForPlayer("player1",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>() { new MoveAntRequest() { AntId = 1, Direction = "left" } }
                });
            board.QueueUpdateForPlayer("player2",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>() { new MoveAntRequest() { AntId = 2, Direction = "left" } }
                });
            board.Update(game);

            // There should be no ants on the board
            Assert.AreEqual(2, board.Ants.Count);
        }

        [TestMethod]
        public void TestMoreThan2Die()
        {
            var game = new Game();
            var board = new Board(new Map() { FogOfWar = 10, Height = 30, Width = 30, HillLocations = new List<Hill>(), WallLocations = new List<Cell>() });
            board.GetCell(2, 0).Ant = new Ant() { Id = 1, Owner = "player1", X = 2, Y = 0 };
            board.GetCell(1, 1).Ant = new Ant() { Id = 2, Owner = "player1", X = 1, Y = 1 };
            board.GetCell(3, 1).Ant = new Ant() { Id = 3, Owner = "player1", X = 3, Y = 1 };
            board.GetCell(2, 2).Ant = new Ant() { Id = 4, Owner = "player1", X = 2, Y = 2 };

            board.QueueUpdateForPlayer("player1",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>()
                    {
                        new MoveAntRequest() { AntId = 1, Direction = "down" },
                        new MoveAntRequest() { AntId = 2, Direction = "right" },
                        new MoveAntRequest() { AntId = 3, Direction = "left" },
                        new MoveAntRequest() { AntId = 4, Direction = "up" }
                    }
                });
          
            board.Update(game);

            // There should be no ants on the board
            Assert.AreEqual(0, board.Ants.Count);
        }

        [TestMethod]
        public void TestSendingBadDirection()
        {
            var game = new Game();
            var board = new Board(new Map() { FogOfWar = 10, Height = 30, Width = 30, HillLocations = new List<Hill>(), WallLocations = new List<Cell>() });
            board.GetCell(2, 0).Ant = new Ant() { Id = 1, Owner = "player1", X = 2, Y = 0 };

            board.QueueUpdateForPlayer("player1",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>()
                    {
                        new MoveAntRequest() { AntId = 1 }
                    }
                });

            board.Update(game);

            // There should be no ants on the board
            Assert.AreEqual(1, board.Ants.Count);
            // Should not have moved
            Assert.AreEqual(2, board.GetCell(2,0).Ant.X);
            Assert.AreEqual(0, board.GetCell(2, 0).Ant.Y);
        }

        [TestMethod]
        public void TestSendingInvalidDirection()
        {
            var game = new Game();
            var board = new Board(new Map() { FogOfWar = 10, Height = 30, Width = 30, HillLocations = new List<Hill>(), WallLocations = new List<Cell>() });
            board.GetCell(7, 7).Ant = new Ant() { Id = 1, Owner = "player1", X = 7, Y = 7 };

            board.QueueUpdateForPlayer("player1",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>()
                    {
                        new MoveAntRequest() { AntId = 1, Direction = "bogus_string" }
                    }
                });

            board.Update(game);

            // There should be 1 ant on the board
            Assert.AreEqual(1, board.Ants.Count);

            // It should still be at position 7, 7
            Assert.AreEqual(7, board.Ants[0].X);
            Assert.AreEqual(7, board.Ants[0].Y);

        }

        [TestMethod]
        public void TestNo1for2LineOfAnts()
        {
            // We have a line of ants: XOO
            // X moves right, and both Os move left.
            // Just the ants that swap should die first, instead of X being able to kill 2 ants in one move

            var game = new Game();
            var board = new Board(new Map() { FogOfWar = 10, Height = 30, Width = 30, HillLocations = new List<Hill>(), WallLocations = new List<Cell>() });
            board.GetCell(0, 0).Ant = new Ant() { Id = 1, Owner = "player1", X = 0, Y = 0 };
            board.GetCell(1, 0).Ant = new Ant() { Id = 2, Owner = "player2", X = 1, Y = 0 };
            board.GetCell(2, 0).Ant = new Ant() { Id = 3, Owner = "player2", X = 2, Y = 0 };

            board.QueueUpdateForPlayer("player1",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>()
                    {
                        new MoveAntRequest() { AntId = 1, Direction = "right" }
                    }
                });
            board.QueueUpdateForPlayer("player2",
                new UpdateRequest()
                {
                    MoveAntRequests = new List<MoveAntRequest>()
                    {
                        new MoveAntRequest() { AntId = 2, Direction = "left" },
                        new MoveAntRequest() { AntId = 3, Direction = "left" }
                    }
                });

            board.Update(game);

            // There should be 1 ant on the board
            Assert.AreEqual(1, board.Ants.Count);
        }

    }
}
