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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Graphics;
using System.ComponentModel;
using Nine.Graphics.Primitives;
using Nine.Components;
using Nine.Graphics.Views;
using System.Xml.Serialization;
#endregion

namespace Game
{
    [Category("Graphics")]
    [DisplayName("Sample Game")]
    [Description("This sample demenstrates the use of Nine.Game")]
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
        private const int BackBufferWidth = 900;
        private const int BackBufferHeight = 600;
#endif

        World world;
        Renderer renderer;

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

            world = new World();
            world.WorldObjects.Add(1);
            world.WorldObjects.Add("sss");
            world.WorldObjects.Add(new FreeObject());
            world.WorldObjects.Add(new WorldObject());
            world.Save("SimpleWorld.xml");
            
            world = World.FromFile("SimpleWorld.xml");

            renderer = new Renderer(GraphicsDevice);
            renderer.Camera = new ModelViewerCamera(GraphicsDevice);

            world.WorldObjects.Add(Content.Load<object>("BasicModelView"));
            world.CreateGraphics(GraphicsDevice);

            base.LoadContent();
        }


        /// <summary>
        /// This is called when the game should update itself.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            world.Update(gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            world.Draw(gameTime.ElapsedGameTime);

            //renderer.Draw(gameTime.ElapsedGameTime, world.WorldObjects.Select(x => x as IDrawableView));

            base.Draw(gameTime);
        }
    }
}