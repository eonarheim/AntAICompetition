using System.Collections.Generic;

namespace SampleAgent.ApiDTOs
{
    public class GameStatus
    {
        public bool IsGameOver { get; set; }
        public int GameId { get; set; }
        public int Turn { get; set; }
        public int TotalFood { get; set; }
        public int FogOfWar { get; set; }
        public long MillisecondsUntilNextTurn { get; set; }
        public List<Ant> FriendlyAnts { get; set; }
        public List<Ant> EnemyAnts { get; set; }
        public List<Hill> EnemyHills { get; set; } 
        public List<Food> VisibleFood { get; set; } 

    }
}
