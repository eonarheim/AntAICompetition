using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace AntAICompetition.Server
{
    public class GameManager
    {
        private GameManager()
        {
            Games = new Dictionary<int, Game>();
        }

        private static readonly GameManager _instance = new GameManager();

        public Dictionary<int, Game> Games { get; set; }

        public void RemoveGame(int id)
        {
            if (Games.ContainsKey(id))
            {
                Games.Remove(id);
            }
        }

        public Game GetGame(int? id)
        {
            if (id.HasValue)
            {
                return Games[id.Value];
            }
            var game = new Game();
            Games.Add(game.Id, game);
            return game;
        }

        public Game GetDemoGame()
        {
            var game = new Game();
            Games.Add(game.Id, game);
            return game;
        }

        public static GameManager Instance
        {
            get { return _instance; }
        }
    }
}