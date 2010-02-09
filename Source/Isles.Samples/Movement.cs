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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Isles.Components;
using Isles.Graphics;
using Isles.Graphics.Cameras;
using Isles.Graphics.Primitives;
using Isles.Graphics.Models;
using Isles.Graphics.Landscape;
using Isles.Graphics.Effects;
using Isles.Navigation;
using Isles.Navigation.Flocking;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class MovementSample : SampleGame
    {
        TopDownEditorCamera camera;
        Model tank;
        ModelBatch batch = new ModelBatch();
        
        IMovable movable;


        protected override void LoadContent()
        {
            camera = new TopDownEditorCamera(this);
            camera.Radius = camera.MaxRadius;

            tank = Content.Load<Model>("Models/Ship");

            
            FlockingMovement flocking = new FlockingMovement();

            SeekBehavior behavior = new SeekBehavior();

            behavior.Target = new Vector3(100, 100, 100);

            flocking.Behaviors.Add(behavior);
            flocking.Movable = new CharactorMovement();

            movable = flocking;
        }

        protected override void Update(GameTime gameTime)
        {
            movable.Update(gameTime);
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);
            GraphicsDevice.RenderState.AlphaBlendEnable = false;

            batch.Begin();
            batch.Draw(tank, Matrix.CreateScale(0.01f) * movable.Transform, camera.View, camera.Projection);
            batch.End();
        }

        [SampleMethod(Startup=true)]
        public static void Test()
        {
            using (MovementSample game = new MovementSample())
                game.Run();
        }
    }
}
