using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelMatchingTest;

namespace _3DScanningTest
{
    [TestClass]
    public class SceneViewTest
    {
        [TestMethod]
        public void Test_ProjectToWorld()
        {
            Image img = new Image(11, 11, 1);

            Point3Df cop = new Point3Df(0, 0, 0);
            Point3Df orient = new Point3Df(0, 0, 1);

            double xFOV = 90.0 / 180 * Math.PI;
            double yFOV = 90.0 / 180 * Math.PI;

            SceneView view = new SceneView(img, cop, orient, xFOV, yFOV);

            Point2Df pt;
            Point3Df res;
            Point3Di roundedRes;
            double ep = 1e-7;

            // Point at center of image
            pt = new Point2Df(5, 5);
            res = view.ProjectToWorld(pt, 100);

            Assert.AreEqual(new Point3Df(0, 0, 100), res, "Projection from center failed");

            // Point in top-left quadrant of image
            pt = new Point2Df(4, 2);
            res = view.ProjectToWorld(pt, 33);
            roundedRes = res.Round();

            Assert.IsFalse(
                Math.Abs(roundedRes.X - res.X) > ep ||
                Math.Abs(roundedRes.Y - res.Y) > ep ||
                Math.Abs(roundedRes.Z - res.Z) > ep,
                "Projection from top-left failed");
            Assert.AreEqual(new Point3Di(-6, 18, 33), roundedRes, 
                "Projection from top-left failed");

            // Point in bottom-right quadrant of image
            pt = new Point2Df(9, 7);
            res = view.ProjectToWorld(pt, 33);
            roundedRes = res.Round();

            Assert.IsFalse(
                Math.Abs(roundedRes.X - res.X) > ep ||
                Math.Abs(roundedRes.Y - res.Y) > ep ||
                Math.Abs(roundedRes.Z - res.Z) > ep,
                "Projection from top-left failed");
            Assert.AreEqual(new Point3Di(24, -12, 33), res.Round(), 
                "Projection from bottom-right failed");
        }
    }
}
