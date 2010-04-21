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

        GridObjectManager objects = new GridObjectManager(64, 64, 0, 0, 16, 16);
        BoundingBox bounds = new BoundingBox(new Vector3(-32, -32, -10), new Vector3(32, 32, 10));
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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Consolas");

            // Create a topdown perspective editor camera to help us visualize the scene
            camera = new TopDownEditorCamera(this);


            FlockingMovement movement;

            // Wander
            movement = new FlockingMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new WanderBehavior(), 0.8f);
            movingEntities.Add(movement);

            // Seek
            movement = new FlockingMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new SeekBehavior());
            movingEntities.Add(movement);

            // Arrive
            movement = new FlockingMovement();
            movement.Behaviors.Add(new BoundAvoidanceBehavior() { Bounds = bounds });
            movement.Behaviors.Add(new ArriveBehavior());
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

            //movement.Behaviors.Add(new EvadeBehavior() { ThreatRange = 16.0f, Pursuer = movingEntities[0] }, 0.9f);
            //movement.Behaviors.Add(new PursuitBehavior() { Evader = movingEntities[0] }, 0.7f);
            //movement.Behaviors.Add(new SeparationBehavior() { SpacialObjectManager = objects, SeparationRadius = 10 }, 0.8f);
            //movement.Behaviors.Add(new CohesionBehavior() { SpacialObjectManager = objects, CohesionRadius = 100 }, 0.8f);
            //movement.Behaviors.Add(new AlignmentBehavior() { SpacialObjectManager = objects, GroupRadius = 100 }, 0.8f);
            //movement.Behaviors.Add(new WanderBehavior(), 0.8f);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {            
            Ray ray = PickEngine.RayFromScreen(GraphicsDevice, Mouse.GetState().X, 
                                                               Mouse.GetState().Y, camera.View, camera.Projection);

            float? distance = ray.Intersects(new Plane(Vector3.UnitZ, 0));

            if (distance.HasValue)
            {
                Vector3 target = ray.Position + ray.Direction * distance.Value;

                foreach (FlockingMovement movable in movingEntities)
                {
                    if (movable.Behaviors.Get<SeekBehavior>() != null)
                        movable.Behaviors.Get<SeekBehavior>().Target = target;
                    if (movable.Behaviors.Get<ArriveBehavior>() != null)
                        movable.Behaviors.Get<ArriveBehavior>().Target = target;
                }
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
                              

                spriteBatch.Begin();
                spriteBatch.DrawString(font, movingEntities[i].Behaviors[1].GetType().Name, new Vector2(screenPosition.X, screenPosition.Y - 20), Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
