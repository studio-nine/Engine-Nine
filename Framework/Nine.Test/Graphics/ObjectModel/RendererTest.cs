#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
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
    public class RendererTest
    {
        public TestGame Game;
        public GraphicsDevice GraphicsDevice { get { return Game.GraphicsDevice; } }

        [TestInitialize()]
        public void InitializeGraphicsTest()
        {
            Game = new TestGame();
        }

        [TestCleanup()]
        public void CleanupGraphicsTest()
        {
            Game.Exit();
        }

        [TestMethod()]
        public void ApplyLightsTest()
        {
            /*
            Game.Paint += (gameTime) =>
            {
                Renderer renderer = new Renderer(GraphicsDevice);
                var lights = new Light[]
                {
                    new SpotLight(GraphicsDevice),
                    new PointLight(GraphicsDevice),
                    new PointLight(GraphicsDevice),
                    new SpotLight(GraphicsDevice),
                    new Nine.Graphics.ObjectModel.DirectionalLight(GraphicsDevice),
                    new Nine.Graphics.ObjectModel.DirectionalLight(GraphicsDevice),
                    new PointLight(GraphicsDevice),
                    new SpotLight(GraphicsDevice),
                };

                var unappliedLights = new List<Light>();
                renderer.ApplyLights(lights, new SpotLight(GraphicsDevice).Effect, l => unappliedLights.Add(l));
                Assert.AreEqual(unappliedLights.Count, unappliedLights.Distinct().Count());
                Assert.AreEqual(7, unappliedLights.Count);

                lights = new Light[]
                {
                    new SpotLight(GraphicsDevice),
                    new PointLight(GraphicsDevice),
                };
                unappliedLights = new List<Light>();
                renderer.ApplyLights(lights, new SpotLight(GraphicsDevice).Effect, l => unappliedLights.Add(l));
                Assert.AreEqual(unappliedLights.Count, unappliedLights.Distinct().Count());
                Assert.AreEqual(1, unappliedLights.Count);

                Game.Exit();
            };
            Game.Run();    
             */
        }
    }
}
