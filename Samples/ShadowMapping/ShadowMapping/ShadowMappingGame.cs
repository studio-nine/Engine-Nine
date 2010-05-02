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
using Isles.Graphics.Effects;
using Isles.Graphics.ScreenEffects;
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
        SpriteBatch spriteBatch;

        Model model;
        Model terrain;

        TopDownEditorCamera camera;

        DepthEffect depth;
        ShadowEffect shadow;
        ShadowMap shadowMap;


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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a model viewer camera to help us visualize the scene
            camera = new TopDownEditorCamera(this);

            // Load our model assert.
            model = Content.Load<Model>("dude");
            terrain = Content.Load<Model>("Terrain");

            // Create shadow map related effects, depth is used to generate shadow maps,
            // shadow is used to draw a shadow receiver with a shadow map.
            depth = new DepthEffect(GraphicsDevice);
            shadow = new ShadowEffect(GraphicsDevice);
            shadowMap = new ShadowMap(GraphicsDevice, 256);

            // Enable shadowmap bluring, this will produce smoothed shadow edge.
            // You can increase the shadow quality by using a larger shadowmap resolution (like 1024 or 2048),
            // and increase the blur samples.
            //shadowMap.Blur.SampleCount = BlurEffect.MaxSampleCount;
            shadowMap.BlurEnabled = true;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Light view and light projection defines a light frustum.
            // Shadows will be projected based on this frustom.
            shadow.LightView = Matrix.CreateLookAt(new Vector3(10, 10, 30), Vector3.Zero, Vector3.UnitZ);
            shadow.LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi / 2.5f, 1, 8.0f, 80.0f);


            // These two matrices are to adjust the position of the two models
            Matrix worldModel = Matrix.CreateScale(0.3f) * Matrix.CreateRotationX(MathHelper.PiOver2);
            Matrix worldTerrain = Matrix.CreateTranslation(-5, 0, 1) * Matrix.CreateScale(10f) * Matrix.CreateRotationX(MathHelper.PiOver2);


            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;            


            // We need to draw the shadow casters on to a render target.
            if (shadowMap.Begin())
            {
                // Clear everything to black. This is required.
                GraphicsDevice.Clear(Color.White);

                // Draw shadow casters using depth effect with the matrices set to light view and projection.
                DrawModel(model, depth, worldModel, shadow.LightView, shadow.LightProjection);

                // We got a shadow map rendered.
                shadow.ShadowMap = shadowMap.End();
            }

            // Now we begin drawing real scene.
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // This value needs to be tweaked
            shadow.DepthBias = 0.005f;

            DrawModel(model, shadow, worldModel, camera.View, camera.Projection);

            // Draw all shadow receivers with the shadow effect
            DrawModel(terrain, shadow, worldTerrain, camera.View, camera.Projection);

            // Draw shadow map
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                spriteBatch.Begin();
                spriteBatch.Draw(shadow.ShadowMap, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }


        /// <summary>
        /// When XNA 4.0 released, IEffectMatrices will be integrated to ModelBatch,
        /// then we don't need these chunck of tricky code to draw a model with a custom effect.
        /// </summary>
        private void DrawModel(Model model, Effect effect, Matrix world, Matrix view, Matrix projection)
        {
            Matrix[] bones = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(bones);

            foreach (ModelMesh mesh in model.Meshes)
            {
                BasicEffect[] effects = new BasicEffect[mesh.MeshParts.Count];

                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    // Store the basic effect associated with the model
                    effects[i] = mesh.MeshParts[i].Effect as BasicEffect;

                    // If we are using the default basic effect
                    if (effect == null)
                    {
                        effects[i].EnableDefaultLighting();
                        effect = effects[i];
                    }

                    mesh.MeshParts[i].Effect = effect;

                    // Set effect properties through reflection
                    SetProperty(effect, "World", bones[mesh.ParentBone.Index] * world);
                    SetProperty(effect, "View", view);
                    SetProperty(effect, "Projection", projection);
                    SetProperty(effect, "Texture", effects[i].Texture);
                }

                mesh.Draw();

                // Restore basic effect
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    mesh.MeshParts[i].Effect = effects[i];
                }
            }
        }

        private void SetProperty(Effect effect, string field, object value)
        {
            if (effect.GetType().GetProperty(field) != null)
                effect.GetType().GetProperty(field).SetValue(effect, value, null);
        }
    }
}
