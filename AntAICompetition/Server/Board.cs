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

        public List<Cell> Walls
        {
            get { return Cells.Where(c => c.Type == CellType.Wall).ToList(); }
        } 

        private ConcurrentDictionary<string, UpdateRequest> _updateList = new ConcurrentDictionary<string, UpdateRequest>();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Cell[] Cells { get; set; }

        public int FogOfWar { get; set; }

        public Dictionary<string, Hill> Hills = new Dictionary<string, Hill>();

        public Board(int width, int height, int FogOfWarDistance = 10, string mapFile = "~/App_Data/default.map")
        {
            // todo load the map file
            Width = width;
            Height = height;
            FogOfWar = FogOfWarDistance;
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
            
            //updates.SelectMany(u => u.MoveAntRequests).ForEach(u => MoveAnt(game, u.AntId, u.Direction));
            updates.SelectMany(u => u.MoveAntRequests).ForEach(u => UpdateAnt(game, u.AntId, u.Direction));
            updates.SelectMany(u => u.MoveAntRequests).ForEach(u => EvaluateAnts(game));
            
            
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

        private void UpdateAnt(Game game, int antId, string direction)
        {
            var ant = Ants.FirstOrDefault(a => a.Id == antId);
            if (ant != null)
            {
                var newX = ant.X;
                var newY = ant.Y;
                if (direction == "up")
                {
                    newY = (Height + ant.Y - 1)%Height;
                }
                else if (direction == "down")
                {
                    newY = (Height + ant.Y + 1)%Height;
                }
                else if (direction == "left")
                {
                    newX = (Width + ant.X - 1)%Width;
                }
                else if (direction == "right")
                {
                    newX = (Width + ant.X + 1)%Width;
                }

                var potentialCell = GetCell(newX, newY);
                if (potentialCell.Type == CellType.Wall)
                {
                    //do nothing it's a wall
                    //todo report error back through api
                    return;
                }
                

                ant.X = newX;
                ant.Y = newY;
            }
        }

        private void EvaluateAnts(Game game)
        {
            // Kill ants 
            var allAnts = this.Ants.ToList();
            foreach (var ant in allAnts)
            {
                var matches = allAnts.FindAll(a => a != ant && a.X == ant.X && a.Y == ant.Y);
                if (matches.Count > 0)
                {
                    matches.ForEach(Kill);
                    Kill(ant);
                }
            }

            // Update positions
            Cells.ForEach(c => c.Ant = null);
            allAnts.ForEach(a => GetCell(a.X, a.Y).Ant = a);

            // Collect food
            allAnts.Where(a => GetCell(a.X, a.Y).Type == CellType.Food).ForEach(a =>
            {
                game.CollectFood(a.Owner);
                GetCell(a.X, a.Y).Type = CellType.Space;
            });

            // Check for win condition
            var winningAnt = allAnts.FirstOrDefault(
                a => GetCell(a.X, a.Y).Type == CellType.Hill && Hills[a.Owner].X != a.X && Hills[a.Owner].Y != a.Y);
            if (winningAnt != null)
            {
                game.Win(winningAnt.Owner);
            }


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
                    return;
                }
                if (potentialCell.Type == CellType.Wall)
                {
                    //do nothing it's a wall
                    //todo report error back through api
                    return;
                }
                if (potentialCell.Type == CellType.Food)
                {
                    potentialCell.Type = CellType.Space;
                    game.CollectFood(ant.Owner);
                }

                // If the space is movable or a hill move the ant
                if (potentialCell.Type == CellType.Space || potentialCell.Type == CellType.Hill)
                {
                    GetCell(ant.X, ant.Y).Ant = null;
                    ant.X = newX;
                    ant.Y = newY;
                    GetCell(newX, newY).Ant = ant;
                    if (potentialCell.Type == CellType.Hill && Hills[ant.Owner].X != potentialCell.X && Hills[ant.Owner].Y != potentialCell.Y)
                    {
                        game.Win(ant.Owner);
                    }
                }

            }
        }

        public List<Food> GetVisibleFood(string playerName)
        {
            var result = new List<Food>();
            var friendlies = GetAllFriendlyAnts(playerName);
            var foods = Cells.Where(c => c.Type == CellType.Food).Select(c => new Food(c.X, c.Y)).ToList();
            foreach (var friendly in friendlies)
            {
                foreach (var food in foods)
                {
                    if (friendly.GetDistance(food) <= FogOfWar && !result.Contains(food))
                    {
                        result.Add(food);
                    }
                }
            }
            return result;
        } 

        public List<Ant> GetAllFriendlyAnts(string playerName)
        {
            return Ants.ToList().Where(a => a!= null && a.Owner == playerName).ToList();
        }

        public List<Ant> GetAllEnemyAnts(string playerName)
        {
            return Ants.ToList().Where(a => a!=null && a.Owner != playerName).ToList();
        } 

        public List<Ant> GetVisibleEnemyAnts(string playerName)
        {
            var result = new List<Ant>();
            var friendlies = GetAllFriendlyAnts(playerName);
            var enemies = GetAllEnemyAnts(playerName);
            // double for loop of DOOOOOOOOM!
            // todo there is probably a smarter way to do this calculation
            foreach (var playerAnt in friendlies)
            {
                foreach (var enemyAnt in enemies)
                {
                    // we are in range and we haven't seen this ant yet
                    if (playerAnt.GetDistance(enemyAnt) <= FogOfWar && !result.Contains(enemyAnt))
                    {
                        result.Add(enemyAnt);
                    } 
                }
            }

            return result;
        }

        public List<Hill> GetVisibleEnemyHills(string playerName)
        {
            var result = new List<Hill>();

            var friendlies = GetAllFriendlyAnts(playerName);
            var enemyHills = Hills.Values.Where(h => h.Owner != playerName);
            foreach (var friendly in friendlies)
            {
                foreach (var enemyHill in enemyHills)
                {
                    if (friendly.GetDistance(enemyHill) <= FogOfWar && !result.Contains(enemyHill))
                    {
                        result.Add(enemyHill);
                    }
                }
            }

            return result; 
        }
        public List<Cell> GetVisibleWalls(string playerName)
        {
            var result = new List<Cell>();
            var friendlies = GetAllFriendlyAnts(playerName);
            var walls = Walls;
            foreach (var friendly in friendlies)
            {
                foreach (var wall in walls)
                {
                    if (friendly.GetDistance(wall) <= FogOfWar && !result.Contains(wall))
                    {
                        result.Add(wall);
                    }
                }
            }

            return result;
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