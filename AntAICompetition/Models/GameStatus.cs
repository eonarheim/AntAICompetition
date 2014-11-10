using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntAICompetition.Models
{
    public class GameStatus
    {
        public string GameId { get; set; }
        public int Turn { get; set; }
        public int MillisecondsUntilNextTurn { get; set; }
        public List<Ant> FriendlyAnts { get; set; }
        public List<Ant> EnemyAnts { get; set; }



    }
}