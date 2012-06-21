#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Graphics.ObjectModel;
using Nine.Physics;

#endregion

namespace Tutorial
{
    public class Tutorial : Microsoft.Xna.Framework.Game
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
        private const int BackBufferHeight = 720;
#endif

        private World world;
        private Scene scene;
        private string[] tutorials;
        private int nextTutorial;
        private Input input;
        
        public Tutorial()
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
            // Add a frame rate counter component.
            // FrameRate is not an Xna game component, it is achieved using an extension method
            // defined in Nine.Components. So you have to include that namespace first.
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));

            // An input component is required for framework to handle input events.
            // You can optionally pass in the handle to the current window, so the input event
            // is driven by windows messages on Windows.
            // This can avoid the event missing problem for Xna pulled input model when frame rate is low.
            Components.Add(new InputComponent(Window.Handle));

            // Loads the names of tutorial files
            tutorials = Content.Load<string[]>("Tutorials/Tutorials");

            // Shows the next scene
            LoadNextScene();
            
            // Create an event based input handler.
            // Note that you have to explictly keep a strong reference to the Input intance.
            input = new Input();
            input.MouseDown += new EventHandler<MouseEventArgs>(Input_MouseDown);
            input.ButtonDown += new EventHandler<GamePadEventArgs>(Input_ButtonDown);

            base.LoadContent();
        }

        private void Input_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                LoadNextScene();
        }

        private void Input_ButtonDown(object sender, GamePadEventArgs e)
        {
            if (e.Button == Buttons.A)
                LoadNextScene();
        }

        /// <summary>
        /// Loads the next scene.
        /// </summary>
        private void LoadNextScene()
        {
            var tutorial = Content.Load<object>(tutorials[nextTutorial]);

            // Since some tutorials are worlds and some of them are scenes,
            // provide a unified initalization for them.
            world = tutorial as World;
            scene = tutorial as Scene;

            if (world != null)
            {
                // After a world is loaded, you have to explicitly initialize the
                // content and graphics of the world.
                world.CreateContent(Content);
                scene = world.CreateGraphics(GraphicsDevice);

                // To enable phyiscs, explicitly create the physics world.
                world.CreatePhysics(true);
            }

            scene.Camera = new FreeCamera(GraphicsDevice, new Vector3(0, -40, 10));           
            scene.Settings.DefaultDebugControlEnabled = true;
            scene.Settings.DefaultFont = Content.Load<SpriteFont>("Consolas");

            Window.Title = tutorials[nextTutorial];

            nextTutorial = (nextTutorial + 1) % tutorials.Length;
        }

        /// <summary>
        /// This is called when the game should update itself.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back))
                Exit();

            if (world != null)
                world.Update(gameTime.ElapsedGameTime);
            else if (scene != null)
                scene.Update(gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            if (world != null)
                world.Draw(gameTime.ElapsedGameTime);
            else if (scene != null)
                scene.Draw(gameTime.ElapsedGameTime);

            base.Draw(gameTime);
        }
    }
}