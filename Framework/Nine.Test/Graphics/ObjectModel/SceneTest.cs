#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.Test;

namespace Nine.Graphics.ObjectModel.Test
{
    [TestClass]
    public class SceneTest : GraphicsTest
    {
        [TestMethod()]
        public void AddQueryRemoveObjectTest()
        {
            Test(() =>
            {
                var spotLight = new SpotLight(GraphicsDevice);
                var pointLight = new PointLight(GraphicsDevice);
                var scene = new Scene(GraphicsDevice);

                scene.Add(spotLight);
                Assert.AreEqual(1, scene.Count);

                scene.Add(pointLight);
                Assert.AreEqual(2, scene.Count);

                var boundingBox = new BoundingBox(Vector3.One * -100, Vector3.One * 100);
                var sceneObjects = new List<object>();
                scene.FindAll(ref boundingBox, sceneObjects);

                Assert.AreEqual(2, sceneObjects.Count);
                Assert.AreNotEqual(sceneObjects[0].GetType(), sceneObjects[1].GetType());

                scene.Remove(spotLight);

                sceneObjects = new List<object>();
                scene.FindAll(ref boundingBox, sceneObjects);

                Assert.AreEqual(1, sceneObjects.Count);
                Assert.AreEqual(pointLight, sceneObjects[0]);
            }); 
        }

        [TestMethod()]
        public void AddQueryCombinedObjectTest()
        {
            Test(() =>
            {
                var spotLight = new SpotLight(GraphicsDevice);
                var pointLight = new PointLight(GraphicsDevice);

                var container = new DisplayObject();
                container.Children.Add(spotLight);
                container.Children.Add(pointLight);

                var scene = new Scene(GraphicsDevice);

                scene.Add(container);
                Assert.AreEqual(1, scene.Count);

                scene.Add(new DirectionalLight(GraphicsDevice));
                Assert.AreEqual(2, scene.Count);

                var boundingBox = new BoundingBox(Vector3.One * -100, Vector3.One * 100);
                var sceneObjects = new List<object>();
                scene.FindAll(ref boundingBox, sceneObjects);
                Assert.AreEqual(4, sceneObjects.Count);

                var pointLights = new List<PointLight>();
                scene.FindAll(ref boundingBox, pointLights);
                Assert.AreEqual(1, pointLights.Count);
                Assert.AreEqual(pointLight, pointLights[0]);
            });
        }

        [TestMethod()]
        public void AddQueryCombinedObjectUsingFindResultTest()
        {
            Test(() =>
            {
                var spotLight = new SpotLight(GraphicsDevice);
                var pointLight = new PointLight(GraphicsDevice);

                var container = new DisplayObject();
                container.Children.Add(spotLight);
                container.Children.Add(pointLight);

                var scene = new Scene(GraphicsDevice);
                scene.Add(container);

                var boundingBox = new BoundingBox(Vector3.One * -100, Vector3.One * 100);
                var findResults = new List<FindResult>();
                scene.FindAll(ref boundingBox, findResults);
                Assert.AreEqual(2, findResults.Count);
                Assert.IsTrue(findResults[0].Target == findResults[1].Target);
                Assert.IsTrue(findResults[0].OriginalTarget != findResults[1].OriginalTarget);
                Assert.AreEqual(container, findResults[0].Target);
                Assert.IsTrue(findResults[0].OriginalTarget == spotLight || findResults[1].OriginalTarget == spotLight);
                Assert.IsTrue(findResults[0].OriginalTarget == pointLight || findResults[1].OriginalTarget == pointLight);
            });
        }

        [TestMethod()]
        public void AddQueryNamedObjectTest()
        {
            Test(() =>
            {
                var spotLight = new SpotLight(GraphicsDevice) { Name = "A" };
                var pointLight = new PointLight(GraphicsDevice) { Name = "B" };

                var container = new DisplayObject() { Name = "A" };
                container.Children.Add(spotLight);
                container.Children.Add(pointLight);

                var scene = new Scene(GraphicsDevice);
                scene.Add(container);

                Assert.AreEqual(pointLight, scene.Find<PointLight>("B"));

                var allObjects = new List<object>();
                scene.FindAll("A", allObjects);
                Assert.AreEqual(2, allObjects.Count);

                var allSpotLights = new List<SpotLight>();
                scene.FindAll("A", allSpotLights);
                Assert.AreEqual(1, allSpotLights.Count);
                Assert.AreEqual(spotLight, allSpotLights[0]);
            });
        }
    }
}
