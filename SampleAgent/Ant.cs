using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAgent
{
    public class Ant : ICoordinates
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public double GetDistance<T>(T other) where T : ICoordinates
        {
            return Math.Sqrt(Math.Pow(this.X - other.X, 2) + Math.Pow(this.Y - other.Y, 2));
        }
    }
}
