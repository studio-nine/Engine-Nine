#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
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
    public class QuadTreeTest
    {
        [TestMethod()]
        public void AddRemoveTest()
        {
            object a = new object();
            object b = new object();

            QuadTree<object> tree = new QuadTree<object>(2,
                            new BoundingRectangle(
                            new Vector2(0, 0), new Vector2(4, 4)));

        }

        [TestMethod()]
        public void ExpandTest()
        {
            Assert.AreEqual(ContainmentType.Contains,
                new BoundingBox(Vector3.Zero, Vector3.One).Contains(Vector3.One));

            BoundingRectangle bounds = new BoundingRectangle(
                new Vector2(0, 0), new Vector2(4, 4));

            QuadTree<object> tree = new QuadTree<object>(2, bounds);

            Assert.AreEqual<BoundingRectangle>(bounds, tree.Bounds);
            Assert.AreEqual<BoundingRectangle>(bounds, tree.Root.Bounds);
            Assert.AreEqual<int>(0, tree.Root.Depth);
            Assert.AreEqual<int>(0, tree.Count);

            tree.Add(null, (o) => 
            {
                return o.Contains(0.5f, 0.5f);
            });

            Assert.AreEqual<int>(2, tree.Depth);
            Assert.AreEqual<int>(1, tree.Count);

            QuadTreeNode<object> leaf = tree.FindFirst(TreeNodeType.Bottom, (o) => 
            {
                return o.Bounds.Contains(0.1f, 0.1f);
            });

            Assert.IsNotNull(leaf);
            Assert.AreEqual<int>(2, leaf.Depth);
        }
    }
}
