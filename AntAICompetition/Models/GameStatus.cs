using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntAICompetition.Server;

namespace AntAICompetition.Models
{
    public class GameStatus
    {
        public int GameId { get; set; }
        public int Turn { get; set; }
        public int Food { get; set; }
        public Hill Hill { get; set; }
        public int FogOfWar { get; set; }
        public long MillisecondsUntilNextTurn { get; set; }
        public List<Ant> FriendlyAnts { get; set; }
        public List<Ant> EnemyAnts { get; set; }

    }
}