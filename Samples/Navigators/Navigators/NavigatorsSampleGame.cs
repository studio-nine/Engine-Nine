using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Nine.Graphics.ObjectModel;
using Nine.Graphics;
using Nine.Components;
using Nine;

namespace Navigators
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class NavigatorsSampleGame : Microsoft.Xna.Framework.Game
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
        Model peonModel;
        NavigatedModel[] bots;

        public NavigatorsSampleGame()
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
            Components.Add(new InputComponent(Window.Handle));

            scene = new Scene(GraphicsDevice);
            scene.Settings.DefaultFont = Content.Load<SpriteFont>("Consolas");

            peonModel = Content.Load<Model>("Models/Peon");

#if WINDOWS_PHONE
            scene.Camera = new TopDownEditorCamera(GraphicsDevice) { Pitch = MathHelper.ToRadians(30), Yaw=1.6f, Radius = 20f};
#else
            scene.Camera = new FreeCamera(GraphicsDevice, new Vector3(190, 30, 20));
#endif
            scene.Add(Content.Load<DisplayObject>("NavigatorsScene"));
            var terrain = scene.Find<DrawableSurface>("Terrain");


            //Init a bunch of npc that will just stay in the mountains
            int npcCount = 5;
            bots = new NavigatedModel[npcCount];
            Neighbors friends = new Neighbors();
            for (int i = 0; i < npcCount; i++)
            {
                var tempGuy = new NavigatedModel(peonModel);
                //Indicate the ground
                tempGuy.Navigator.Ground = terrain;
                //Indicate initial position of the navigator
                tempGuy.Navigator.Position = new Vector3(20 + i, 20 + i, 0);
                tempGuy.LoadContent();
                bots[i] = tempGuy;
                friends.Add(tempGuy.Navigator);
            }

            for (int i = 0; i < npcCount; i++)
            {
                //All npc consider each other friends (meaning that will give space for other friends passing by)
                bots[i].Navigator.Friends = friends;
                scene.Add(bots[i]);
            }

            //Waypoints the moveableGuy would be walking
            List<Vector3> waypoints = new List<Vector3> { 
                new Vector3(20, -50, 0),
                new Vector3(20, 50 ,0),            
                new Vector3(60, 0, 0),
                new Vector3(60, 50, 0),
                new Vector3(-40, 50, 0),
            };

            var movableGuy = new NavigatedModel(peonModel);
            movableGuy.Navigator.Ground = terrain;
            movableGuy.Navigator.Position = new Vector3(20, -50, 0);
            movableGuy.LoadContent();
            friends.Add(movableGuy.Navigator);
            movableGuy.Navigator.Friends = friends;
            scene.Add(movableGuy);

            //Command to move along points
            movableGuy.Navigator.MoveAlong(waypoints);

        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            scene.Draw(gameTime.ElapsedGameTime);
            base.Draw(gameTime);
        }
    }
}
