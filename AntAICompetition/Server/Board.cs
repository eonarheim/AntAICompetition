using System;
using System.Collections.Concurrent;
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
            get { return Cells.Where(c => c.Ant != null).Select(c => c.Ant).ToList(); }
        }

        private ConcurrentDictionary<string, UpdateRequest> _updateList = new ConcurrentDictionary<string, UpdateRequest>();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Cell[] Cells { get; set; }

        public int FogOfWar { get; set; }

        public Dictionary<string, Hill> Hills = new Dictionary<string, Hill>();

        public Board(int width, int height, string mapFile = "~/App_Data/default.map")
        {
            // todo load the map file
            Width = width;
            Height = height;
            Cells = new Cell[width*height];
            for (var i = 0; i < Cells.Length; i++)
            {
                Cells[i] = new Cell();
            }
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    
                    GetCell(i, j).X = i;
                    GetCell(i, j).Y = j;
                }
            }
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

            return Cells[x+y*Width];
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
            // Get new player updates
            var updates = _updateList.Values.Where(v => v != null);
            
            updates.SelectMany(u => u.MoveAntRequests).ForEach(u => MoveAnt(game, u.AntId, u.Direction));
            
            SpawnFood();
            
            // Clear updates
            _updateList.Clear();
        }

        public void BuildHill(int x, int y, string player)
        {
            GetCell(x, y).Type = CellType.Hill;
            Hills.Add(player, new Hill()
                              {
                                  Owner = player,
                                  X = x,
                                  Y = y
                              });
        }

        public bool CanSpawnAnt(string player)
        {
            var hill = Hills[player];
            return GetCell(hill.X, hill.Y).Ant == null;
        }

        public void SpawnAnt(string player)
        {
            var hill = Hills[player];
            GetCell(hill.X, hill.Y).Ant = new Ant()
            {
                Owner = player,
                X = hill.X,
                Y = hill.Y
            };
        }

        public void SpawnFood()
        {
            var rng = new Random(DateTime.Now.Millisecond);
            var x = rng.Next(0, Width);
            var y = rng.Next(0, Height);
            GetCell(x, y).Type = CellType.Food;
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
                    newY = (Height + ant.Y-1)%Height;
                }
                else if (direction == "down")
                {
                    newY = (Height + ant.Y+1)%Height;
                }
                else if (direction == "left")
                {
                    newX = (Width + ant.X-1)%Width;
                }
                else if (direction == "right")
                {
                    newX = (Width + ant.X+1)%Width;
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
                    GetCell(ant.X, ant.Y).Ant = null;
                    ant.X = newX;
                    ant.Y = newY;
                    GetCell(newX, newY).Ant = ant;
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
                _updateList.TryAdd(playeName, updateRequest);
            }
            _updateList[playeName] = updateRequest;
        }
    }
}