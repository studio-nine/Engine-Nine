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
using Nine.Animations;
using Nine.Graphics;
using Nine.Graphics.Effects;
using Nine.Graphics.Effects.EffectParts;
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
        Model skyBox;
        ModelBatch modelBatch;
        BoneAnimation animation;

        ModelViewerCamera camera;

        DepthEffect depth;
        SkyBoxEffect skyBoxEffect;
        ShadowMap shadowMap;

        Matrix lightView;
        Matrix lightProjection;
        
        public ShadowMappingGame()
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
            spriteBatch = new SpriteBatch(GraphicsDevice);
            modelBatch = new ModelBatch(GraphicsDevice);

            // Create a model viewer camera to help us visualize the scene
            camera = new ModelViewerCamera(GraphicsDevice);
            camera.Center = Vector3.UnitZ * 15;
            camera.Radius = camera.MaxRadius;

            // Load our model assert.
            model = Content.Load<Model>("dude");
            terrain = Content.Load<Model>("Terrain");

            animation = new BoneAnimation(model, model.GetAnimation(0));
            animation.Play();

            // Create skybox.
            skyBox = Content.Load<Model>("cube");
            skyBoxEffect = new SkyBoxEffect(GraphicsDevice);
            skyBoxEffect.Texture = Content.Load<TextureCube>("SkyCubeMap");

            // Create lights.
            lightView = Matrix.CreateLookAt(new Vector3(10, 10, 30), Vector3.Zero, Vector3.UnitZ);
            lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi / 2.5f, 1, 8.0f, 80.0f);

            // Create shadow map related effects, depth is used to generate shadow maps,
            // shadow is used to draw a shadow receiver with a shadow map.
            shadowMap = new ShadowMap(GraphicsDevice, 1024);
            depth = new DepthEffect(GraphicsDevice);
            
            LoadShadowEffect(terrain, "ShadowEffect");
            LoadShadowEffect(model, "ShadowNormalSkinnedEffect");

            // Enable shadowmap bluring, this will produce smoothed shadow edge.
            // You can increase the shadow quality by using a larger shadowmap resolution (like 1024 or 2048),
            // and increase the blur samples.
            shadowMap.BlurEnabled = true;
        }

        private void LoadShadowEffect(Model model, string effect)
        {
            LinkedEffect shadow = Content.Load<LinkedEffect>(effect);
            shadow.EnableDefaultLighting();

            // Light view and light projection defines a light frustum.
            // Shadows will be projected based on this frustom.
            ShadowMapEffectPart shadowMapEffectPart = shadow.FindFirst<ShadowMapEffectPart>();

            // This value needs to be tweaked
            shadowMapEffectPart.DepthBias = 0.005f;
            shadowMapEffectPart.LightView = lightView;
            shadowMapEffectPart.LightProjection = lightProjection;

            model.ConvertEffectTo(shadow);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Update model animation.
            animation.Update(gameTime);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // These two matrices are to adjust the position of the two models
            Matrix worldModel = Matrix.CreateScale(0.3f) * Matrix.CreateRotationX(MathHelper.PiOver2);
            Matrix worldTerrain = Matrix.CreateTranslation(-5, 0, 1) * Matrix.CreateScale(10f) * Matrix.CreateRotationX(MathHelper.PiOver2);


            // We need to draw the shadow casters on to a render target.
            shadowMap.Begin();
            {
                // Clear everything to white. This is required.
                GraphicsDevice.Clear(Color.White);

                GraphicsDevice.BlendState = BlendState.Opaque;

                // Draw shadow casters using depth effect with the matrices set to light view and projection.
                modelBatch.Begin(lightView, lightProjection);
                modelBatch.DrawSkinned(model, worldModel, model.GetBoneTransforms(), depth);
                modelBatch.End();
            }
            // We got a shadow map rendered.
            shadowMap.End();

            foreach (IEffectTexture effect in model.GetEffects())
                effect.SetTexture(TextureNames.ShadowMap, shadowMap.Texture);

            foreach (IEffectTexture effect in terrain.GetEffects())
                effect.SetTexture(TextureNames.ShadowMap, shadowMap.Texture);

            // Now we begin drawing real scene.
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // Draw all shadow receivers with the shadow effect
            modelBatch.Begin(ModelSortMode.Immediate, camera.View, camera.Projection);
            {
                modelBatch.Draw(skyBox, Matrix.Identity, skyBoxEffect);
                modelBatch.DrawSkinned(model, worldModel, model.GetBoneTransforms(), null);
                modelBatch.Draw(terrain, worldTerrain, null);
            }
            modelBatch.End();


            // Draw shadow map
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                spriteBatch.Begin();
                spriteBatch.Draw(shadowMap.Texture, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
