using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntAICompetition.Server;

namespace AntAICompetition.Models
{
    public class GameStatus
    {
        public bool IsGameOver { get; set; }
        public string Status { get; set; }
        public int GameId { get; set; }
        public int Turn { get; set; }
        public int TotalFood { get; set; }
        public Hill Hill { get; set; }
        public int FogOfWar { get; set; }
        public long MillisecondsUntilNextTurn { get; set; }

        public List<Ant> FriendlyAnts { get; set; }
        public List<Ant> EnemyAnts { get; set; }
        public List<Hill> EnemyHills { get; set; } 
        public List<Food> VisibleFood { get; set; }
        public List<Cell> Walls { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}