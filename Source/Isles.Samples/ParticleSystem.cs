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
        public ParticleSystem ParticleSystem { get; set; }

        ITransition<Vector3> circle;
        TransitionManager transitions = new TransitionManager();

        PointSpriteBatch pointSprite;
        LineSpriteBatch lineSprite;

        Texture2D texture;
        Texture2D texture1;

        public Vector2 Offset { get; set; }

        public ParticleSystemGame()
        {

        }


        protected override void LoadContent()
        {
            pointSprite = new PointSpriteBatch(GraphicsDevice, 64);
            lineSprite = new LineSpriteBatch(GraphicsDevice, 64);

            texture = Content.Load<Texture2D>("Textures/fire");
            texture1 = Content.Load<Texture2D>("Textures/flake"); 

            ParticleSystem = new ParticleSystem(1280);
            ParticleSystem.Emitter = new BoxEmitter();
            //ParticleSystem.ParticleEffect = new BasicParticleEffect();
            //ParticleSystem.ParticleEffect.Texture = Content.Load<Texture2D>("Textures/flake");

            SpinnerTransition<Vector3> transition = new SpinnerTransition<Vector3>();

            transition.Radius = 10;

            circle = transition as ITransition<Vector3>;


            transitions.Start(
                new LinearTransition<Vector2>(
                    Vector2.Zero, Vector2.UnitY, TimeSpan.FromSeconds(1), TransitionEffect.Loop), this, "Offset");

            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            //(ParticleSystem.ParticleEmitter as PointEmitter).Position = circle.Update(gameTime);
            ParticleSystem.Update(gameTime);

            transitions.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ShowAxis = false;
            ShowGrid = false;

            base.Draw(gameTime);

            //ParticleSystem.Draw(GraphicsDevice, gameTime, Camera.View, Camera.Projection);

            //GraphicsDevice.Clear(Color.Green);

            /*
            pointSprite.Begin(Camera.View, Camera.Projection);
            pointSprite.Draw(texture, Vector3.Zero, 5, 0, Color.White);
            pointSprite.Draw(texture1, new Vector3(10, 0, 0), 5, 0, Color.White);
            pointSprite.End();
            */

            lineSprite.BlendMode = SpriteBlendMode.AlphaBlend;

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


            lineSprite.Begin(Camera.View, Camera.Projection);
            lineSprite.Draw(texture1, vv.ToArray(), 1, Vector2.One * 0.5f, Offset, Color.White);
            lineSprite.Draw(texture, ring.ToArray(), 1, Vector2.One, Offset, Color.White);
            lineSprite.End();
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
