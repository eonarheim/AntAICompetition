using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace AntAICompetition.Server
{
    public class Game
    {
        private static int maxId = 1;
        private int id = maxId++;
        private int turn = 0;
        private long gameTime = 0;
        private Timer _gameLoop;


        

        public Game()
        {
            System.Diagnostics.Debug.WriteLine("Starting game {0}...", id);
            _gameLoop = new Timer(Tick, this, 10000, 3000);

            // when auto event signals it is game over, stop the timer

        }

        public void Tick(object stateInfo)
        {
            System.Diagnostics.Debug.WriteLine("[{0}] Tick...", DateTime.Now);
        }
    }
}