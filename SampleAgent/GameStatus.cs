using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAgent
{
    public class GameStatus
    {
        public int GameId { get; set; }
        public int Turn { get; set; }
        public int Food { get; set; }
        public int FogOfWar { get; set; }
        public long MillisecondsUntilNextTurn { get; set; }
        public List<Ant> FriendlyAnts { get; set; }
        public List<Ant> EnemyAnts { get; set; }

    }
}
