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
using Isles.Transitions;
using Isles.Graphics;
using Isles.Graphics.ParticleEffects;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class ParticleSystemGame : BasicModelViewerGame
    {
        public ParticleEffect ParticleSystem { get; set; }

        ITransition<Vector3> circle;
        TransitionManager transitions = new TransitionManager();

        ParticleBatch batch;

        Texture2D texture;
        Texture2D texture1;

        public Vector2 Offset { get; set; }


        public ParticleSystemGame()
        {

        }


        protected override void LoadContent()
        {
            batch = new ParticleBatch(GraphicsDevice, 64, 64, 8);

            texture = Content.Load<Texture2D>("Textures/fire");
            texture1 = Content.Load<Texture2D>("Textures/flake"); 

            ParticleSystem = new ParticleEffect(512);
            ParticleSystem.Texture = Content.Load<Texture2D>("Textures/flake");

            SpinnerTransition<Vector3> transition = new SpinnerTransition<Vector3>();

            transition.Radius = 10;

            circle = transition as ITransition<Vector3>;


            //transitions.Start(
            //    new LinearTransition<Vector2>(
            //        Vector2.Zero, Vector2.UnitY, TimeSpan.FromSeconds(1), TransitionEffect.Loop), this, "Offset");

            transitions.Start<Vector2>(
                new LinearTransition<Vector2>(
                    Vector2.Zero, Vector2.UnitY, TimeSpan.FromSeconds(1), TransitionEffect.Loop),
                        x => Offset = (Vector2)x);

            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            circle.Update(gameTime);

            (ParticleSystem.SpacialEmitter as PointEmitter).Position = circle.Value;

            ParticleSystem.Update(gameTime);

            transitions.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //ShowAxis = false;
            ShowGrid = false;

            base.Draw(gameTime);


            List<Vector3> vv = new List<Vector3>();

            vv.Add(Vector3.Zero);
            vv.Add(Vector3.UnitY);
            vv.Add(Vector3.UnitX * 4);


            int n = 32;
            List<Vector3> ring = new List<Vector3>();

            for (int i = 0; i <= n; i++)
            {
                Vector3 v;

                v.X = (float)(5 * Math.Cos(i * MathHelper.Pi * 2 / n));
                v.Y = (float)(5 * Math.Sin(i * MathHelper.Pi * 2 / n));
                v.Z = 0;

                ring.Add(v);
            }


            batch.Begin(SpriteBlendMode.Additive, Camera.View, Camera.Projection);
            batch.Draw(texture, Vector3.Zero, 5, 0, Color.White);
            batch.Draw(texture1, new Vector3(10, 0, 0), 5, 0, Color.White);
            batch.Draw(texture1, vv.ToArray(), 1, Vector2.One * 0.5f, Offset, Color.White);
            batch.Draw(texture, ring.ToArray(), 1, Vector2.One, Offset, Color.White);
            batch.Draw(ParticleSystem, gameTime);
            batch.End();
        }


        [SampleMethod(IsStartup = true)]
        public static void Test()
        {
            using (ParticleSystemGame game = new ParticleSystemGame())
            {
                game.Run();
            }
        }
    }
}
