#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
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
using Isles;
using Isles.Graphics;
using Isles.Graphics.Cameras;
using Isles.Navigation.Flocking;
#endregion


namespace FlockingBehaviors
{
    /// <summary>
    /// Sample game for full screen post processing effects.
    /// </summary>
    public class FlockingBehaviorGame : Microsoft.Xna.Framework.Game
    {
        SpriteFont font;
        SpriteBatch spriteBatch;

        TopDownEditorCamera camera;

        GridObjectManager objects;
        BoundingBox bounds;
        List<FlockingMovement> movingEntities = new List<FlockingMovement>();


        public FlockingBehaviorGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
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
            camera = new TopDownEditorCamera(this);


            // Create a grid object manager to keep track of object position.
            // This manager will be used by group flocking behaviors to detect neighbors.
            objects = new GridObjectManager(64, 64, 0, 0, 16, 16);

            
            // Create a bounds for the world.
            bounds = new BoundingBox(new Vector3(-32, -32, -10), new Vector3(32, 32, 10));


            FlockingMovement movement;


            // Arrive
            movement = new FlockingMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new ArriveBehavior());
            movingEntities.Add(movement);

            // Seek
            movement = new FlockingMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new SeekBehavior());
            movingEntities.Add(movement);

            // Pursuit
            movement = new FlockingMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new PursuitBehavior() { Evader = movingEntities[1] });
            movingEntities.Add(movement);

            // Evade
            movement = new FlockingMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new EvadeBehavior() { Pursuer = movingEntities[1], ThreatRange = 16 });
            movingEntities.Add(movement);

            // Wander
            movement = new FlockingMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new WanderBehavior(), 0.8f);
            movingEntities.Add(movement);

            // Idle
            //movement = new FlockingMovement();
            //movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            //movement.Behaviors.Add(new IdleBehavior() { Range = 10 }, 0.8f);
            //movingEntities.Add(movement);

            // Group behavior
            for (int i = 0; i < 100; i++)
            {
                movement = new FlockingMovement();
                movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
                movement.Behaviors.Add(new EvadeBehavior() { Pursuer = movingEntities[1], ThreatRange = 16 }, 2.0f);
                movement.Behaviors.Add(new SeparationBehavior() { SpacialObjectManager = objects, SeparationRadius = 2.5f }, 0.8f);
                movement.Behaviors.Add(new CohesionBehavior() { SpacialObjectManager = objects, GroupRadius = 20 }, 0.8f);
                movement.Behaviors.Add(new AlignmentBehavior() { SpacialObjectManager = objects, GroupRadius = 20 }, 0.8f);
                movement.Behaviors.Add(new WanderBehavior(), 0.8f);
                movingEntities.Add(movement);
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {            
            // Gets the pick ray from current mouse cursor
            Ray ray = PickEngine.RayFromScreen(GraphicsDevice, Mouse.GetState().X, 
                                                               Mouse.GetState().Y, camera.View, camera.Projection);

            float? distance = ray.Intersects(new Plane(Vector3.UnitZ, 0));

            if (distance.HasValue)
            {
                Vector3 target = ray.Position + ray.Direction * distance.Value;

                // Let our moving entities steer towards the mouse
                foreach (FlockingMovement movable in movingEntities)
                {
                    if (movable.Behaviors.Get<SeekBehavior>() != null)
                        movable.Behaviors.Get<SeekBehavior>().Target = target;
                    if (movable.Behaviors.Get<ArriveBehavior>() != null)
                        movable.Behaviors.Get<ArriveBehavior>().Target = target;
                }
            }


            // Update object manager since the position of moving entities change every frame.            
            objects.Clear();

            foreach (FlockingMovement movable in movingEntities)
            {
                objects.Add(movable, movable.Position.X, movable.Position.Y);
            }


            // Update all moving entities
            foreach (FlockingMovement movable in movingEntities)
            {
                movable.Update(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);


            DebugVisual.Alpha = 1.0f;
            DebugVisual.View = camera.View;
            DebugVisual.Projection = camera.Projection;


            // Draw grid
            DebugVisual.DrawGrid(GraphicsDevice, Vector3.Zero, 2, 32, 32, Color.Gray);
            DebugVisual.DrawGrid(GraphicsDevice, Vector3.Zero, 16, 4, 4, Color.Yellow);


            // Draw world bounds
            DebugVisual.DrawBox(GraphicsDevice, bounds, new Color(Color.Gray, 80));


            // Draw all moving entities
            for (int i = 0; i < movingEntities.Count; i++)
            {
                if (i == 0)
                    DebugVisual.DrawPoint(GraphicsDevice, movingEntities[i].Position, Color.Red, 1.4f);
                else
                    DebugVisual.DrawPoint(GraphicsDevice, movingEntities[i].Position, Color.White, 1.0f);

                Vector3 screenPosition = GraphicsDevice.Viewport.Project(movingEntities[i].Position, camera.Projection, camera.View, Matrix.Identity);

                if (movingEntities[i].Behaviors.Count < 3)
                {
                    spriteBatch.Begin();
                    spriteBatch.DrawString(font, movingEntities[i].Behaviors[1].GetType().Name, new Vector2(screenPosition.X, screenPosition.Y - 20), Color.White);
                    spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }
    }
}
