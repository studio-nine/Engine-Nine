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
#endregion

namespace MaterialsSample
{
    [Category("Graphics")]
    [DisplayName("Materials")]
    [Description("This sample demenstrates how to use LinkedEffect to create various materials.")]
    public class MaterialsGame : Microsoft.Xna.Framework.Game
    {
        ModelViewerCamera camera;
        DrawableSurface surface;
        BasicEffect basicEffect;
        LinkedEffect normalMappingEffect;

        public MaterialsGame()
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
            camera = new ModelViewerCamera(GraphicsDevice);

            // Create a terrain based on the terrain geometry loaded from file
            surface = new DrawableSurface(GraphicsDevice, 1, 32, 32, 8);
            surface.ConvertVertexType<VertexPositionNormalTangentBinormalTexture>(InitializeSurfaceVertices);
            surface.Position = -surface.BoundingBox.GetCenter();

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.Texture = Content.Load<Texture2D>("box");
            basicEffect.TextureEnabled = true;

            normalMappingEffect = Content.Load<LinkedEffect>("NormalMappingEffect");
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

            // Initialize render state
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Draw the terrain
            BoundingFrustum frustum = new BoundingFrustum(camera.View * camera.Projection);

            foreach (DrawableSurfacePatch patch in surface.Patches)
            {
                // Cull patches that are outside the view frustum
                if (frustum.Contains(patch.BoundingBox) != ContainmentType.Disjoint)
                {
                    // Setup matrices
                    normalMappingEffect.World = patch.Transform;
                    normalMappingEffect.View = camera.View;
                    normalMappingEffect.Projection = camera.Projection;

                    // Draw each path with the specified effect
                    patch.Draw(normalMappingEffect);
                }
            }

            base.Draw(gameTime);
        }
    }
}
