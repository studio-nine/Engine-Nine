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
#if !WINDOWS_PHONE
using Nine.Graphics.Effects;
#endif
#endregion

namespace TerrainSample
{
    /// <summary>
    /// Demonstrates how to create a terrain based on a heightmap.
    /// </summary>
    public class TerrainGame : Microsoft.Xna.Framework.Game
    {
        TopDownEditorCamera camera;

        DrawableSurface terrain;
        BasicEffect basicEffect;

#if !WINDOWS_PHONE
        LinkedEffect terrainEffect;
#endif

        public TerrainGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Components.Add(new FrameRate(this, "Consolas"));
            Components.Add(new InputComponent(Window.Handle));
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a topdown perspective editor camera to help us visualize the scene
            camera = new TopDownEditorCamera(GraphicsDevice);

            // Create a terrain based on the terrain geometry loaded from file
            //terrain = new DrawableSurface(GraphicsDevice, Content.Load<Heightmap>("RF1"), 64);
            terrain = new DrawableSurface(GraphicsDevice, Content.Load<Heightmap>("MountainHeightmap"), 8);
            terrain.TextureTransform = TextureTransform.CreateScale(32, 32);
            terrain.Invalidate();

            // Uncomment next line to create a flat terrain
            //terrain = new DrawableSurface(GraphicsDevice, 1, 128, 128, 8);
            terrain.Freeze();

            // Initialize terrain effects
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;

#if !WINDOWS_PHONE
            basicEffect.Texture = Content.Load<Texture2D>("Mountain");
            terrainEffect = Content.Load<LinkedEffect>("TerrainEffect");
#else
            basicEffect.Texture = Content.Load<Texture2D>("Mountain.Low");
#endif
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
            
            // Initialize render state
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Draw the terrain
            BoundingFrustum frustum = new BoundingFrustum(camera.View * camera.Projection);
            
            foreach (DrawableSurfacePatch patch in terrain.Patches)
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
