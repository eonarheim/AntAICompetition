using System;
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
        private Board _board;

        // Players
        private int _numPlayers = 2;
        private List<string> _players = new List<string>();
        private Dictionary<string,string> _authTokensToPlayers = new Dictionary<string, string>();
        private Dictionary<string, bool> _playersUpdatedThisTurn = new Dictionary<string, bool>(); 

        // Timing
        private int _turnLength;
        private DateTime _lastTick = new DateTime();
        private DateTime _nextTick = new DateTime();


        // Public Properties
        public List<string> Players
        {
            get { return _players; }
        }

        public bool Running { get; private set; }

        public Game(int gameStartDelay = 10000, int turnLength = 3000, int numPlayers = 2)
        {
            _numPlayers = numPlayers;
            _turnLength = turnLength;
            System.Diagnostics.Debug.WriteLine("Starting game {0}...", _id);
            _gameLoop = new Timer(Tick, this, gameStartDelay, _turnLength);
            _lastTick = DateTime.Now;
            _nextTick = _lastTick.AddMilliseconds(turnLength);

        }

        /// <summary>
        /// Stops the game
        /// </summary>
        public void Stop()
        {
            Running = false;
            _gameLoop.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Logs player with a certain name into the game and returns an authorization token
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public string LogonPlayer(string playerName)
        {
            if (!_players.Contains(playerName))
            {
                _players.Add(playerName);
                // I know guids are not crypto secure, for this game I don't think it matters
                var newAuthToken = System.Guid.NewGuid().ToString();
                _authTokensToPlayers.Add(newAuthToken, playerName);
                _playersUpdatedThisTurn.Add(playerName, false);
                return newAuthToken;
            }

            return "Already logged on!";
        }

        /// <summary>
        /// Returns the time to the next turn in milliseconds
        /// </summary>
        /// <returns></returns>
        public int TimeToNextTurn()
        {
            return _nextTick.Millisecond - DateTime.Now.Millisecond;
        }

        /// <summary>
        /// Queues an update for a player once per turn
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="updateRequest"></param>
        /// <returns></returns>
        public UpdateResult UpdatePlayer(string auth, UpdateRequest updateRequest)
        {
            string playerName;
            if ((playerName = AuthorizePlayer(auth)) != null && _playersUpdatedThisTurn[playerName])
            {
                // update player

            }
            return new UpdateResult()
            {
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

            System.Diagnostics.Debug.WriteLine("[{0}] Tick turn {1} time to next turn {2}", DateTime.Now, _turn, _nextTick);
            Board.Update();
            
        }

        /// <summary>
        /// Reports gamestate to clients
        /// </summary>
        public void Draw()
        {
            // todo call signal updater
        }
    }
}