#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Graphics;
using Nine.Graphics.Effects;
using Nine.Graphics.Effects.Deferred;
using Nine.Graphics.ScreenEffects;
using Nine.Components;
using Nine.Graphics.ObjectModel;
using Model = Microsoft.Xna.Framework.Graphics.Model;
#endregion

namespace DeferredLighting
{
    [Category("Graphics")]
    [DisplayName("Deferred Lighting")]
    [Description("This sample demenstrates how to use deferred lighting to render many lights.")]
    public class DeferredLightingGame : Microsoft.Xna.Framework.Game
    {
        Model model;
        ModelViewerCamera camera;
        DrawableSurface surface;
        ModelBatch modelBatch;
        Texture2D boxTexture;
        Texture2D boxNormalTexture;
        DeferredEffect deferredEffect;
        GraphicsBuffer graphicsBuffer;
        List<IDeferredLight> lights;

        public DeferredLightingGame()
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

            // Create a topdown perspective editor camera to help us visualize the scene
            camera = new ModelViewerCamera(GraphicsDevice);
            modelBatch = new ModelBatch(GraphicsDevice);

            // Create a terrain based on the terrain geometry loaded from file
            surface = new DrawableSurface(GraphicsDevice, 1, 32, 32, 8);
            surface.TextureTransform = TextureTransform.CreateScale(0.25f, 0.25f);
            surface.ConvertVertexType<VertexPositionNormalTangentBinormalTexture>(InitializeSurfaceVertices);
            surface.Position = -surface.BoundingBox.GetCenter();

            graphicsBuffer = new GraphicsBuffer(GraphicsDevice);
            deferredEffect = new DeferredEffect(GraphicsDevice);
            deferredEffect.SpecularColor = Vector3.One;
            boxTexture = Content.Load<Texture2D>("box");
            boxNormalTexture = Content.Load<Texture2D>("box_n");

            // Create lights
            lights = new List<IDeferredLight>();
            lights.Add(new DeferredAmbientLight(GraphicsDevice) { AmbientLightColor = Vector3.One * 0.1f });
            lights.Add(new DeferredDirectionalLight(GraphicsDevice) { DiffuseColor = Vector3.One * 0.1f, SpecularColor = Vector3.Zero });
            lights.Add(new DeferredPointLight(GraphicsDevice) { DiffuseColor = new Vector3(1, 1, 0), Position = new Vector3(2, 2, 1f), Range = 5, SpecularColor = Vector3.One });
            lights.Add(new DeferredSpotLight(GraphicsDevice) { DiffuseColor = new Vector3(1, 0, 0), Position = new Vector3(0, 0, 1), Direction = new Vector3(-1, 0, 0), Range = 16, Falloff = 2, SpecularColor = Vector3.One });
        }

        private void InitializeSurfaceVertices(int xPatch, int yPatch, int x, int y, ref VertexPositionNormalTexture input, ref VertexPositionNormalTangentBinormalTexture output)
        {
            output.Position = input.Position;
            output.Normal = input.Normal;
            output.TextureCoordinate = input.TextureCoordinate;
            output.Tangent = Vector3.UnitX;
            output.Binormal = Vector3.UnitY;
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
            // Initialize render state
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // 1. Draw deferred scene with DepthNormalEffect first.
            graphicsBuffer.Begin();
            {
                graphicsBuffer.Effect.NormalMap = boxNormalTexture;
                graphicsBuffer.Effect.NormalMappingEnabled = true;
                DrawTerrain(graphicsBuffer.Effect);
            }
            graphicsBuffer.End();

            // 2. Draw all the lights
            graphicsBuffer.DrawLights(camera.View, camera.Projection, lights);

            // 3. Draw the scene using DeferredEffect.
            GraphicsDevice.Clear(Color.DarkSlateGray);

            deferredEffect.Texture = boxTexture;
            deferredEffect.LightTexture = graphicsBuffer.LightBuffer;
            DrawTerrain(deferredEffect);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the terrain with the specified effect.
        /// </summary>
        private void DrawTerrain(Effect effect)
        {
            BoundingFrustum frustum = new BoundingFrustum(camera.View * camera.Projection);

            foreach (var patch in surface.Patches)
            {
                // Cull patches that are outside the view frustum
                if (frustum.Contains(patch.BoundingBox) != ContainmentType.Disjoint)
                {
                    // Setup matrices
                    IEffectMatrices matrices = effect as IEffectMatrices;
                    if (matrices != null)
                    {
                        matrices.World = patch.Transform;
                        matrices.View = camera.View;
                        matrices.Projection = camera.Projection;
                    }
                    // Draw each path with the specified effect
                    patch.Draw(effect);
                }
            }
        }
    }
}
