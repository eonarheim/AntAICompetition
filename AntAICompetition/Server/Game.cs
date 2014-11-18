﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using AntAICompetition.Models;
using Microsoft.Ajax.Utilities;

namespace AntAICompetition.Server
{
    public class Game
    {
        // Game ids
        private static int _maxId = 1;
        private int _id = _maxId++;
        private int _turn = 0;
        private long gameTime = 0;

        private Timer _gameLoop;
        private Board _board = new Board(30, 30);

        public Board Board
        {
            get { return _board; }
        }

        public int Id
        {
            get { return _id; }
        }

        // Players
        private int _numPlayers = 2;
        private List<string> _players = new List<string>();
        private ConcurrentDictionary<string,string> _authTokensToPlayers = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, bool> _playersUpdatedThisTurn = new ConcurrentDictionary<string, bool>();
        private ConcurrentDictionary<string, int> _playerFood = new ConcurrentDictionary<string, int>();

        // Timing
        private int _turnLength;
        private DateTime _lastTick = new DateTime();
        private DateTime _nextTick = new DateTime();


        // Public Properties
        public string Status { get; set; }
        public string Name { get; set; }
        public List<string> Players
        {
            get { return _players; }
        }

        public int Turn
        {
            get { return _turn; }
        }

        public bool Running { get; private set; }

        public Game(int gameStartDelay = 10000, int turnLength = 2000, int numPlayers = 2)
        {
            _numPlayers = numPlayers;
            _turnLength = turnLength;
            System.Diagnostics.Debug.WriteLine("Starting game {0}...", _id);
            _gameLoop = new Timer(Tick, this, gameStartDelay, _turnLength);
            _lastTick = DateTime.Now;
            _nextTick = _lastTick.AddMilliseconds(_turnLength+gameStartDelay);

        }

        public Hill GetHillFromToken(string authToken)
        {
            var player = GetPlayerFromToken(authToken);
            return _board.Hills[player];
        }

        public int GetFoodFromToken(string authToken)
        {
            var result = 0;
            var player = GetPlayerFromToken(authToken);
            if (_playerFood.TryGetValue(player, out result))
            {
                return result;
            }
            return 0;
        }
        public string GetPlayerFromToken(string authToken)
        {
            var result = "";
            if (_authTokensToPlayers.TryGetValue(authToken, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Stops the game
        /// </summary>
        public void Stop()
        {
            Running = false;
            _gameLoop.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {

        }

        public void CollectFood(string player, int amount = 1)
        {
            _playerFood[player]+= amount;
        }

        /// <summary>
        /// Logs player with a certain name into the game and returns an authorization token
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public LogonResult LogonPlayer(string playerName)
        {
            var result = new LogonResult();
            if (!_players.Contains(playerName))
            {
                _players.Add(playerName);
                // I know guids are not crypto secure, for this game I don't think it matters
                var newAuthToken = System.Guid.NewGuid().ToString();
                _authTokensToPlayers.TryAdd(newAuthToken, playerName);
                _playersUpdatedThisTurn.TryAdd(playerName, false);
                _playerFood.TryAdd(playerName, 0);

                System.Diagnostics.Debug.WriteLine("Player logon [{0}]:[{1}]", playerName, newAuthToken);
                result.AuthToken = newAuthToken;
                result.GameStartTime = _nextTick;

                _board.BuildHill(3, 3, playerName);
                CollectFood(playerName, 3);
            }
            result.GameId = Id;
            System.Diagnostics.Debug.WriteLine("Player {0} already logged on!", playerName);
            return result;
        }

        /// <summary>
        /// Returns the time to the next turn in milliseconds
        /// </summary>
        /// <returns></returns>
        public long TimeToNextTurn
        {
            get { return (long) (_nextTick - DateTime.Now).TotalMilliseconds; }
        }

        /// <summary>
        /// Queues an update for a player once per turn
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="updateRequest"></param>
        /// <returns></returns>
        public UpdateResult UpdatePlayer(UpdateRequest updateRequest)
        {
            string playerName;
            if ((playerName = AuthorizePlayer(updateRequest.AuthToken)) != null && !_playersUpdatedThisTurn[playerName])
            {
                // update player
                _board.QueueUpdateForPlayer(playerName, updateRequest);
                return new UpdateResult()
                       {
                           TimeToNextTurn = TimeToNextTurn,
                           Success = true
                       };

            }
            return new UpdateResult()
            {
                TimeToNextTurn = TimeToNextTurn,
                Message = "Player was already updated this turn",
                Success = false
            };
        }

        private string AuthorizePlayer(string authToken)
        {
            if (_authTokensToPlayers.ContainsKey(authToken))
            {
                return _authTokensToPlayers[authToken];
            }
            return null;
        }

        /// <summary>
        /// Ticks the simulation forward
        /// </summary>
        /// <param name="stateInfo"></param>
        public void Tick(object stateInfo)
        {
            _turn++;
            _lastTick = _nextTick;
            _nextTick = DateTime.Now.AddMilliseconds(_turnLength);
            _players.ForEach(p => _playersUpdatedThisTurn[p] = false);
            foreach (var player in _players)
            {
                if (_playerFood[player] > 0 && _board.CanSpawnAnt(player))
                {
                    _playerFood[player]--;
                    _board.SpawnAnt(player);
                }
            }

            System.Diagnostics.Debug.WriteLine("[{0}] Game {3} - Tick turn {1} time to next turn {2}", DateTime.Now, _turn, _nextTick, Id);
            _board.Update(this);
            ClientManager.UpdateClientGame(this);
            
        }

    }
}