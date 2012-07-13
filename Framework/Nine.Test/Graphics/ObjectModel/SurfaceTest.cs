﻿#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nine.Graphics.ObjectModel.Test
{
    [TestClass()]
    public class SurfaceTest : GraphicsTest
    {
        [TestMethod()]
        public void HeightmapTest()
        {
            Heightmap heightmap = new Heightmap(1.0f, 2, 2);
            
            Assert.AreEqual<Vector3>(new Vector3(2, 2, 0), heightmap.Size);

            Assert.AreEqual<BoundingBox>(
                new BoundingBox(Vector3.Zero, new Vector3(2, 2, 0)),
                heightmap.BoundingBox);
        }

        [TestMethod()]
        public void DrawableSurfaceContructorTest()
        {
            Surface surface = new Surface(
                GraphicsDevice, new Heightmap(1, 2, 2), 2);

            Assert.AreEqual<int>(1, surface.PatchCountX);
            Assert.AreEqual<int>(1, surface.PatchCountY);
            Assert.AreEqual<int>(1, surface.Patches.Count);
            Assert.AreEqual<int>(2, surface.PatchSegmentCount);
            Assert.AreEqual<Vector3>(new Vector3(2, 2, 0), surface.Size);
            Assert.AreEqual<BoundingBox>(surface.BoundingBox, surface.Patches[0].BoundingBox);

            surface.Position = new Vector3(1, 1, 0);

            Assert.AreEqual<BoundingBox>(
                new BoundingBox(new Vector3(1, 1, 0), new Vector3(3, 3, 0)),
                surface.BoundingBox);

            Assert.AreEqual<BoundingBox>(surface.BoundingBox, surface.Patches[0].BoundingBox);
        }
        /*
        [TestMethod()]
        public void DrawableSurfaceTriangleTest()
        {
            Game.Paint += (gameTime) =>
            {
                DrawableSurface surface = new DrawableSurface(
                    GraphicsDevice, new Heightmap(1, 2, 2), 2);

                Assert.AreEqual<int>(8, surface.Patches[0].PrimitiveCount);

                surface.Triangles[0, 0].Visible = false;
                surface.Triangles[1.95f, 1.95f].Visible = false;
                surface.Invalidate();

                Assert.AreEqual<int>(6, surface.Patches[0].PrimitiveCount);

                foreach (var triangle in surface.Triangles)
                {
                    triangle.Visible = false;
                }
                surface.Invalidate();
                
                Assert.AreEqual<int>(0, surface.Patches[0].PrimitiveCount);

                foreach (var triangle in surface.Triangles)
                {
                    triangle.Visible = true;
                }
                surface.Invalidate();

                Assert.AreEqual<int>(8, surface.Patches[0].PrimitiveCount);

                Game.Exit();
            };
            Game.Run();
        }
         */
    }
}
