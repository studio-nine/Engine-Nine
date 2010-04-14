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
using Isles;
using Isles.Graphics;
using Isles.Graphics.Effects;
using Isles.Graphics.Cameras;
using Isles.Graphics.Models;
#endregion


namespace ShadowMapping
{
    /// <summary>
    /// Sample game showing how to draw shadows using shadow mapping.
    /// </summary>
    public class ShadowMappingGame : Microsoft.Xna.Framework.Game
    {
        Model model;
        Model terrain;
        ModelBatch modelBatch;

        ModelViewerCamera light;
        TopDownEditorCamera camera;


        ShadowMapEffect shadowMap;
        ShadowMapCasterEffect shadowCaster;
        ShadowMapReceiverEffect shadowReceiver;


        public ShadowMappingGame()
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
            // Create a model viewer camera to help us visualize the scene
            light = new ModelViewerCamera(this);
            camera = new TopDownEditorCamera(this);

            // Model batch makes drawing models easier
            modelBatch = new ModelBatch();

            // Load our model assert.
            model = Content.Load<Model>("dude");
            terrain = Content.Load<Model>("Terrain");

            // Create shadow map related effects
            shadowMap = new ShadowMapEffect(GraphicsDevice);

            shadowCaster = new ShadowMapCasterEffect();
            shadowReceiver = new ShadowMapReceiverEffect();
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
            

            Matrix worldModel = Matrix.CreateScale(0.3f) *
                                Matrix.CreateRotationX(MathHelper.PiOver2);


            Matrix worldTerrain = Matrix.CreateTranslation(-5, 0, 0) *
                                  Matrix.CreateScale(10f) *
                                  Matrix.CreateRotationX(MathHelper.PiOver2);

            if (shadowMap.Begin())
            {
                // Generate shadow map
                shadowCaster.LightViewProjection =
                shadowReceiver.LightViewProjection = light.View * light.Projection;

                modelBatch.Begin();
                modelBatch.Draw(model, shadowCaster, gameTime, worldModel, camera.View, camera.Projection);
                modelBatch.End();

                shadowReceiver.Texture = shadowMap.End();

                // Draw Scene
                modelBatch.Begin();
                modelBatch.Draw(model, worldModel, camera.View, camera.Projection);
                modelBatch.Draw(terrain, worldTerrain, camera.View, camera.Projection);
                modelBatch.End();


                // Draw shadow map overlay for shadow receivers
                modelBatch.Begin();
                modelBatch.Draw(terrain, shadowReceiver, gameTime, worldTerrain, camera.View, camera.Projection);
                modelBatch.End();
            }


            base.Draw(gameTime);
        }
    }
}
