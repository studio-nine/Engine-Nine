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

        PointSpriteBatch pointSprite;
        Texture2D texture;
        Texture2D texture1;

        public ParticleSystemGame()
        {

        }


        protected override void LoadContent()
        {
            pointSprite = new PointSpriteBatch(GraphicsDevice, 64);
            texture = Content.Load<Texture2D>("Textures/fire");
            texture1 = Content.Load<Texture2D>("Textures/flake"); 

            ParticleSystem = new ParticleSystem(1280);
            ParticleSystem.Emitter = new BoxEmitter();
            //ParticleSystem.ParticleEffect = new BasicParticleEffect();
            //ParticleSystem.ParticleEffect.Texture = Content.Load<Texture2D>("Textures/flake");

            SpinnerTransition<Vector3> transition = new SpinnerTransition<Vector3>();

            transition.Radius = 10;

            circle = transition as ITransition<Vector3>;

            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            //(ParticleSystem.ParticleEmitter as PointEmitter).Position = circle.Update(gameTime);
            ParticleSystem.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //ParticleSystem.Draw(GraphicsDevice, gameTime, Camera.View, Camera.Projection);

            //GraphicsDevice.Clear(Color.Green);

            pointSprite.Begin(Camera.View, Camera.Projection);
            pointSprite.Draw(texture, Vector3.Zero, 5, Color.White);
            pointSprite.Draw(texture1, new Vector3(10, 0, 0), 5, Color.White);
            pointSprite.End();
        }


        [SampleMethod(IsStartup = false)]
        public static void Test()
        {
            using (ParticleSystemGame game = new ParticleSystemGame())
            {
                game.Run();
            }
        }
    }
}
