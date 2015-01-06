using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntAICompetition.Server;

namespace AntAICompetition.Models
{
    public class Ant : ICoordinates
    {
        private static int _maxId = 1;

        public Ant()
        {
            Id = _maxId++;
        }

        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Owner { get; set; }

        public double GetDistance<T>(T other) where T : ICoordinates
        {

            var minCombinedFromEdgeDistanceX = Math.Min(other.X - 0, Game.DEFAULT_WIDTH - 1 - other.X) + Math.Min(X - 0, Game.DEFAULT_WIDTH - 1 - X);
            var minCombinedFromEdgeDistanceY = Math.Min(other.Y - 0, Game.DEFAULT_WIDTH - 1 - other.Y) + Math.Min(Y - 0, Game.DEFAULT_WIDTH - 1 - Y);

            return Math.Sqrt(Math.Pow(Math.Min(this.X - other.X,minCombinedFromEdgeDistanceX), 2) +
                Math.Pow(Math.Min(this.Y - other.Y, minCombinedFromEdgeDistanceY), 2));
        }

    }
}