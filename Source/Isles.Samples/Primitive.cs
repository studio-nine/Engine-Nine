#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Isles.Components;
using Isles.Graphics;
using Isles.Graphics.Cameras;
using Isles.Graphics.Primitives;
using Isles.Graphics.Landscape;
using Isles.Graphics.Models;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class PrimitiveGame : BasicModelViewerGame
    {
        int index = 0;
        List<Primitive> primitives = new List<Primitive>();
        GraphicsEffect effect;

        public PrimitiveGame()
        {

        }

        protected override void LoadContent()
        {
            primitives.Add(new Isles.Graphics.Primitives.Plane(GraphicsDevice));
            primitives.Add(new Dome(GraphicsDevice));
            primitives.Add(new Sphere(GraphicsDevice));
            primitives.Add(new Cube(GraphicsDevice));
            primitives.Add(new Teapot(GraphicsDevice));
            primitives.Add(new Cylinder(GraphicsDevice));
            primitives.Add(new Torus(GraphicsDevice));
            primitives.Add(new Centrum(GraphicsDevice));

            Input.MouseDown += new EventHandler<MouseEventArgs>(Input_LeftButtonDown);

            effect = new Isles.Graphics.Effects.FloatTextureEffect(Content.Load<Texture2D>("Terrain/sky"));
            
            base.LoadContent();
        }

        void Input_LeftButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                index = (index + 1) % primitives.Count;

                Window.Title = primitives[index].GetType().Name;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            primitives[index].Draw(Matrix.CreateScale(5), Camera.View, Camera.Projection, Color.Yellow);
            primitives[index].Draw(effect, gameTime, Matrix.CreateScale(5), Camera.View, Camera.Projection);
        }
    
        [SampleMethod]
        public static void Test()
        {
            using (PrimitiveGame game = new PrimitiveGame())
            {
                game.Run();
            }
        }
    }
}
