using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Ajax.Utilities;
using SampleAgent;
using Ant = AntAICompetition.Models.Ant;
using LogonResult = AntAICompetition.Models.LogonResult;
using UpdateRequest = AntAICompetition.Models.UpdateRequest;
using UpdateResult = AntAICompetition.Models.UpdateResult;

namespace AntAICompetition.Server
{
    public class Game
    {
        // Game ids
        private static int _maxId = 1;
        private int _id = _maxId++;
        private int _turn = 0;
        private long gameTime = 0;

        public static readonly int DEFAULT_HEIGHT=30;
        public static readonly int DEFAULT_WIDTH = 30;

        private Timer _gameLoop;
        private Board _board = new Board(DEFAULT_HEIGHT, DEFAULT_WIDTH);

        public Board Board
        {
            get { return _board; }
        }

        public int Id
        {
            get { return _id; }
        }

        // Server agents
        // todo implement server agent

        private int _currentHill = 0;
        private List<Hill> HillLocations = new List<Hill>()
        {
            new Hill(){X=3, Y=3},
            new Hill(){X=27,Y=27},
            new Hill(){X=27,Y=3},
            new Hill(){X=3,Y=27}
        }; 


        // Players
        private int _numPlayers = 2;
        private List<string> _players = new List<string>();
        private ConcurrentDictionary<string,string> _authTokensToPlayers = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, bool> _playersUpdatedThisTurn = new ConcurrentDictionary<string, bool>();
        private ConcurrentDictionary<string, int> _playerFood = new ConcurrentDictionary<string, int>();

        // Timing
        private int _turnLength;
        private int _maxTurn;
        private DateTime _lastTick = new DateTime();
        private DateTime _nextTick = new DateTime();
        private bool _demoAgentStarted = false;

        private bool _killed = false;


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

        public Game(int gameStartDelay = 10000, int turnLength = 500, int numPlayers = 2, int maxTurn = 400)
        {
            _numPlayers = numPlayers;
            _turnLength = turnLength;
            _maxTurn = maxTurn;
            System.Diagnostics.Debug.WriteLine("Starting game {0}...", _id);

            _gameLoop = new Timer(Tick, this, gameStartDelay, _turnLength);
            _lastTick = DateTime.Now;
            _nextTick = _lastTick.AddMilliseconds(_turnLength + gameStartDelay);
            Running = true;
        }

        #region Helpers

        public Hill GetHillFromToken(string authToken)
        {
            var player = GetPlayerFromToken(authToken);
            return _board.Hills[player];
        }

        public List<Hill> GetVisibileHills(string authToken)
        {
            var player = GetPlayerFromToken(authToken);
            return _board.GetVisibleEnemyHills(player);

        }

        public List<Cell> GetVisibileWalls(string authToken)
        {
            var player = GetPlayerFromToken(authToken);
            return _board.GetVisibleWalls(player);
        } 

        public List<Ant> GetFriendlyAnts(string authToken)
        {
            var player = GetPlayerFromToken(authToken);
            return _board.GetAllFriendlyAnts(player);
        } 

        public List<Ant> GetVisibleAnts(string authToken)
        {
            var player = GetPlayerFromToken(authToken);
            return _board.GetVisibleEnemyAnts(player);
        } 

        public List<Food> GetVisibleFood(string authToken)
        {
            var player = GetPlayerFromToken(authToken);
            return _board.GetVisibleFood(player);
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

        public Hill GetNextHill()
        {
            var result = HillLocations[_currentHill];
            _currentHill = (_currentHill + 1)%HillLocations.Count;
            return result;
        }

        #endregion

        /// <summary>
        /// Stops the game
        /// </summary>
        public void Stop()
        {
            try
            {
                Running = false;
                this._killed = true;
                _gameLoop.Change(Timeout.Infinite, Timeout.Infinite);
                _gameLoop.Dispose();

                //GameManager.Instance.RemoveGame(this.Id);
            }
            catch (Exception e)
            {
                //swallow
            }
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

                var h = GetNextHill();
                _board.BuildHill(h.X, h.Y, playerName);
                CollectFood(playerName, 3);
            }
            result.GameId = Id;
            System.Diagnostics.Debug.WriteLine("Player {0} already logged on!", playerName);
            return result;
        }

        public void LogonDemoAgent()
        {
            if (!_demoAgentStarted)
            {
                _demoAgentStarted = true;
                var agentTask = Task.Factory.StartNew(() =>
                {
                    var agent = new Agent("Demo Agent");
                    agent.Start(this.Id).Wait();
                });
            }
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
            if (this._killed) return;
            _turn++;
            if (_turn >= this._maxTurn)
            {
                this.Status = "Max Turn limit reached, ties broken by most ants";
                this.Stop();
                return;
            }

            _lastTick = _nextTick;
            _nextTick = DateTime.Now.AddMilliseconds(_turnLength);
            _players.ForEach(p => _playersUpdatedThisTurn[p] = false);

            System.Diagnostics.Debug.WriteLine("[{0}] Game {3} - Tick turn {1} time to next turn {2}", DateTime.Now,
                _turn, _nextTick, Id);
            _board.Update(this);
            foreach (var player in _players)
            {
                if (_playerFood[player] > 0 && _board.CanSpawnAnt(player))
                {
                    _playerFood[player]--;
                    _board.SpawnAnt(player);
                }

                if (_board.GetAllFriendlyAnts(player).Count == 0)
                {
                    Lose(player);
                }
            }
            ClientManager.UpdateClientGame(this);
        }

        public void Win(string playerName)
        {
            this.Status = string.Format("{0} Wins!", playerName);
            this.Stop();
        }

        public void Lose(string playerName)
        {
            // todo this is wrong
            var other = this._players.FirstOrDefault(p => p != playerName);
            this.Status = string.Format("{0} Wins!", other);
            this.Stop();
        }
    }
}