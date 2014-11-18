using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntAICompetition.Models;

namespace AntAICompetition.Server
{
    public class Cell
    {
        public Cell()
        {
            Type = CellType.Space;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public CellType Type { get; set; }
        public Ant Ant { get; set; }

    }
}