using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntAICompetition.Models;

namespace AntAICompetition.Server
{
    public class Food : ICoordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Food(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}