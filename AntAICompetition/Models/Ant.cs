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
            return Math.Sqrt(Math.Pow(this.X - other.X, 2) + Math.Pow(this.Y - other.Y, 2));
        }

    }
}