using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SampleAgent.ApiDTOs;

namespace SampleAgent
{
    public class Agent : AgentBase
    {
        public List<string> Directions = new List<string>(){"up", "down", "left", "right"};
        private Random rng = new Random(DateTime.Now.Millisecond);

        public Agent(string name, string endPoint = "http://antsgame.azurewebsites.net/")
            : base(name, endPoint)
        {
            // todo fun ant agent initialization stuff :)
        }
        
        // Some helpers
        private string GetDirectionOfClosestFoodToAnt(Ant ant, List<Food> food)
        {
            Console.WriteLine("Looking for food in {0}", food.Count);
            Food closest = null;
            var minDistance = 9999.0;
            foreach (var f in food)
            {
                if (ant.GetDistance(f) < minDistance)
                {
                    minDistance = ant.GetDistance(f);
                    closest = f;
                }
            }
            if (closest != null)
            {
                return GetDirection(ant, closest);
            }
            else
            {
                return GetRandomDirection();
            }
        }

        private string GetRandomDirection()
        {
            return Directions[rng.Next(0, 4)];
        }

        private string GetDirection(ICoordinates from, ICoordinates to)
        {
            //Console.WriteLine("From {0},{1} to {2},{3}", @from.X, @from.Y, to.X, to.Y);
            var dirX = to.X - from.X;
            var dirY = to.Y - from.Y;
            // greatest diff in x
            var dir = "left";
            if (Math.Abs(dirX) > Math.Abs(dirY))
            {
                dir = dirX > 0 ? "right" : "left";
            }
            else
            {
                dir = dirY > 0 ? "down" : "up";
            }
            //Console.WriteLine("Going {0}", dir);
            return dir;
        }


        // Override the Update method 
        public override void Update(GameStatus gs)
        {
            // todo implement your awesome agent here :)
            // Call MoveAnt(ant, direction) to move ants in the simulation
            gs.FriendlyAnts.ForEach(a => MoveAnt(a, GetDirectionOfClosestFoodToAnt(a, gs.VisibleFood)));
            Console.WriteLine("Current Turn {1} : Time to next turn {0}", this.TimeToNextTurn, gs.Turn);

            // Updates are sent to the server automagically at the end of update!
        }
    }
}
