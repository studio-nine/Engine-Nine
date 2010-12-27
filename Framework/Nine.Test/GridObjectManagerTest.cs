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
    public class GridObjectManagerTest
    {
        [TestMethod()]
        public void BresenhamLineTest()
        {
            List<Point> points;
            UniformGrid grid = new UniformGrid(0, 0, 2, 10, 10);
            
            points = new List<Point>(grid.ForEach(new Point(0, 9), new Point(9, 0)));
            Assert.AreEqual<int>(10, points.Count);

            points = new List<Point>(grid.ForEach(new Point(-10, -10), new Point(100, 100)));
            Assert.AreEqual<int>(10, points.Count);

            points = new List<Point>(grid.ForEach(Vector2.Zero, new Vector2(3, 10), 1));
            Assert.IsTrue(points.Count > 10);

            points = new List<Point>(grid.ForEach(Vector2.Zero, new Vector2(4, 20), 1));
        }
    }
}
