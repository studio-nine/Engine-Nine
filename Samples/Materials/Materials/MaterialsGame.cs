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
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Graphics;
using Nine.Graphics.Effects;
using System.ComponentModel;
using Nine.Components;
using Nine.Graphics.ObjectModel;
#endregion

namespace MaterialsSample
{
    [Category("Graphics")]
    [DisplayName("Materials")]
    [Description("This sample demenstrates how to use LinkedEffect to create various materials.")]
    public class MaterialsGame : Microsoft.Xna.Framework.Game
    {
        Model model;
        ModelBatch modelBatch;
        ModelViewerCamera camera;
        DrawableSurface surface;
        BasicEffect basicEffect;
        LinkedEffect normalMappingEffect;
        LinkedEffect basicDirectionalLightEffect;

        public MaterialsGame()
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

            modelBatch = new ModelBatch(GraphicsDevice);
            model = Content.Load<Model>("dude");
            
            basicDirectionalLightEffect = Content.Load<LinkedEffect>("BasicDirectionalLight");
            basicDirectionalLightEffect.EnableDefaultLighting();

            // Create a topdown perspective editor camera to help us visualize the scene
            camera = new ModelViewerCamera(GraphicsDevice);

            // Create a terrain based on the terrain geometry loaded from file
            surface = new DrawableSurface(GraphicsDevice, 1, 32, 32, 8);
            surface.TextureTransform = TextureTransform.CreateScale(0.25f, 0.25f);
            surface.ConvertVertexType<VertexPositionNormalTangentBinormalTexture>(InitializeSurfaceVertices);
            surface.Position = -surface.BoundingBox.GetCenter();

            normalMappingEffect = Content.Load<LinkedEffect>("NormalMappingEffect");

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.Texture = Content.Load<Texture2D>("box");
            basicEffect.TextureEnabled = true;
            basicEffect.EnableDefaultLighting();
            basicEffect.AmbientLightColor = Vector3.Zero;
        }

        private void InitializeSurfaceVertices(int x, int y, ref VertexPositionColorNormalTexture input, ref VertexPositionNormalTangentBinormalTexture output)
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
            GraphicsDevice.Clear(Color.DarkSlateGray);

            Matrix world = Matrix.CreateScale(0.27f) * Matrix.CreateTranslation(0, -12f, 0);

            // Initialize render state
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Draw the terrain
            BoundingFrustum frustum = new BoundingFrustum(camera.View * camera.Projection);

            modelBatch.Begin(camera.View, camera.Projection);
            modelBatch.Draw(model, world, basicDirectionalLightEffect);
            modelBatch.DrawSurface(surface, normalMappingEffect);
            modelBatch.End();

            base.Draw(gameTime);
        }
    }
}
