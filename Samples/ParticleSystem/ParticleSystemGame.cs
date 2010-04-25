#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Isles;
using Isles.Graphics;
using Isles.Graphics.Cameras;
using Isles.Graphics.ParticleEffects;
#endregion


namespace ParticleSystem
{
    /// <summary>
    /// Sample game showing how to display skinned character animation.
    /// </summary>
    public class ParticleSystemGame : Microsoft.Xna.Framework.Game
    {
        ModelViewerCamera camera;

        ParticleEffect snow;
        ParticleEffect fire;
        ParticleBatch particleBatch;


        public ParticleSystemGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a model viewer camera to help us visualize the scene
            camera = new ModelViewerCamera(this);

            snow = new ParticleEffect(1024);
            snow.Texture = Content.Load<Texture2D>("flake");
            snow.Duration = TimeSpan.FromSeconds(4);
            snow.Gravity = Vector3.Zero;
            snow.Emitter.Emission = 256;
            snow.MaxStartSize = snow.MaxEndSize = 0.8f;
            snow.MinStartSize = snow.MinEndSize = 0.4f;
            snow.MaxHorizontalVelocity = snow.MinHorizontalVelocity = 0;
            snow.MaxVerticalVelocity = snow.MinVerticalVelocity = -40.0f;
            
            snow.SpacialEmitter = new BoxEmitter() 
            {
                Box = new BoundingBox(new Vector3(-10, -10, 30), new Vector3(10, 10, 30))
            };


            fire = new ParticleEffect(1024);
            snow.MaxStartSize = snow.MaxEndSize =
            snow.MinStartSize = snow.MinEndSize = 0.4f;
            fire.Texture = Content.Load<Texture2D>("fire");
            fire.Emitter.Emission = 256;
            fire.Gravity = Vector3.Zero;
            fire.MaxHorizontalVelocity = 1.5f;
            fire.MinHorizontalVelocity = 0f;
            fire.MaxVerticalVelocity = 1.5f;
            fire.MinVerticalVelocity = 0;

            fire.SpacialEmitter = new RingEmitter()
            {
                Radius = 2.5f
            };


            particleBatch = new ParticleBatch(GraphicsDevice, 32, 32, 32);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            snow.Update(gameTime);
            fire.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            particleBatch.Begin(SpriteBlendMode.Additive, camera.View, camera.Projection);
            particleBatch.Draw(snow, gameTime);
            particleBatch.Draw(fire, gameTime);
            particleBatch.End();

            base.Draw(gameTime);
        }
    }
}
