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

        private const double _chanceToSpawnFood = .45;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Cell[] Cells { get; set; }

        public int FogOfWar { get; set; }

        public Dictionary<string, Hill> Hills = new Dictionary<string, Hill>();

        public Board(int width, int height, int FogOfWarDistance = 10, string mapFile = "~/App_Data/default.map")
        {
            var mapString = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath(mapFile));
            var map = JsonConvert.DeserializeObject<Map>(mapString); // I love Json.net, James Newton-King you rock +20 gold stars

            Width = map.Width;
            Height = map.Height;
            FogOfWar = map.FogOfWar;
            Cells = new Cell[Width*Height];
            for (var i = 0; i < Cells.Length; i++)
            {
                Cells[i] = new Cell();
            }
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    
                    GetCell(i, j).X = i;
                    GetCell(i, j).Y = j;
                }
            }

            if (map.SymmetricHills)
            {
                // todo hills need to be a member off of board not game :(
            }

            map.WallLocations.ForEach(w =>
            {
                GetCell(w.X, w.Y).Type = CellType.Wall;
            });
            if (map.SymmetricWalls)
            {
                map.WallLocations.ForEach(w =>
                {
                    GetCell(Width - w.X - 1, w.Y).Type = CellType.Wall;
                    GetCell(w.X, Height - w.Y - 1).Type = CellType.Wall;
                    GetCell(Width - w.X - 1, Height - w.Y - 1).Type = CellType.Wall;
                }); 
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

        private string Opposite(string direction)
        {
            switch (direction.ToLower())
            {
                case "left":
                    return "right";
                case "up":
                    return "down";
                case "down":
                    return "up";
                case "right":
                    return "left";
                default:
                    return "error";
            }
        }

        private double Distance(int antId1, int antId2)
        {
            var a1 = Ants.FirstOrDefault(a => a.Id == antId1);
            var a2 = Ants.FirstOrDefault(a => a.Id == antId2);
            if (a1 == null || a2 == null) return 9999999;
            return a1.GetDistance(a2);
        }

        private bool WillSwap(MoveAntRequest m1, MoveAntRequest m2)
        {
            var a1 = Ants.FirstOrDefault(a => a.Id == m1.AntId);
            var a2 = Ants.FirstOrDefault(a => a.Id == m2.AntId);
            if (a1 == null || a2 == null) return false;

            // what if positions
            var a1x = (a1.MoveX(m1.Direction) + Width) % Width;
            var a1y = (a1.MoveY(m1.Direction) + Height) % Height;
            var a2x = (a2.MoveX(m2.Direction) + Width) % Width;
            var a2y = (a2.MoveY(m2.Direction) + Height) % Height;

            // if they are converse they have swapped
            return a1x == a2.X && a1y == a2.Y && a2x == a1.X && a2y == a1.Y;

        }
        public void Update(Game game)
        {
            // Get new player updates
            var updates = _updateList.Values.Where(v => v != null);
            
            var updateRequests = updates.SelectMany(u => u.MoveAntRequests).ToList();

           

            // move and evaluate
            updateRequests.ForEach(u => UpdateAnt(game, u.AntId, u.Direction));
            // detect ant swaps
            // ants that are moving in opposite directions that are 1 unit away are swapping :)
            var swappedAntIds = new List<int>();
            foreach (var update in updateRequests)
            {
                var swaps = updateRequests.Where(u => u != update && WillSwap(u, update)).Select(u => u.AntId).ToList();
                swappedAntIds.AddRange(swaps);
            }
            updateRequests.ForEach(u => EvaluateAnts(game));

            // kill swaps
            swappedAntIds.ForEach(Kill);
            
            
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
            var midWidth = Width / 2;
            var midHeight = Height / 2;

            if (rng.NextDouble() < _chanceToSpawnFood)
            {
                var x1 = rng.Next(0, midWidth);
                var y1 = rng.Next(0, midHeight);
                var potentialCell = GetCell(x1, y1);
                if (potentialCell.Type != CellType.Wall)
                {
                    GetCell(x1, y1).Type = CellType.Food;
                }
            }

            if (rng.NextDouble() < _chanceToSpawnFood)
            {
                var x2 = rng.Next(midWidth, Width);
                var y2 = rng.Next(0, midHeight);
                var potentialCell = GetCell(x2, y2);
                if (potentialCell.Type != CellType.Wall)
                {
                    GetCell(x2, y2).Type = CellType.Food;
                }
            }

            if (rng.NextDouble() < _chanceToSpawnFood)
            {
                var x3 = rng.Next(0, midWidth);
                var y3 = rng.Next(midHeight, Height);
                var potentialCell = GetCell(x3, y3);
                if (potentialCell.Type != CellType.Wall)
                {
                    GetCell(x3, y3).Type = CellType.Food;
                }
            }


            if (rng.NextDouble() < _chanceToSpawnFood)
            {                
                var x4 = rng.Next(midWidth, Width);
                var y4 = rng.Next(midHeight, Height);
                var potentialCell = GetCell(x4, y4);
                if (potentialCell.Type != CellType.Wall)
                {
                    GetCell(x4, y4).Type = CellType.Food;
                }
            }
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

                
                // detect walls
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
            var killedAnts = new List<Ant>();
            foreach (var ant in allAnts)
            {
                var matches = allAnts.Where(a => a != ant && a.X == ant.X && a.Y == ant.Y).ToList();
                if (matches.Count > 0)
                {
                    killedAnts.Add(ant);
                    killedAnts.AddRange(matches);
                }
            }

            // Update positions
            var newAnts = allAnts.Where(a => !killedAnts.Contains(a)).ToList();
            Cells.ForEach(c => c.Ant = null);
            
            newAnts.ForEach(a => GetCell(a.X, a.Y).Ant = a);

            // Collect food
            newAnts.Where(a => GetCell(a.X, a.Y).Type == CellType.Food).ForEach(a =>
            {
                game.CollectFood(a.Owner);
                GetCell(a.X, a.Y).Type = CellType.Space;
            });

            // Check for win condition
            var winningAnt = newAnts.FirstOrDefault(
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

        private void Kill(int antId)
        {
            var ant = Ants.FirstOrDefault(a => a.Id == antId);
            if (ant != null)
            {
                Kill(ant);
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