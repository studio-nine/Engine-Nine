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
using Nine;
using Nine.Graphics;
#endregion

namespace SkinnedModel
{
    /// <summary>
    /// Sample game showing how to display skinned character animation.
    /// </summary>
    public class SkinnedModelGame : Microsoft.Xna.Framework.Game
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

        Model model;
        BoneAnimation animation;
        ModelSkinning skinning;
        ModelBatch modelBatch;

        ModelViewerCamera camera;


        public SkinnedModelGame()
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
            // Create a model viewer camera to help us visualize the scene
            camera = new ModelViewerCamera(GraphicsDevice);

            // Model batch makes drawing models easier
            modelBatch = new ModelBatch(GraphicsDevice);

            // Load our model assert.
            // If the model is processed by our ExtendedModelProcesser,
            // we will try to add model animation and skinning data.
            model = Content.Load<Model>("peon");

            // Now load our model animation and skinning using extension method.
            animation = new BoneAnimation(model, model.GetAnimation(0));
            animation.Speed = 1.0f;

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

            Matrix[] bones = null;
            
            Matrix world = Matrix.CreateTranslation(0, -4000, 0) *
                           Matrix.CreateScale(0.001f);

            // To draw skinned models, first compute bone transforms using model skinning
            if (skinning != null)
                bones = skinning.GetBoneTransforms(model);

            // Pass bone transforms to model batch to draw skinned models
            modelBatch.Begin(ModelSortMode.Immediate, camera.View, camera.Projection);
            modelBatch.Draw(model, world, bones, null);
            modelBatch.End();

            base.Draw(gameTime);
        }
    }
}
