using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SampleAgent.ApiDTOs;

namespace SampleAgent
{
    public class AgentBase
    {
        private bool _isRunning = false;
        private readonly HttpClient _client = null;

        private List<MoveAntRequest> _pendingMoveRequests = new List<MoveAntRequest>();

        public AgentBase(string name, string endpoint)
        {
            Name = name;
            // connect to api
            _client = new HttpClient { BaseAddress = new Uri(endpoint) };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected async Task<LogonResult> Logon(int? gameId = null)
        {
            var response = await _client.PostAsJsonAsync("api/game/logon", new LogonRequest()
            {
                GameId = gameId,
                AgentName = Name
            });
            var result = await response.Content.ReadAsAsync<LogonResult>();
            AuthToken = result.AuthToken;
            GameId = result.GameId;
            return result;
        }

        protected async Task<GameStatus> UpdateGameState()
        {
            var response = await _client.PostAsync(string.Format("api/game/{0}/status/{1}", GameId, AuthToken), null);
            var result = await response.Content.ReadAsAsync<GameStatus>();
            TimeToNextTurn = result.MillisecondsUntilNextTurn;
            return result;
        }

        protected async Task<UpdateResult> SendUpdate(List<MoveAntRequest> moveRequests)
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

        public bool MoveAnt(Ant ant, string direction)
        {
            // Prevent dup move requests
            if (this._pendingMoveRequests.Any(m => m.AntId == ant.Id))
            {
                Console.WriteLine("WARNING! A move request has already been issued for ant {0}",ant.Id);
                return false;
            }

            this._pendingMoveRequests.Add(new MoveAntRequest()
            {
                AntId = ant.Id,
                Direction = direction
            });
            return true;
        }

        public virtual void Update(GameStatus gs)
        {
            // todo implement your agent's logic here
        }


        public async Task Start(int? gameId = null)
        {
            Logon(gameId).Wait();
            if (!_isRunning)
            {
                _isRunning = true;
                while (_isRunning)
                {

                    var gs = await UpdateGameState();
                    Update(gs);
                    var ur = await SendUpdate(this._pendingMoveRequests);
                    this._pendingMoveRequests.Clear();
                    Thread.Sleep((int)(TimeToNextTurn));
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }
        
        protected long TimeToNextTurn { get; set; }

        protected int GameId { get; set; }

        public string AuthToken { get; set; }

        public string Name { get; set; }
    }
}
