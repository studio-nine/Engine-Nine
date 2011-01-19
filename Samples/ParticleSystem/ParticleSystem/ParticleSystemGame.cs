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
using Nine.Graphics.ParticleEffects;
using Nine.Animations;
#endregion

namespace ParticleSystem
{
    /// <summary>
    /// Demonstrates how to draw particle effects.
    /// </summary>
    public class ParticleSystemGame : Microsoft.Xna.Framework.Game
    {
#if WINDOWS_PHONE
        private const int TargetFrameRate = 30;
        private const int BackBufferWidth = 800;
        private const int BackBufferHeight = 480;
#elif XBOX
        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 1280;
        private const int BackBufferHeight = 720;
#else
        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 900;
        private const int BackBufferHeight = 600;
#endif

        ModelViewerCamera camera;
        ParticleEffect snow;
        ParticleBatch particlePatch;
        PrimitiveBatch primitiveBatch;

        public ParticleSystemGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Components.Add(new FrameRate(this, "Consolas"));
            Components.Add(new InputComponent(Window.Handle));
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            camera = new ModelViewerCamera(GraphicsDevice);

            particlePatch = new ParticleBatch(GraphicsDevice);
            primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            
            snow = new ParticleEffect(5000);
            snow.Texture = Content.Load<Texture2D>("flake");
            snow.Duration = 4;
            snow.Emission = 100;
            snow.Size = 0.2f;
            snow.Color = Color.White;
            snow.Speed = 8f;
            snow.Stretch = 10;
            
            //snow.Controllers.Add(new SizeController() { EndSize = 0.4f });
            snow.Controllers.Add(new ColorController() { EndColor = Color.White });
            snow.Controllers.Add(new FadeController());
            //snow.Controllers.Add(new AbsorbController() { Force = 40 });
            //snow.Controllers.Add(new SpeedController() { EndSpeed = 0 });
            //snow.Controllers.Add(new RotationController() { EndRotation = MathHelper.Pi });
            //snow.Controllers.Add(new ForceController() { Force = Vector3.UnitZ * 20 });
            //snow.Controllers.Add(new TangentForceController() { Force = 0.5f });

            //snow.Emitter = new LineEmitter() { LineList = new[] { Vector3.Zero, Vector3.One, Vector3.UnitX, Vector3.UnitY, } };
            //snow.Emitter = new CylinderEmitter() { Radius = 0, Height = 0, Shell = true, Radiate = true };
            //snow.Emitter = new SphereEmitter() { Radius = 10, Shell = true, Radiate = true };
            //snow.Emitter = new PointEmitter() { Spread = MathHelper.PiOver4 };
            //snow.Emitter = new BoxEmitter() { Box = new BoundingBox(new Vector3(-30, -30, 30), new Vector3(30, 30, 30)) };
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
            

            // Update effects
            snow.Update(gameTime);

            particlePatch.Begin(camera.View, camera.Projection);
            //particlePatch.DrawBillboard(snow);
            particlePatch.DrawConstrainedBillboard(snow);
            //particlePatch.DrawConstrainedBillboard(snow, Vector3.UnitX);
            particlePatch.End();

            primitiveBatch.Begin(camera.View, camera.Projection);
            primitiveBatch.DrawBox(snow.BoundingBox, null, Color.Azure);
            primitiveBatch.End();

            base.Draw(gameTime);
        }
    }
}
