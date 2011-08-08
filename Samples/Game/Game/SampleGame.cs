#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Graphics;
using Nine.Graphics.Primitives;
using Nine.Graphics.ObjectModel;
using Nine.Components;
using Microsoft.Xna.Framework.Input;
using Nine.Graphics.Effects.Deferred;
#endregion

namespace Game
{
    [Category("Graphics")]
    [DisplayName("Sample Game")]
    [Description("This sample demenstrates the use of Nine.Graphics")]
    public class SampleGame : Microsoft.Xna.Framework.Game
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
        private const int BackBufferWidth = 1024;
        private const int BackBufferHeight = 768;
#endif

        World world;
        Renderer renderer;
        PointLight pointLight1;
        PointLight pointLight2;
        PointLight pointLight3;
        SpotLight spotLight;


        public SampleGame()
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

            spotLight = new SpotLight(GraphicsDevice);

            world = new World();
            world.WorldObjects.Add(new WorldObject() { Template = "WorldView" });
            world.WorldObjects.Add(spotLight);
            //world.WorldObjects.Add(new PointLight(GraphicsDevice));
            world.WorldObjects.Add(new Nine.Graphics.ObjectModel.AmbientLight(GraphicsDevice) { AmbientLightColor = Vector3.One * 0.1f });
            world.WorldObjects.Add(new Nine.Graphics.ObjectModel.DirectionalLight(GraphicsDevice) { Transform = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, -1, -1), Vector3.UnitZ), DiffuseColor = Vector3.One * 0.4f });

            renderer = world.CreateGraphics(GraphicsDevice);
            //renderer.Camera = new ModelViewerCamera(GraphicsDevice);

            //renderer.Drawables.Remove(renderer.Drawables.First(d => d is Terrain));
            
            renderer.Lights.Add(pointLight1 = new PointLight(GraphicsDevice));
            renderer.Lights.Add(pointLight2 = new PointLight(GraphicsDevice));
            renderer.Lights.Add(pointLight3 = new PointLight(GraphicsDevice));

            pointLight1.Range = 20;
            pointLight2.Range = 20;
            pointLight3.Range = 20;
            
            spotLight.Range = 50;
            spotLight.Attenuation = 100;
            
            base.LoadContent();
        }

        /// <summary>
        /// This is called when the game should update itself.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            world.Update(gameTime.ElapsedGameTime);

            //light.Attenuation = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds) + 1) * 100f;
            //light.Range = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds) + 1) * 100;
            pointLight1.Transform = Matrix.CreateTranslation(
                                     (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds) * 50 + 50,
                                     (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 50 + 50, 10);

            pointLight2.Transform = Matrix.CreateTranslation(
                                     (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2f) * 50 + 50,
                                     (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds * 2f) * 50 + 50, 10);

            pointLight3.Transform = Matrix.CreateTranslation(
                                     (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds + 10) * 50 + 50,
                                     (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds + 10) * 50 + 50, 10);

            spotLight.Transform = Matrix.CreateRotationX(MathHelper.PiOver2) *
                                  Matrix.CreateRotationZ(-(float)gameTime.TotalGameTime.TotalSeconds) *
                                  Matrix.CreateTranslation(50, 50, 10);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            var keyboardState = Keyboard.GetState();
            {
                //renderer.Settings.PreferHighDynamicRangeLighting = keyboardState.IsKeyDown(Keys.H);
                renderer.Settings.PreferDeferredLighting = keyboardState.IsKeyDown(Keys.D);
                renderer.Settings.Debug.ShowWireframe = keyboardState.IsKeyDown(Keys.W);
                renderer.Settings.Debug.ShowBoundingBox = keyboardState.IsKeyDown(Keys.B);
                renderer.Settings.Debug.ShowLightFrustum = keyboardState.IsKeyDown(Keys.L);
                renderer.Settings.Debug.ShowSceneManager = keyboardState.IsKeyDown(Keys.S);

                renderer.Settings.Debug.ShowDepthBuffer = keyboardState.IsKeyDown(Keys.Space);
                renderer.Settings.Debug.ShowNormalBuffer = keyboardState.IsKeyDown(Keys.N);
            }

            world.Draw(gameTime.ElapsedGameTime);

            base.Draw(gameTime);
        }
    }
}