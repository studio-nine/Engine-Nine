#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace Nine.Test
{
    [TestClass()]
    public class UnifromGridTest
    {
        [TestMethod()]
        public void PositionToSegmentTest()
        {
            UniformGrid grid = new UniformGrid(1.5f, 1.5f, 1, 10, 10);

            Assert.AreEqual<Point>(new Point(9, 9), grid.PositionToSegment(11.5f, 11.5f));
            Assert.AreEqual<Vector2>(new Vector2(2, 2), grid.SegmentToPosition(0, 0));
            Assert.AreEqual<Vector2>(new Vector2(1.5f, 1.5f), grid.GetSegmentBounds(0, 0).Min);
            Assert.AreEqual<Vector2>(new Vector2(11.5f, 11.5f), grid.GetSegmentBounds(9, 9).Max);
        }

        [TestMethod()]
        public void BresenhamLineTest()
        {
            int count;
            UniformGrid grid = new UniformGrid(0, 0, 2, 10, 10);

            count = 0;
            grid.Traverse(new Point(0, 9), new Point(9, 0), pt => count++ >= 0);
            Assert.AreEqual<int>(10, count);

            count = 0;
            grid.Traverse(new Point(-10, -10), new Point(100, 100), pt => count++ >= 0);
            Assert.AreEqual<int>(10, count);

            count = 0;
            grid.Traverse(Vector2.Zero, new Vector2(6, 20), 1, pt => count++ >= 0);
            Assert.IsTrue(count > 10);

            count = 0;
            grid.Traverse(Vector2.Zero, new Vector2(4, 40), 1, pt => count++ >= 0);
            Assert.IsTrue(count > 10 && count < 15);

            count = 0;
            grid.Traverse(new Ray(Vector3.One * 4.8f, Vector3.UnitX), 1, pt => count++ >= 0);
            Assert.AreEqual<int>(8, count);
        }

        [TestMethod()]
        public void TraverseInnerOutTest()
        {
            List<Point> points = new List<Point>();
            UniformGrid grid = new UniformGrid(0, 0, 2, 10, 10);

            grid.Traverse(4, 4, pt => { points.Add(pt); return true; });
        }
    }
}