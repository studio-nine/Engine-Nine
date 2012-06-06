#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace Nine.Test
{
    class TestBoundable : ISpatialQueryable
    {
        public bool NoEventHandlerAttached { get { return BoundingBoxChanged == null; } }

        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
            set { boundingBox = value; if (BoundingBoxChanged != null) BoundingBoxChanged(this, EventArgs.Empty); }
        }

        public BoundingBox boundingBox;

        public event EventHandler<EventArgs> BoundingBoxChanged;

        static Random random = new Random();
        public static TestBoundable CreateRandom(BoundingBox bounds, float size)
        {
            BoundingBox box;
            do
            {
                var point = new Vector3(MathHelper.Lerp(bounds.Min.X, bounds.Max.X, (float)random.NextDouble()),
                                        MathHelper.Lerp(bounds.Min.Y, bounds.Max.Y, (float)random.NextDouble()),
                                        MathHelper.Lerp(bounds.Min.Z, bounds.Max.Z, (float)random.NextDouble()));

                box = BoundingBox.CreateFromPoints(new Vector3[] { point, point + Vector3.One * size });

            } while (bounds.Contains(box) != ContainmentType.Contains);

            return new TestBoundable() { BoundingBox = box };
        }

        public object SpatialData { get; set; }
    }

    [TestClass()]
    public class OctreeObjectManagerTest
    {
        [TestMethod()]
        public void AddUpdateRemoveTest()
        {
            OctreeSceneManager scene = new OctreeSceneManager();
            Assert.AreEqual(1, scene.Tree.Count());
            var boundable = new TestBoundable() { BoundingBox = new BoundingBox(Vector3.One * 0.1f, Vector3.One) };
            scene.Add(boundable);

            //dynamic data1 = boundable.SpatialData;
            //var node1 = data1.Node;

            Assert.AreEqual(1, scene.Count);
            Assert.AreEqual(1, scene.Count());
            boundable.BoundingBox = new BoundingBox(-Vector3.One, -Vector3.One * 0.1f);
            
            //dynamic data2 = boundable.SpatialData;
            //var node2 = data2.Node;

            //Assert.AreNotEqual(node1, node2);

            Assert.AreEqual(1, scene.Count);
            Assert.AreEqual(1, scene.Count());

            scene.Remove(boundable);

            Assert.AreEqual(0, scene.Count);
            Assert.AreEqual(1, scene.Tree.Count());
        }

        [TestMethod()]
        public void AddUpdateToNewBoundsRemoveTest()
        {
            OctreeSceneManager scene = new OctreeSceneManager();

            var boundable = new TestBoundable() { BoundingBox = new BoundingBox(Vector3.One * 0.1f, Vector3.One) };
            scene.Add(boundable);
            
            Assert.AreEqual(1, scene.Count);
            Assert.AreEqual(1, scene.Count());
            boundable.BoundingBox = new BoundingBox(-Vector3.One * 10000, Vector3.One * 10000);
            
            Assert.AreEqual(1, scene.Count);
            Assert.AreEqual(1, scene.Count());

            scene.Remove(boundable);

            Assert.AreEqual(0, scene.Count);
            Assert.AreEqual(1, scene.Tree.Count());
        }
        
        [TestMethod()]
        public void AddQueryUpdateRemoveTest()
        {
            // 1000 x 1000 x 1000
            var scene = new OctreeSceneManager();
            var bounds = new BoundingBox(Vector3.One * -500, Vector3.One * 1000);
            var objects = Enumerable.Range(0, 1000).Select(i => TestBoundable.CreateRandom(bounds, 50f)).ToArray();
            var updatedObjects = Enumerable.Range(0, 1000).Select(i => TestBoundable.CreateRandom(bounds, 50f)).ToArray();
            var queries = Enumerable.Range(0, 1000).Select(i => TestBoundable.CreateRandom(bounds, 200)).ToArray();

            // Add
            GC.Collect();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            foreach (var o in objects)
            {
                scene.Add(o);
            }

            watch.Stop();
            Trace.WriteLine("Tree Nodes: " + scene.Tree.Count());
            Trace.WriteLine("Max num objects added per frame (60 FPS): " + objects.Length / watch.Elapsed.TotalSeconds / 60.0);

            Assert.AreEqual(objects.Length, scene.Count);
            Assert.AreEqual(objects.Length, scene.Count());

            List<ISpatialQueryable> queryResult = new List<ISpatialQueryable>();

            // Query
            watch.Restart();
            
            foreach (var q in queries)
            {
                scene.FindAll(ref q.boundingBox, queryResult);
            }

            watch.Stop();
            Trace.WriteLine("Tree Nodes: " + scene.Tree.Count());
            Trace.WriteLine(string.Format("Max queries per frame (60 FPS): {0} on {1} objects",
                                        queries.Length / watch.Elapsed.TotalSeconds / 60.0, objects.Length));

            foreach (var g in scene.Tree.GroupBy(n => n.Depth))
            {
                Trace.WriteLine(string.Format("Objects with depth {0}: {1}", g.Key, g.Sum(n => n.Value != null ? n.Value.Count : 0)));
                Trace.WriteLine(string.Format("Average objects per node with depth {0}: {1}", g.Key, g.Average(n => n.Value != null ? n.Value.Count : 0)));
            }

            // Update
            watch.Restart();

            for (int i = 0; i < updatedObjects.Length; i++)
            {
                objects[i].BoundingBox = updatedObjects[i].BoundingBox;
            }

            watch.Stop();
            Trace.WriteLine("Tree Nodes: " + scene.Tree.Count());
            Trace.WriteLine("Max num objects updated per frame (60 FPS): " + objects.Length / watch.Elapsed.TotalSeconds / 60.0);

            Assert.AreEqual(objects.Length, scene.Count);
            Assert.AreEqual(objects.Length, scene.Count());

            //var all = scene.FindAll(scene.Bounds).ToArray();
            var all = objects;

            // Remove
            watch.Restart();

            for (int i = 0; i < all.Length; i++)
            {
                scene.Remove(all[i]);
            }

            watch.Stop();
            Trace.WriteLine("Tree Nodes: " + scene.Tree.Count());
            Trace.WriteLine("Max num objects removed per frame (60 FPS): " + objects.Length / watch.Elapsed.TotalSeconds / 60.0);

            Assert.AreEqual(0, scene.Count);
            Assert.IsTrue(objects[0].NoEventHandlerAttached);
            Assert.AreEqual(1, scene.Tree.Count());
        }
    }
}
