using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntAICompetition.Server
{
    public class Map
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int FogOfWar { get; set; }
        public bool SymmetricHills { get; set; }
        public bool SymmetricWalls { get; set; }
        public List<Hill> HillLocations { get; set; }
        public List<Cell> WallLocations { get; set; }

    }
}