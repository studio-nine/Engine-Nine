#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Navigation.Steering;
#endregion

namespace FlockingBehaviors
{
    /// <summary>
    /// Sample game for full screen post processing effects.
    /// </summary>
    public class FlockingBehaviorGame : Microsoft.Xna.Framework.Game
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
        ScreenCamera camera;
        BasicEffect basicEffect;

        SpriteFont font;
        SpriteBatch spriteBatch;
        Texture2D butterfly;

        GridSceneManager<Steerable> objects;

        BoundingRectangle bounds;

        Random random = new Random();

        List<Steerable> movingEntities = new List<Steerable>();
        

        public FlockingBehaviorGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);
            
            Content.RootDirectory = "Content";

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

            camera = new ScreenCamera(GraphicsDevice, ScreenCameraCoordinate.TwoDimension);
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = true;

            // Create a sprite batch to draw text on to the screen
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Consolas");
            butterfly = Content.Load<Texture2D>("Butterfly");
            
            // Create a bounds for the world.
            bounds = new BoundingRectangle(GraphicsDevice.Viewport.Bounds);


            // Create a grid object manager to keep track of object position.
            // This manager will be used by group flocking behaviors to detect neighbors.
            objects = new GridSceneManager<Steerable>(bounds, 16, 16);


            Steerable movement;

            // Group behavior
            for (int i = 0; i < 200; i++)
            {
                movement = new Steerable();
                movement.BoundingRadius = 5;
                movement.MaxSpeed = 100;
                movement.Acceleration = 200;
                movement.Position = NextPosition();
                movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds, Skin = 20 });
                movement.Behaviors.Add(new WanderBehavior(), 0.5f);
                movement.Behaviors.Add(new SeparationBehavior() { Neighbors = objects, Range = 10f }, 0.5f);
                movement.Behaviors.Add(new CohesionBehavior() { Neighbors = objects, Range = 100f }, 0.5f);
                movement.Behaviors.Add(new AlignmentBehavior() { Neighbors = objects, Range = 100f }, 0.5f);
                movingEntities.Add(movement);
            }
        }

        private Vector2 NextPosition()
        {
            // Randomize positions
            Vector2 position = new Vector2();

            position.X = (float)random.NextDouble() * (bounds.Max.X - bounds.Min.X) + bounds.Min.X;
            position.Y = (float)random.NextDouble() * (bounds.Max.Y - bounds.Min.Y) + bounds.Min.Y;

            return position;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {            
            // Update object manager since the position of moving entities change every frame.            
            objects.Clear();

            foreach (Steerable movable in movingEntities)
            {
                objects.Add(movable, new Vector3(movable.Position, 0), 0);
            }


            // Update all moving entities
            foreach (Steerable movable in movingEntities)
            {
                movable.Update(gameTime.ElapsedGameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            Rectangle viewport = GraphicsDevice.Viewport.Bounds;

            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, basicEffect);

            for (int i = 0; i < movingEntities.Count; i++)
            {
                spriteBatch.Draw(butterfly, movingEntities[i].Position, null, Color.Gold,
                                 (float)Math.Atan2(movingEntities[i].Forward.Y, movingEntities[i].Forward.X) + MathHelper.PiOver2,
                                 new Vector2(butterfly.Width / 2, butterfly.Height / 2), 2 * movingEntities[i].BoundingRadius / butterfly.Width,
                                 SpriteEffects.None, 0);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
