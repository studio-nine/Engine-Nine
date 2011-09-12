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
using System.ComponentModel;
using Nine.Components;
#endregion

namespace ParticleSystem
{
    [Category("Graphics, Animation")]
    [DisplayName("Particle System")]
    [Description("This sample demenstrates how to draw particle effects.")]
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
        ParticleEffect fireworks;
        ParticleEffect galaxy;
        ParticleBatch particlePatch;
        PrimitiveBatch primitiveBatch;
        SpriteBatch spriteBatch;

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
            Window.AllowUserResizing = true;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));
            Components.Add(new InputComponent(Window.Handle));

            camera = new ModelViewerCamera(GraphicsDevice);

            particlePatch = new ParticleBatch(GraphicsDevice);
            primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // You can either programatically create the effect or author the effect
            // using xml file and load it through the content pipeline.
            //particleEffect = CreateParticleEffect();
            galaxy = Content.Load<ParticleEffect>("Galaxy");

            // Trigger another wave of particles for 5 seconds
            //particleEffect.Trigger(Vector3.One * 2, TimeSpan.FromSeconds(5));
            //particleEffect.Trigger(Vector3.Zero, 10);

            // Advanced particle effects can be composed through ChildEffects
            // and EndingEffects property.
            fireworks = Content.Load<ParticleEffect>("Fireworks");
            fireworks.Trigger();
            //fireworks.Trigger(-Vector3.One * 2, 5);
        }

        private ParticleEffect CreateParticleEffect()
        {
            ParticleEffect snow = new ParticleEffect(1024);
            snow.Texture = Content.Load<Texture2D>("flake");
            snow.Duration = 4;
            snow.Emission = 100;
            snow.Size = 0.2f;
            snow.Color = Color.White;
            snow.Speed = 8f;
            snow.Stretch = 10;

            //snow.Controllers.Add(new SizeController() { EndSize = 0.4f });
            //snow.Controllers.Add(new ColorController() { EndColor = Color.White });
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

            return snow;
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
            fireworks.Update(gameTime.ElapsedGameTime);
            galaxy.Update(gameTime.ElapsedGameTime);

            // Draw particle system using ParticleBatch
            particlePatch.Begin(camera.View, camera.Projection);
            particlePatch.Draw(fireworks);
            particlePatch.Draw(galaxy);
            particlePatch.End();

            // Use SpriteBatch to draw 2D particles
            // Scale up the effect since it's too small
            spriteBatch.Begin(0, null, null, null, null, null, Matrix.CreateScale(20, 20, 1) * Matrix.CreateTranslation(100, 100, 0));
            spriteBatch.DrawParticleEffect(galaxy);
            spriteBatch.End();


            // Draw particle system bounds
            primitiveBatch.Begin(camera.View, camera.Projection);
            primitiveBatch.DrawBox(galaxy.BoundingBox, null, Color.Azure);
            primitiveBatch.End();

            base.Draw(gameTime);
        }
    }
}
