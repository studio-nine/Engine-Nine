#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Animations;
using Nine.Components;
using Nine.Graphics;
using Nine.Graphics.Effects;
using Nine.Graphics.Primitives;
#endregion

namespace ShadowMapping
{
    [Category("Graphics")]
    [DisplayName("Shadow Mapping")]
    [Description("This sample demenstrates how to draw shadows using shadow mapping.")]
    public class ShadowMappingGame : Microsoft.Xna.Framework.Game
    {
        SpriteBatch spriteBatch;

        Model model;
        Model terrain;
        ModelBatch modelBatch;
        PrimitiveBatch primitiveBatch;
        BoneAnimation animation;

        ModelViewerCamera camera;

        TextureCube skyBoxTexture;
        ShadowMap shadowMap;

        Matrix lightViewProjection;
        
        public ShadowMappingGame()
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

            spriteBatch = new SpriteBatch(GraphicsDevice);
            modelBatch = new ModelBatch(GraphicsDevice);
            primitiveBatch = new PrimitiveBatch(GraphicsDevice);

            // Create a model viewer camera to help us visualize the scene
            camera = new ModelViewerCamera(GraphicsDevice);
            camera.Center = Vector3.UnitZ * 15;
            camera.Radius = camera.MaxRadius;

            // Load our model assert.
            model = Content.Load<Model>("dude");
            terrain = Content.Load<Model>("Terrain");

            animation = new BoneAnimation(new ModelSkeleton(model), model.GetAnimation(0));
            animation.Play();

            // Create skybox.
            skyBoxTexture = Content.Load<TextureCube>("SkyCubeMap");

            // Create lights.
            lightViewProjection = Matrix.CreateLookAt(new Vector3(10, 10, 30), Vector3.Zero, Vector3.UnitZ) *
                                  Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi / 2.5f, 1, 8.0f, 80.0f);

            //lightViewProjection = Matrix.CreateLookAt(new Vector3(10, 10, 30), Vector3.Zero, Vector3.UnitZ) *
            //                      Matrix.CreateOrthographic(40, 40, 1, 80);

            // Create shadow map related effects, depth is used to generate shadow maps,
            // shadow is used to draw a shadow receiver with a shadow map.
            shadowMap = new ShadowMap(GraphicsDevice, 1024);
            
            LoadShadowEffect(terrain, "ShadowEffect");
            LoadShadowEffect(model, "ShadowNormalSkinnedEffect");
        }

        private void LoadShadowEffect(Model model, string effect)
        {
            LinkedEffect shadow = Content.Load<LinkedEffect>(effect);
            shadow.EnableDefaultLighting();

            // Light view and light projection defines a light frustum.
            // Shadows will be projected based on this frustom.
            IEffectShadowMap shadowMapEffectPart = shadow.Find<IEffectShadowMap>();

            // This value needs to be tweaked
            shadowMapEffectPart.DepthBias = 0.005f;
            shadowMapEffectPart.LightViewProjection = lightViewProjection;

            model.ConvertEffectTo(shadow);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Update model animation.
            animation.Update(gameTime.ElapsedGameTime);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // These two matrices are to adjust the position of the two models
            Matrix worldModel = Matrix.CreateScale(0.3f) * Matrix.CreateRotationX(MathHelper.PiOver2);
            Matrix worldTerrain = Matrix.CreateTranslation(-5, 0, 1) * Matrix.CreateScale(10f) * Matrix.CreateRotationX(MathHelper.PiOver2);


            // We need to draw the shadow casters on to a render target.
            // ShadowMap.Begin will clear everything to white for us.
            shadowMap.SurfaceFormat = SurfaceFormat.Color;
            shadowMap.Begin();
            {
                GraphicsDevice.BlendState = BlendState.Opaque;

                // Draw shadow casters using depth effect with the matrices set to light view and projection.
                modelBatch.Begin(Matrix.Identity, lightViewProjection);
                modelBatch.DrawSkinned(model, worldModel, animation.Skeleton.GetSkinTransforms(), shadowMap.Effect);
                modelBatch.End();
            }
            // We got a shadow map rendered.
            shadowMap.End();

            foreach (IEffectTexture effect in model.GetEffects())
                effect.SetTexture(TextureUsage.ShadowMap, shadowMap.Texture);

            foreach (IEffectTexture effect in terrain.GetEffects())
                effect.SetTexture(TextureUsage.ShadowMap, shadowMap.Texture);

            // Now we begin drawing real scene.
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // Draw all shadow receivers with the shadow effect
            modelBatch.Begin(ModelSortMode.Immediate, camera.View, camera.Projection);
            {
                modelBatch.DrawSkyBox(skyBoxTexture);
                modelBatch.DrawSkinned(model, worldModel, animation.Skeleton.GetSkinTransforms(), null);
                modelBatch.Draw(terrain, worldTerrain, null);
            }
            modelBatch.End();


            // Draw shadow map
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
                spriteBatch.Draw(shadowMap.Texture, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                primitiveBatch.Begin(camera.View, camera.Projection);
                primitiveBatch.DrawSkeleton(model, worldModel, Color.White);
                primitiveBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
