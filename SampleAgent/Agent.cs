using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleAgent
{
    public class Agent
    {
        public int GameId { get; set; }
        public string AuthToken { get; set; }
        public List<Ant> Ants { get; set; }
        public List<Ant> EnemyAnts { get; set; } 
        public string Name { get; set; }

        public long TimeToNextTurn { get; set; }

        public List<string> Directions = new List<string>(){"up", "down", "left", "right"};
        private bool _isRunning = false;
        private readonly HttpClient _client = null;

        private Random rng = new Random(DateTime.Now.Millisecond);

        public Agent(string name)
        {
            Name = name;
            // connect to api
            _client = new HttpClient {BaseAddress = new Uri("http://localhost:16901/")};
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Ants = new List<Ant>();
            EnemyAnts = new List<Ant>();
            
        }

        private async Task<LogonResult> Logon()
        {
            var response = await _client.PostAsJsonAsync("api/game/logon", new LogonRequest(){AgentName = Name});
            var result = await response.Content.ReadAsAsync<LogonResult>();
            AuthToken = result.AuthToken;
            GameId = result.GameId;
            return result;
        }

        

        private async Task<GameStatus> UpdateGameState()
        {
            var response = await _client.PostAsync(string.Format("api/game/{0}/status/{1}", GameId, AuthToken), null);
            var result = await response.Content.ReadAsAsync<GameStatus>();
            Ants.Clear();
            EnemyAnts.Clear();
            Ants = result.FriendlyAnts;
            EnemyAnts = result.EnemyAnts;
            TimeToNextTurn = result.MillisecondsUntilNextTurn;
            return result;
        }

        private async Task<UpdateResult> SendUpdate(List<MoveAntRequest> moveRequests)
        {
            var response = await _client.PostAsJsonAsync("api/game/update", new UpdateRequest()
                                                                      {
                                                                          AuthToken = AuthToken,
                                                                          GameId = GameId,
                                                                          MoveAntRequests = moveRequests
                                                                      });
            var result = await response.Content.ReadAsAsync<UpdateResult>();
            return result;
        }

       
        public MoveAntRequest MoveAnt(Ant ant)
        {
            var directionId = rng.Next(0, 4);
            return new MoveAntRequest(){AntId = ant.Id, Direction = Directions[directionId]};
        }

        public void Update()
        {
            UpdateGameState().Wait();
            Console.WriteLine("Ants to update " + Ants.Count);
            SendUpdate(Ants.Select(MoveAnt).ToList()).Wait();
        }

        public void Start()
        {
            Logon().Wait();
            if (!_isRunning)
            {
                _isRunning = true;
                while (_isRunning)
                {
                    Update();
                    Thread.Sleep((int)(TimeToNextTurn));
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}
