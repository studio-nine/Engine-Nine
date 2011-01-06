#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Nine;
using Microsoft.Xna.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

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
            Assert.AreEqual<Vector2>(new Vector2(1.5f, 1.5f), grid.SegmentToPositionMin(0, 0));
            Assert.AreEqual<Vector2>(new Vector2(11.5f, 11.5f), grid.SegmentToPositionMax(9, 9));
        }

        [TestMethod()]
        public void BresenhamLineTest()
        {
            List<Point> points;
            UniformGrid grid = new UniformGrid(0, 0, 2, 10, 10);
            
            points = new List<Point>(grid.Traverse(new Point(0, 9), new Point(9, 0)));
            Assert.AreEqual<int>(10, points.Count);

            points = new List<Point>(grid.Traverse(new Point(-10, -10), new Point(100, 100)));
            Assert.AreEqual<int>(10, points.Count);

            points = new List<Point>(grid.Traverse(Vector2.Zero, new Vector2(6, 20), 1));
            Assert.IsTrue(points.Count > 10);

            points = new List<Point>(grid.Traverse(Vector2.Zero, new Vector2(4, 40), 1));
            Assert.IsTrue(points.Count > 10 && points.Count < 15);

            points = new List<Point>(grid.Traverse(new Ray(Vector3.One * 4.8f, Vector3.UnitX), 1));
            Assert.AreEqual<int>(8, points.Count);
        }
    }
}
