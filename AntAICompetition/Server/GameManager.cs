using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace AntAICompetition.Server
{
    public class GameManager
    {
        private int _currentTourneyId = -1;

        private GameManager()
        {
            Games = new Dictionary<int, Game>();
        }

        private static readonly GameManager _instance = new GameManager();

        public Dictionary<int, Game> Games { get; set; }

        public void KillGame(int id)
        {
            if (Games.ContainsKey(id))
            {
                Games[id].Stop();
                Games.Remove(id);
            }
        }

        public Game GetTourneyGame()
        {
            // only works for two player games
            if (_currentTourneyId == -1 || (Games[_currentTourneyId].Players.Count%2 == 0))
            {
                var game = new Game(gameStartDelay: 20000);
                game.Start();
                _currentTourneyId = game.Id;
                Games.Add(game.Id, game);
                return game;
            }
            else
            {
                return Games[_currentTourneyId];
            }
        }

        public Game GetGame(int? id)
        {
            if (id.HasValue)
            {
                if (Games.ContainsKey(id.Value))
                {
                    return Games[id.Value];
                }
                return null;
            }
            var game = new Game();
            game.Start();
            Games.Add(game.Id, game);
            return game;
        }

        public Game GetDemoGame()
        {
            var game = new Game();
            game.Start();
            Games.Add(game.Id, game);
            return game;
        }

        public static GameManager Instance
        {
            get { return _instance; }
        }
    }
}