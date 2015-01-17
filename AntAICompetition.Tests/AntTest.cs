using System;
using AntAICompetition.Models;
using AntAICompetition.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AntAICompetition.Tests
{
    [TestClass]
    public class AntTest
    {
        [TestMethod]
        public void TestDistanceWorksAsExpectedInX()
        {
            var ant1 = new Ant(){X = 0, Y = 0};
            var ant2 = new Ant(){X = 2, Y = 0};

            var distance = ant1.GetDistance(ant2);

            // The 2 ants should be 2 units away
            Assert.AreEqual(2, distance, .001);
        }

        [TestMethod]
        public void TestDistanceWorksAsExpectedInY()
        {
            var ant1 = new Ant() { X = 0, Y = 0 };
            var ant2 = new Ant() { X = 0, Y = 6 };

            var distance = ant1.GetDistance(ant2);

            // The 2 ants should be 6 units away
            Assert.AreEqual(6, distance, .001);
        }
        [TestMethod]
        public void TestDistanceWorksAsExpectedInXandY()
        {
            var ant1 = new Ant() { X = 0, Y = 0 };
            var ant2 = new Ant() { X = 3, Y = 4 };

            var distance = ant1.GetDistance(ant2);

            // The 2 ants should be 4 units away
            Assert.AreEqual(5, distance, .001);
        }

        [TestMethod]
        public void TestDistanceWorksAroundTheTorusInX()
        {
            var ant1 = new Ant() { X = 0, Y = 0 };
            var ant2 = new Ant() { X = Game.DEFAULT_WIDTH-1, Y = 0 };

            var distance = ant1.GetDistance(ant2);

            // The ants should be only 1 units away around the torus
            Assert.AreEqual(1, distance, .001);
        }

        [TestMethod]
        public void TestDistanceWorksAroundTheTorusInY()
        {
            var ant1 = new Ant() { X = 0, Y = 0 };
            var ant2 = new Ant() { X = 0, Y = Game.DEFAULT_HEIGHT - 5};

            var distance = ant1.GetDistance(ant2);

            // The ants should be only 4 units away around the torus
            Assert.AreEqual(5, distance, .001);
        }


        [TestMethod]
        public void TestDistanceWorksAroundTheTorusInXAndY()
        {
            var ant1 = new Ant() { X = 0, Y = 0 };
            var ant2 = new Ant() { X = Game.DEFAULT_HEIGHT - 3, Y = Game.DEFAULT_HEIGHT - 4 };

            var distance = ant1.GetDistance(ant2);

            // The ants should be only 4 units away around the torus
            Assert.AreEqual(5, distance, .001);
        }


    }
}
