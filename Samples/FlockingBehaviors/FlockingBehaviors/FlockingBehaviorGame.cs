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
using Nine.Navigation.SteeringBehaviors;
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

        SpriteFont font;
        SpriteBatch spriteBatch;

        TopDownEditorCamera camera;

        GridObjectManager objects;

        BoundingBox bounds;

        Random random = new Random();

        List<SteeringMovement> movingEntities = new List<SteeringMovement>();
        

        public FlockingBehaviorGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);
            
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            Components.Add(new InputComponent(Window.Handle));
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a sprite batch to draw text on to the screen
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Consolas");


            // Create a topdown perspective editor camera to help us visualize the scene
            camera = new TopDownEditorCamera(GraphicsDevice);

            
            // Create a bounds for the world.
            bounds = new BoundingBox(new Vector3(-32, -32, -10), new Vector3(32, 32, 10));


            // Create a grid object manager to keep track of object position.
            // This manager will be used by group flocking behaviors to detect neighbors.
            objects = new GridObjectManager(64, 64, 0, 0, 16, 16);


            SteeringMovement movement;

            // Arrive
            movement = new SteeringMovement();
            // By setting MaxForce to a large value, acceleration is removed.
            // Try uncomment the next line to see the effect.
            //movement.MaxForce = float.MaxValue;            
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new ObstacleAvoidanceBehavior() { Obstacles = objects });
            // By increasing Deceleration and decreasing DecelerateRange, deceleration is removed.
            // Try uncomment the parameters in the next line to see the effect.
            movement.Behaviors.Add(new ArriveBehavior() { /* Deceleration = 100, DecelerateRange = 0.25f */ });
            movingEntities.Add(movement);

            // Evade
            movement = new SteeringMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new ObstacleAvoidanceBehavior() { Obstacles = objects });
            movement.Behaviors.Add(new EvadeBehavior() { Pursuer = movingEntities[0], ThreatRange = 16 });
            movingEntities.Add(movement);

            // Wander
            movement = new SteeringMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new ObstacleAvoidanceBehavior() { Obstacles = objects });
            movement.Behaviors.Add(new WanderBehavior(), 0.8f);
            movingEntities.Add(movement);

            // Idle
            movement = new SteeringMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new ObstacleAvoidanceBehavior() { Obstacles = objects });
            movement.Behaviors.Add(new IdleBehavior() { Range = 10 }, 0.8f);
            movingEntities.Add(movement);
            

            // Group behavior
            for (int i = 0; i < 200; i++)
            {
                movement = new SteeringMovement();
                movement.Position = NextPosition();
                movement.Resistance = 6.0f;
                movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
                movement.Behaviors.Add(new SeparationBehavior() { SpatialObjectManager = objects, SeparationRadius = 2.35f });
                movingEntities.Add(movement);
            }
        }

        private Vector3 NextPosition()
        {
            // Randomize positions
            Vector3 position = new Vector3();

            position.X = (float)random.NextDouble() * (bounds.Max.X - bounds.Min.X) + bounds.Min.X;
            position.Y = (float)random.NextDouble() * (bounds.Max.Y - bounds.Min.Y) + bounds.Min.Y;
            position.Z = 0;

            return position;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {            
            // Gets the pick ray from current mouse cursor
            Ray ray = PickEngine.RayFromScreen(GraphicsDevice, Mouse.GetState().X, 
                                                               Mouse.GetState().Y, 
                                                               camera.View, 
                                                               camera.Projection);

            // Test ray against ground plane
            float? distance = ray.Intersects(new Plane(Vector3.UnitZ, 0));

            if (distance.HasValue)
            {
                Vector3 target = ray.Position + ray.Direction * distance.Value;

                // Let our moving entities steer towards the mouse
                foreach (SteeringMovement movable in movingEntities)
                {
                    if (movable.Behaviors.Get<SeekBehavior>() != null)
                        movable.Behaviors.Get<SeekBehavior>().Target = target;
                    if (movable.Behaviors.Get<ArriveBehavior>() != null)
                        movable.Behaviors.Get<ArriveBehavior>().Target = target;
                }
            }


            // Update object manager since the position of moving entities change every frame.            
            objects.Clear();

            foreach (SteeringMovement movable in movingEntities)
            {
                objects.Add(movable, movable.Position.X, movable.Position.Y);
            }


            // Update all moving entities
            foreach (SteeringMovement movable in movingEntities)
            {
                movable.Update(gameTime);

                // Since our steering behavior is in 3D, but this sample only demonstrate
                // 2D behaviors, we have to snap the moving entities to the ground.
                movable.Position = new Vector3(movable.Position.X, movable.Position.Y, 0);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            DebugVisual.Alpha = 1.0f;
            DebugVisual.View = camera.View;
            DebugVisual.Projection = camera.Projection;


            // Draw grid
            DebugVisual.DrawGrid(GraphicsDevice, Vector3.Zero, 2, 32, 32, Color.Gray);


            // Draw all moving entities
            for (int i = 0; i < movingEntities.Count; i++)
            {
                Color color = (i == 0 ? Color.Gold :
                              (i == 1 ? Color.GreenYellow :
                              (i == 2 ? Color.Silver :
                              (i == 3 ? Color.Pink : Color.CornflowerBlue))));

                DebugVisual.DrawLine(GraphicsDevice,
                                     movingEntities[i].Position,
                                     movingEntities[i].Position + Vector3.UnitZ * 4, 
                                     movingEntities[i].BoundingRadius, color);
            }

            // Draw world bounds
            DebugVisual.DrawBox(GraphicsDevice, bounds, Matrix.Identity, Color.Gray * 0.4f);


            // Draw states
            spriteBatch.Begin();

            for (int i = 0; i < movingEntities.Count; i++)
            {
                if (movingEntities[i].Behaviors.Get<SeparationBehavior>() == null)
                {
                    Vector3 screenPosition = GraphicsDevice.Viewport.Project(movingEntities[i].Position, camera.Projection, camera.View, Matrix.Identity);

                    spriteBatch.DrawString(font, movingEntities[i].Behaviors[2].GetType().Name,
                                           new Vector2(screenPosition.X, screenPosition.Y - 20), Color.White);
                }
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
