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
using Nine.Graphics.Effects;
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

            GraphicsAdapter.UseReferenceDevice = false;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;
#if WINDOWS
            graphics.EnablePerfHudProfiling();
#endif

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

#if WINDOWS_PHONE
            scene = Content.Load<Scene>("Scene.WindowsPhone");
            scene.Camera = new TopDownEditorCamera(GraphicsDevice) { Pitch = MathHelper.ToRadians(30) };
#elif XBOX
            scene = Content.Load<Scene>("Scene");
            scene.Camera = new TopDownEditorCamera(GraphicsDevice) { Pitch = MathHelper.ToRadians(30) };
#else
            scene = Content.Load<Scene>("Scene");
            scene.Camera = new FreeCamera(GraphicsDevice, new Vector3(-10, -30, 10));
            //scene.Camera = scene.Find<Camera>("MyCamera");
#endif
            scene.Settings.DefaultFont = Content.Load<SpriteFont>("Consolas");

            base.LoadContent(); 
        }

        int frame = 0;
        /// <summary>
        /// This is called when the game should update itself.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            if (frame++ > 1000)
                ;// Exit();
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back))
                Exit();

            double totalSeconds = gameTime.TotalGameTime.TotalSeconds;
                        
            var pickRay = GraphicsDevice.Viewport.CreatePickRay(
                Mouse.GetState().X, Mouse.GetState().Y, scene.Camera.View, scene.Camera.Projection);

            var pickedObject = scene.Find(pickRay);
            if (pickedObject.Target != null)
            {
                Window.Title = string.Format("Target {0}, OriginalTarget {1}, Distance {2}", pickedObject.Target,
                                                                                             pickedObject.OriginalTarget,
                                                                                             pickedObject.Distance);
            }
            else
            {
                Window.Title = "";
            }

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
                //scene.Settings.PreferHighDynamicRangeLighting = keyboardState.IsKeyDown(Keys.H);
                scene.Settings.Debug.ShowWireframe = keyboardState.IsKeyDown(Keys.V);
                scene.Settings.Debug.ShowBoundingBox = keyboardState.IsKeyDown(Keys.B);
                scene.Settings.Debug.ShowLightFrustum = keyboardState.IsKeyDown(Keys.L);
                scene.Settings.Debug.ShowSceneManager = keyboardState.IsKeyDown(Keys.C);
                scene.Settings.Debug.ShowShadowMap = keyboardState.IsKeyDown(Keys.M);
                scene.Settings.Debug.ShowStatistics = keyboardState.IsKeyDown(Keys.F);
                scene.Settings.Debug.ShowDepthBuffer = keyboardState.IsKeyDown(Keys.Space);
                scene.Settings.Debug.ShowNormalBuffer = keyboardState.IsKeyDown(Keys.N);
                scene.Settings.Debug.ShowLightBuffer = keyboardState.IsKeyDown(Keys.K);
            }

            scene.Draw(gameTime.ElapsedGameTime);

            base.Draw(gameTime);

            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            
        }
    }
}