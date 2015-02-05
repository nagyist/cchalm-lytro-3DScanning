using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using Scanning;

namespace _3DScanningTest
{

    [TestClass]
    public class GeometryTest
    {
        [TestMethod]
        public void Test_RayRectangleIntersect_FromTopLeft_Miss()
        {
            
        }

        [TestMethod]
        public void Test_RayRectangleIntersect_FromTopLeft_Hit()
        {
            Rectangle rect = new Rectangle(0, 0, 21, 21);

            Point2Df rayPos;
            Point2Df rayStep;
            Geometry.RayIntersection res;

            ///////////////////////////////
            rayPos = new Point2Df(-5, -10);
            ///////////////////////////////

            // Q4: In top, out right
            rayStep = new Point2Df(1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(25, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: In top, out bottom
            rayStep = new Point2Df(0.75, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: In top, out bottom right
            rayStep = new Point2Df(2.5 / 3, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: In top left, out bottom
            rayStep = new Point2Df(0.5, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            ///////////////////////////////
            rayPos = new Point2Df(-10, -5);
            ///////////////////////////////

            // Q4: In left, out bottom
            rayStep = new Point2Df(1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(25, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: In left, out right
            rayStep = new Point2Df(1, 0.75);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: In left, out bottom right
            rayStep = new Point2Df(1, 2.5 / 3);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: In top left, out right
            rayStep = new Point2Df(1, 0.5);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            ////////////////////////////////
            rayPos = new Point2Df(-10, -10);
            ////////////////////////////////

            // Q4: In top left, out bottom right
            rayStep = new Point2Df(1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: Hit top right corner
            rayStep = new Point2Df(3, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: Hit bottom left corner
            rayStep = new Point2Df(1, 3);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit,  "Incorrect steps to exit");
        }

        [TestMethod]
        public void Test_RayRectangleIntersect_FromTopRight_Miss()
        {
            Rectangle rect = new Rectangle(0, 0, 21, 21);
            Point2Df rayPos = new Point2Df(30, -10);

            Point2Df rayStep;
            Geometry.RayIntersection res;

            // Q2: Up, left
            rayStep = new Point2Df(-1, -1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Up
            rayStep = new Point2Df(0, -1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Q1: Up, Right
            rayStep = new Point2Df(1, -1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Right
            rayStep = new Point2Df(1, 0);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Q1: Down, Right
            rayStep = new Point2Df(1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Down
            rayStep = new Point2Df(0, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Q3: Miss below
            rayStep = new Point2Df(-1, 4);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Q3: Miss above
            rayStep = new Point2Df(-4, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Left
            rayStep = new Point2Df(-1, 0);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");
        }

        [TestMethod]
        public void Test_RayRectangleIntersect_FromTopRight_Hit()
        {
            Rectangle rect = new Rectangle(0, 0, 21, 21);

            Point2Df rayPos;
            Point2Df rayStep;
            Geometry.RayIntersection res;

            ///////////////////////////////
            rayPos = new Point2Df(25, -10);
            ///////////////////////////////

            // Q4: In top, out left
            rayStep = new Point2Df(-1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(25, res.StepsToExit, "Incorrect steps to exit");

            // Q4: In top, out bottom
            rayStep = new Point2Df(-0.75, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            // Q4: In top, out bottom left
            rayStep = new Point2Df(-2.5 / 3, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            // Q4: In top right, out bottom
            rayStep = new Point2Df(-0.5, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            ///////////////////////////////
            rayPos = new Point2Df(30, -5);
            ///////////////////////////////

            // Q4: In right, out bottom
            rayStep = new Point2Df(-1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(25, res.StepsToExit, "Incorrect steps to exit");

            // Q4: In right, out left
            rayStep = new Point2Df(-1, 0.75);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            // Q4: In right, out bottom left
            rayStep = new Point2Df(-1, 2.5 / 3);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            // Q4: In top right, out left
            rayStep = new Point2Df(-1, 0.5);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            ////////////////////////////////
            rayPos = new Point2Df(30, -10);
            ////////////////////////////////

            // Q4: In top right, out bottom left
            rayStep = new Point2Df(-1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            // Q4: Hit top left corner
            rayStep = new Point2Df(-3, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit, "Incorrect steps to exit");

            // Q4: Hit bottom right corner
            rayStep = new Point2Df(-1, 3);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit, "Incorrect steps to exit");
        }

        [TestMethod]
        public void Test_RayRectangleIntersect_FromLeft_Hit()
        {
            Rectangle rect = new Rectangle(0, 0, 21, 21);

            Point2Df rayPos;
            Point2Df rayStep;
            Geometry.RayIntersection res;

            // Horizontal cases
            rayStep = new Point2Df(1, 0);

            //////////////////////////////
            rayPos = new Point2Df(-10, 0);
            //////////////////////////////

            // Horizontal step, intersect along top edge
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            ///////////////////////////////
            rayPos = new Point2Df(-10, 20);
            ///////////////////////////////

            // Horizontal step, intersect along bottom edge
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            ///////////////////////////////
            rayPos = new Point2Df(-10, 10);
            ///////////////////////////////

            // Horizontal step, intersect through middle
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            // Q1: In left, out top
            rayStep = new Point2Df(1, 0.5);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(20, res.StepsToExit,  "Incorrect steps to exit");

            // Q1: In left, out right
            rayStep = new Point2Df(1, 0.25);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: In left, out right
            rayStep = new Point2Df(1, -0.25);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: in left, out bottom
            rayStep = new Point2Df(1, -0.5);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(20, res.StepsToExit, "Incorrect steps to exit");


            // Q1: Hit top left corner
            rayStep = new Point2Df(1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit,  "Incorrect steps to exit");

            // Q4: Hit bottom left corner
            rayStep = new Point2Df(1, -1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit,  "Incorrect steps to exit");

            ///////////////////////////////
            rayPos = new Point2Df(-5, 10);
            ///////////////////////////////

            // Q1: In left, out top right
            rayStep = new Point2Df(2.5, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(2, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit, "Incorrect steps to exit");

            // Q4: In left, out bottom right
            rayStep = new Point2Df(2.5, -1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(2, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit, "Incorrect steps to exit");
        }

        [TestMethod]
        public void Test_RayRectangleIntersect_FromLeft_Miss()
        {
            Rectangle rect = new Rectangle(0, 0, 21, 21);
            Point2Df rayPos = new Point2Df(-10, 10);

            Point2Df rayStep;
            Geometry.RayIntersection res;

            // Q2: Up, left
            rayStep = new Point2Df(-1, -1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Up
            rayStep = new Point2Df(0, -1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Left
            rayStep = new Point2Df(-1, 0);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Q1: Miss above
            rayStep = new Point2Df(1, 2);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Q3: Down, Left
            rayStep = new Point2Df(-1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Down
            rayStep = new Point2Df(0, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Q4: Miss below
            rayStep = new Point2Df(1, -2);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");
        }

        [TestMethod]
        public void Test_RayRectangleIntersect_FromTop_Hit()
        {
            Rectangle rect = new Rectangle(0, 0, 21, 21);

            Point2Df rayPos;
            Point2Df rayStep;
            Geometry.RayIntersection res;

            // Vertical cases
            rayStep = new Point2Df(0, 1);

            //////////////////////////////
            rayPos = new Point2Df(0, -10);
            //////////////////////////////

            // Horizontal step, intersect along top edge
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            ///////////////////////////////
            rayPos = new Point2Df(20, -10);
            ///////////////////////////////

            // Horizontal step, intersect along bottom edge
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            ///////////////////////////////
            rayPos = new Point2Df(10, -10);
            ///////////////////////////////

            // Horizontal step, intersect through middle
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            // Q1: In left, out top
            rayStep = new Point2Df(0.5, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(20, res.StepsToExit, "Incorrect steps to exit");

            // Q1: In left, out right
            rayStep = new Point2Df(0.25, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            // Q4: In left, out right
            rayStep = new Point2Df(-0.25, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(30, res.StepsToExit, "Incorrect steps to exit");

            // Q4: in left, out bottom
            rayStep = new Point2Df(-0.5, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(20, res.StepsToExit, "Incorrect steps to exit");


            // Q1: Hit top left corner
            rayStep = new Point2Df(1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit, "Incorrect steps to exit");

            // Q4: Hit bottom left corner
            rayStep = new Point2Df(-1, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(10, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit, "Incorrect steps to exit");

            ///////////////////////////////
            rayPos = new Point2Df(10, -5);
            ///////////////////////////////

            // Q1: In left, out top right
            rayStep = new Point2Df(1, 2.5);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(2, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit, "Incorrect steps to exit");

            // Q4: In left, out bottom right
            rayStep = new Point2Df(-1, 2.5);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNotNull(res, "Ray should have intersected rectangle");
            Assert.AreEqual(2, res.StepsToEntry, "Incorrect steps to entry");
            Assert.AreEqual(10, res.StepsToExit, "Incorrect steps to exit");
        }

        [TestMethod]
        public void Test_RayRectangleIntersect_FromTop_Miss()
        {
            Rectangle rect = new Rectangle(0, 0, 21, 21);
            Point2Df rayPos = new Point2Df(10, -10);

            Point2Df rayStep;
            Geometry.RayIntersection res;

            // Q2: Up, left
            rayStep = new Point2Df(-1, -1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Up
            rayStep = new Point2Df(-1, 0);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Left
            rayStep = new Point2Df(0, -1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Q1: Miss above
            rayStep = new Point2Df(2, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Q3: Down, Left
            rayStep = new Point2Df(1, -1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Down
            rayStep = new Point2Df(1, 0);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");

            // Q4: Miss below
            rayStep = new Point2Df(-2, 1);
            res = Geometry.RayRectangleIntersect(rayPos, rayStep, rect);

            Assert.IsNull(res, "Ray should not have intersected the rectangle");
        }
    }
}
