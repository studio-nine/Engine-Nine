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
using System.ComponentModel;
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
using Nine.Components;
using Nine.Graphics.ObjectModel;
#endregion

namespace TerrainSample
{
    [Category("Graphics")]
    [DisplayName("Skinned Animation")]
    [Description("This sample demenstrates how to create and draw heightmap based terrain.")]
    public class TerrainGame : Microsoft.Xna.Framework.Game
    {
        TopDownEditorCamera camera;

        ModelBatch modelBatch;
        PrimitiveBatch primitiveBatch;
        DrawableSurface terrain;
        BasicEffect basicEffect;
        Vector3 pickedPosition;
        RasterizerState wireframe;        

        public TerrainGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

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
#if !WINDOWS_PHONE
            Components.Add(new ScreenshotCapturer(GraphicsDevice));
#endif

            // Create a topdown perspective editor camera to help us visualize the scene
            camera = new TopDownEditorCamera(GraphicsDevice);
            modelBatch = new ModelBatch(GraphicsDevice);
            primitiveBatch = new PrimitiveBatch(GraphicsDevice, 10000);

            wireframe = new RasterizerState() { FillMode = FillMode.WireFrame, CullMode = CullMode.None };

            // Create a terrain based on the terrain geometry loaded from file
            terrain = new DrawableSurface(GraphicsDevice, Content.Load<Heightmap>("MountainHeightmap"), 32);
            //terrain = new DrawableSurface(GraphicsDevice, 1, 16, 16, 8);
            
            // Center the terrain to the camera
            terrain.Position = -terrain.Center;

            // Enable terrain level of detail
            terrain.LevelOfDetailEnabled = true;
            terrain.LevelOfDetailStart = 50;
            terrain.LevelOfDetailEnd = 100;

            // Initialize terrain effects
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
#if WINDOWS_PHONE
            basicEffect.Texture = Content.Load<Texture2D>("Mountain.Low");
#else
            basicEffect.Texture = Content.Load<Texture2D>("Mountain");
#endif
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            Vector3 positionWorld = new Vector3();
            if (terrain.TryPick(GraphicsDevice.Viewport, camera.View, camera.Projection, out positionWorld))
            {
                pickedPosition = positionWorld;
            }
            base.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // Update terrain level of detail
            Vector3 eyePosition = Matrix.Invert(camera.View).Translation;
            terrain.UpdateLevelOfDetail(eyePosition);

            // Draw the terrain
            BoundingFrustum frustum = new BoundingFrustum(camera.View * camera.Projection);

            modelBatch.Begin(ModelSortMode.Deferred, camera.View, camera.Projection, BlendState.AlphaBlend, SamplerState.LinearWrap, null,
                Keyboard.GetState().IsKeyDown(Keys.Space) ? wireframe : null);
            {
                foreach (var patch in terrain.Patches)
                {
                    // Cull patches that are outside the view frustum
                    if (frustum.Contains(patch.BoundingBox) != ContainmentType.Disjoint)
                    {
                        modelBatch.DrawSurface(patch, basicEffect);
                    }
                }
            }
            modelBatch.End();

            primitiveBatch.Begin(camera.View, camera.Projection);
            primitiveBatch.DrawSolidSphere(new BoundingSphere(pickedPosition, 1), 5, null, Color.Red);
            primitiveBatch.End();

            base.Draw(gameTime);
        }
    }
}
