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
using Isles.Graphics.Cameras;
using Isles.Graphics.Models;
#endregion


namespace SkinnedModel
{
    /// <summary>
    /// Sample game showing how to display skinned character animation.
    /// </summary>
    public class SkinnedModelGame : Microsoft.Xna.Framework.Game
    {
        Model model;
        ModelAnimation animation;
        ModelSkinning skinning;
        ModelBatch modelBatch;

        ModelViewerCamera camera;


        public SkinnedModelGame()
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
            camera = new ModelViewerCamera(this);

            // Model batch makes drawing models easier
            modelBatch = new ModelBatch(GraphicsDevice);

            // Load our model assert.
            // If the model is processed by our ExtendedModelProcesser,
            // we will try to add model animation and skinning data.
            model = Content.Load<Model>("dude");

            // Now load our model animation and skinning using extension method.
            animation = new ModelAnimation(model, model.GetAnimation(0));

            skinning = model.GetSkinning();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Update model animation.
            // Note how animations and skinning are seperated.
            animation.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);


            Matrix world = Matrix.CreateTranslation(0, -40, 0) *
                           Matrix.CreateScale(0.3f);

            // To draw skinned models, first compute bone transforms using model skinning
            Matrix[] bones = skinning.GetBoneTransform(model, world);

            // Pass bone transforms to model batch to draw skinned models
            modelBatch.Begin();
            modelBatch.Draw(model, bones, camera.View, camera.Projection);
            modelBatch.End();

            base.Draw(gameTime);
        }
    }
}
