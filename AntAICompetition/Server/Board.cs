using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntAICompetition.Server
{
    public enum CellType
    {
        Empty,
        Wall,
        Ant,
        Hill
    }
    public class Board
    {
        public Board(int width, int height, string mapFile = "~/App_Data/default.map")
        {
            Width = width;
            Height = height;
            Cells = new string[width*height];
        }

        public string GetCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return null;

            return Cells[x*y*Width];
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public string[] Cells { get; set;}

        public int FogOfWar { get; set; }

        public static void Update()
        {
            throw new NotImplementedException();
        }
    }
}