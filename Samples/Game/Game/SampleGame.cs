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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Graphics;
using Nine.Graphics.Primitives;
using Nine.Graphics.ObjectModel;
using Nine.Components;
using DirectionalLight = Nine.Graphics.ObjectModel.DirectionalLight;
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

        Scene scene;
        PointLight pointLight1;
        PointLight pointLight2;
        PointLight pointLight3;
        SpotLight spotLight;
        DirectionalLight directionalLight;


        public SampleGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = true;
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
            //Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")) { Position = new Vector2(100, 100), Scale = 5, Color = Color.Black });
            Components.Add(new InputComponent(Window.Handle));
            

            scene = new Scene(GraphicsDevice);
            scene.Settings.DefaultFont = Content.Load<SpriteFont>("Consolas");
            scene.Add(Content.Load<DisplayObject>("Scene"));

            /*
            scene.Add(directionalLight = new DirectionalLight(GraphicsDevice) { Transform = Matrix.CreateWorld(Vector3.Zero, new Vector3(-1, -1, -1), Vector3.UnitZ), DiffuseColor = Vector3.One * 1 });

            scene.Add(pointLight1 = new PointLight(GraphicsDevice));
            scene.Add(pointLight2 = new PointLight(GraphicsDevice));
            scene.Add(pointLight3 = new PointLight(GraphicsDevice));

            pointLight1.Range = 4;
            pointLight2.Range = 4;
            pointLight3.Range = 4;

            scene.Add(spotLight = new SpotLight(GraphicsDevice));
            //scene.Camera = new ModelViewerCamera(GraphicsDevice);
            

            //pointLight1.Enabled = pointLight2.Enabled = pointLight3.Enabled = false;

            //directionalLight.Transform = Matrix.CreateWorld(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);
            directionalLight.CastShadow = true;
            //directionalLight.Enabled = false;
            
            spotLight.Range = 50;
            //spotLight.Attenuation = 100;
            //spotLight.Enabled = false;
            //spotLight.CastShadow = true;
            scene.Settings.LightingEnabled = false;
            */
            base.LoadContent(); 
        }

        /// <summary>
        /// This is called when the game should update itself.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back))
                Exit();

            double totalSeconds = gameTime.TotalGameTime.TotalSeconds;

            //scene.OfType<DrawableModel>().First().Alpha = (float)Math.Sin(totalSeconds * 4) * 0.5f + 0.5f;
            //scene.OfType<DrawableModel>().First().DiffuseColor = Vector3.One * ((float)Math.Sin(totalSeconds) * 0.5f + 0.5f);

            if (pointLight1 != null)
                pointLight1.Transform = Matrix.CreateTranslation(
                                         (float)Math.Cos(totalSeconds) * 10,
                                         (float)Math.Sin(totalSeconds) * 10, 0);

            if (pointLight2 != null)
                pointLight2.Transform = Matrix.CreateTranslation(
                                         (float)Math.Sin(totalSeconds * 2f) * 15,
                                         (float)Math.Cos(totalSeconds * 2f) * 10, 0);

            if (pointLight3 != null)
                pointLight3.Transform = Matrix.CreateTranslation(
                                         (float)Math.Sin(totalSeconds + 10) * 10,
                                         (float)Math.Cos(totalSeconds + 10) * 15, 0);

            if (spotLight != null)
                spotLight.Transform = Matrix.CreateRotationX(MathHelper.PiOver2) *
                                      Matrix.CreateRotationZ(-(float)totalSeconds * 1.5f) *
                    //Matrix.CreateRotationZ(-16.75f) *
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
                //scene.Settings.LightingEnabled = false;
                scene.Settings.MultiPassLightingEnabled = true;
                scene.Settings.PreferHighDynamicRangeLighting = false;
                scene.Settings.PreferDeferredLighting = false;
                //scene.Settings.PreferHighDynamicRangeLighting = keyboardState.IsKeyDown(Keys.H);
                //scene.Settings.PreferDeferredLighting = keyboardState.IsKeyDown(Keys.D);
                scene.Settings.Debug.ShowWireframe = keyboardState.IsKeyDown(Keys.W);
                scene.Settings.Debug.ShowBoundingBox = keyboardState.IsKeyDown(Keys.B);
                scene.Settings.Debug.ShowLightFrustum = keyboardState.IsKeyDown(Keys.L);
                scene.Settings.Debug.ShowSceneManager = keyboardState.IsKeyDown(Keys.S);
                scene.Settings.Debug.ShowShadowMap = keyboardState.IsKeyDown(Keys.M);
                scene.Settings.Debug.ShowStatistics = keyboardState.IsKeyDown(Keys.F);
                scene.Settings.Debug.ShowDepthBuffer = keyboardState.IsKeyDown(Keys.Space);
                scene.Settings.Debug.ShowNormalBuffer = keyboardState.IsKeyDown(Keys.N);
            }

            scene.Draw(gameTime.ElapsedGameTime);

            base.Draw(gameTime);
        }
    }
}