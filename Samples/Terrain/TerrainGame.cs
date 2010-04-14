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
using Isles.Graphics.Landscape;
#endregion


namespace TerrainSample
{
    /// <summary>
    /// Demonstrates how to create a terrain based on a heightmap.
    /// </summary>
    public class TerrainGame : Microsoft.Xna.Framework.Game
    {
        TopDownEditorCamera camera;

        Terrain terrain;
        BasicEffect basicEffect;
        

        public TerrainGame()
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
            // Create a topdown perspective editor camera to help us visualize the scene
            camera = new TopDownEditorCamera(this);


            // Create a terrain based on the terrain geometry loaded from file
            terrain = new Terrain(GraphicsDevice, Content.Load<TerrainGeometry>("RF1"), 8);


            // Initialize terrain effects
            basicEffect = new BasicEffect(GraphicsDevice, null);
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight0.DiffuseColor = Color.Yellow.ToVector3();
            basicEffect.DirectionalLight0.Direction = Vector3.Normalize(-Vector3.One);
            basicEffect.LightingEnabled = true;
            basicEffect.PreferPerPixelLighting = true;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);


            // Toggle wireframe when W is pressed
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            else
                GraphicsDevice.RenderState.FillMode = FillMode.Solid;


            // Draw the terrain
            BoundingFrustum frustum = new BoundingFrustum(camera.View * camera.Projection);

            foreach (TerrainPatch patch in terrain.Patches)
            {
                // Cull patches that are outside the view frustum
                if (frustum.Contains(patch.BoundingBox) != ContainmentType.Disjoint)
                {
                    // Setup matrices
                    basicEffect.World = patch.Transform;
                    basicEffect.View = camera.View;
                    basicEffect.Projection = camera.Projection;

                    // Draw each path with the specified effect
                    patch.Draw(basicEffect);
                }
            }


            base.Draw(gameTime);
        }
    }
}
