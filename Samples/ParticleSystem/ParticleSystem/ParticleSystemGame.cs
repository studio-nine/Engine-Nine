#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Graphics.ParticleEffects;
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
        private const int BackBufferWidth = 1280;
        private const int BackBufferHeight = 800;
#endif

        Scene scene;
        SpriteBatch spriteBatch;

        public ParticleSystemGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);

            Content.RootDirectory = "Content";

            graphics.IsFullScreen = true;
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));
            Components.Add(new InputComponent(Window.Handle));

            spriteBatch = new SpriteBatch(GraphicsDevice);

            scene = new Scene(GraphicsDevice);

            scene.Add(CreateParticleEffect());
        }

        private ParticleEffect CreateParticleEffect()
        {
            ParticleEffect snow = new ParticleEffect(GraphicsDevice);
            snow.Texture = Content.Load<Texture2D>("Textures/flake");
            snow.Stretch = 10;
            
            ParticleEffect.IsAsync = true;

            snow.Emitter = new PointEmitter
            {
                Duration = 4,
                Emission = 10000,
                Size = 0.2f,
                Color = Color.White,
                Speed = 8f,
            };

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

        protected override void Update(GameTime gameTime)
        {
            scene.Update(gameTime.ElapsedGameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            scene.Draw(gameTime.ElapsedGameTime);

            base.Draw(gameTime);
        }
    }
}
