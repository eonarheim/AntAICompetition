using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntAICompetition.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WebGrease.Css.Extensions;

namespace AntAICompetition.Server
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CellType
    {
        Space,
        Food,
        Wall,
        Hill
    }
    public class Board
    {
        public List<Ant> Ants
        {
            get { return Cells.Select(c => c.Ant).ToList(); }
        }

        private Dictionary<string, UpdateRequest> _updateList = new Dictionary<string, UpdateRequest>();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Cell[] Cells { get; set; }

        public int FogOfWar { get; set; }

        public Board(int width, int height, string mapFile = "~/App_Data/default.map")
        {
            // todo load the map file
            Width = width;
            Height = height;
            Cells = new Cell[width*height];
        }

        /// <summary>
        /// Gets the board cell by X and Y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Cell GetCell(int x, int y)
        {
            // Torus get, so cell (0,0) == (width, height)
            x = (Width + x)%Width;
            y = (Height + y)%Height;

            return Cells[x*y*Width];
        }

        /// <summary>
        /// Gets the board cell by the occupying ant
        /// </summary>
        /// <param name="ant"></param>
        /// <returns></returns>
        public Cell GetCell(Ant ant)
        {
            return GetCell(ant.X, ant.Y);
        }

        public void Update(Game game)
        {
            // Get new updates
            var upates = _updateList.Values.Where(v => v != null);

            upates.SelectMany(u => u.MoveAntRequests).ForEach(u => MoveAnt(game, u.AntId, u.Direction));
            
            // Clear updates
            _updateList.Keys.ForEach(k => _updateList[k] = null);
        }

        public void SpawnAnt(int x, int y, string player)
        {
            GetCell(x, y).Ant = new Ant()
            {
                Id = 0,
                Owner = player,
                X = x,
                Y = y
            };
        }

        private void MoveAnt(Game game, int antId, string direction)
        {
            var ant = Ants.FirstOrDefault(a => a.Id == antId);
            if (ant !=null)
            {
                var newX = ant.X;
                var newY = ant.Y;
                if (direction == "up")
                {
                    newY = ant.Y-1;
                }
                else if (direction == "down")
                {
                    newY = ant.Y+1;
                }
                else if (direction == "left")
                {
                    newX = ant.X-1;
                }
                else if (direction == "right")
                {
                    newX =ant.X+1;
                }
                var potentialCell = GetCell(newX, newY);
                if (potentialCell.Ant != null && ant.Owner != potentialCell.Ant.Owner)
                {
                    Kill(potentialCell.Ant);
                    Kill(ant);
                }
                if (potentialCell.Type == CellType.Wall)
                {
                    //do nothing it's a wall
                    //todo report error back through api
                }
                if (potentialCell.Type == CellType.Food)
                {
                    potentialCell.Type = CellType.Space;
                    game.CollectFood(ant.Owner);
                }

                // If the space is movable or a hill move the ant
                if (potentialCell.Type == CellType.Space || potentialCell.Type == CellType.Hill)
                {
                    //todo implement win condition when an ant steps on an enemy hill
                    ant.X = newX;
                    ant.Y = newY;
                }

            }
            
        }

        private void Kill(Ant ant)
        {
            GetCell(ant.X, ant.Y).Ant = null;
        }

        public void QueueUpdateForPlayer(string playeName, UpdateRequest updateRequest)
        {
            // todo why do I tend to solve problems with dictionaries, maybe too much javascript :P
            if (!_updateList.ContainsKey(playeName))
            {
                _updateList.Add(playeName, updateRequest);
            }
            _updateList[playeName] = updateRequest;
        }
    }
}