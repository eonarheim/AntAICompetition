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
            board.GetCell(1, 0).Ant = new Ant() { Id = 2, Owner = "player2", X = 0, Y = 1 };

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
            board.GetCell(1, 0).Ant = new Ant() { Id = 2, Owner = "player2", X = 2, Y = 0 };

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


    }
}
