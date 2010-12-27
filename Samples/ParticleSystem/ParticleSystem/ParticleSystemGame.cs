#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
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
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Graphics;
using Nine.Graphics.Effects;
using Nine.Graphics.ParticleEffects;
using Nine.Animations;
#endregion

namespace ParticleSystem
{
    /// <summary>
    /// Demonstrate how to use ParticleBatch to draw point sprites, textured lines and particle effects.
    /// </summary>
    public class ParticleSystemGame : Microsoft.Xna.Framework.Game
    {
        TopDownEditorCamera camera;
        ParticleBatch particles;
        PrimitiveBatch primitiveBatch;
        
        Texture2D lightning;

        ParticleEffect snow;

        public ParticleSystemGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
            //Components.Add(new FrameRate(this, "Consolas"));
            Components.Add(new InputComponent(Window.Handle));
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            camera = new TopDownEditorCamera(GraphicsDevice);

            particles = new ParticleBatch(GraphicsDevice, 1024);
            primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            
            snow = new ParticleEffect(600);
            snow.Texture = Content.Load<Texture2D>("flake");
            snow.Duration = TimeSpan.FromSeconds(1.5f);
            snow.Gravity = Vector3.Zero;
            snow.Emitter.Emission = 400;
            snow.StartSize = 0.8f;
            snow.EndSize = 0.4f;
            snow.EndVelocity = 1.0f;
            snow.HorizontalVelocity = 0;
            snow.VerticalVelocity = -40.0f;
            
            snow.SpatialEmitter = new BoxEmitter()
            {
                Box = new BoundingBox(new Vector3(-30, -30, 30), new Vector3(30, 30, 30))
            };
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // Prepare render states used to draw particles.
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            
            GraphicsDevice.SetVertexBuffer(null);

            // Update effects
            snow.Update(gameTime);


            particles.Begin(camera.View, camera.Projection);
            particles.Draw(snow, gameTime);
            particles.End();

            primitiveBatch.Begin(camera.View, camera.Projection);
            primitiveBatch.DrawBox(snow.BoundingBox, null, Color.Azure);
            primitiveBatch.End();

            base.Draw(gameTime);
        }
    }
}
