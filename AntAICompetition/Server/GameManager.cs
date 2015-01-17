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

        public void KillGame(int id)
        {
            if (Games.ContainsKey(id))
            {
                Games[id].Stop();
                Games.Remove(id);
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