using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public Agent(string name)
        {
            Name = name;
            // connect to api
            _client = new HttpClient {BaseAddress = new Uri("http://localhost:16901")};
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
        }

        private async Task<LogonResult> Logon()
        {
            var response = await _client.PostAsync(string.Format("api/game/logon?agentName={0}", Name), null);
            var result = await response.Content.ReadAsAsync<LogonResult>();
            AuthToken = result.AuthToken;
            GameId = result.GameId;
            return result;
        }

        

        private async void UpdateGameState()
        {
            var response = await _client.PostAsync(string.Format("api/game/{0}/status/{1}", GameId, AuthToken), null);
            var result = await response.Content.ReadAsAsync<GameStatus>();
            Ants.Clear();
            EnemyAnts.Clear();
            Ants = result.FriendlyAnts;
            EnemyAnts = result.EnemyAnts;
            TimeToNextTurn = result.MillisecondsUntilNextTurn;
        }

        private async void SendUpdate(List<MoveRequest> moveRequests)
        {
            var response = await _client.PostAsJsonAsync("api/game/update", new UpdateRequest()
                                                                            {
                                                                                AuthToken = AuthToken,
                                                                                GameId = GameId,
                                                                                MoveRequests = moveRequests
                                                                            });
            var result = await response.Content.ReadAsAsync<UpdateResult>();
        }

       
        public MoveRequest MoveAnt(Ant ant)
        {
            var directionId = new Random().Next(0, 4);
            return new MoveRequest(){AntId = ant.Id, Direction = Directions[directionId]};
        }

        public void Update()
        {
            UpdateGameState();
            SendUpdate(Ants.Select(MoveAnt).ToList());
        }

        public async void Start()
        {
            await Logon();
            if (!_isRunning)
            {
                _isRunning = true;
                while (_isRunning)
                {
                    Update();
                    Thread.Sleep((int)(TimeToNextTurn - 200));
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}
