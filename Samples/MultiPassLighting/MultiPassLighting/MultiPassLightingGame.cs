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
#endregion

namespace MultiPassLighting
{
    [Category("Graphics")]
    [DisplayName("Multi-pass lighting")]
    [Description("This sample demenstrates how to use draw arbitray lights using multi pass lighting.")]
    public class MultiPassLightingGame : Microsoft.Xna.Framework.Game
    {
        Model model;
        ModelBatch modelBatch;
        ModelViewerCamera camera;
        BasicEffect basicEffect;
        DirectionalLightEffect directionalLightEffect;
        PointLightEffect pointLightEffect;
        SpotLightEffect spotLightEffect;
        LinkedEffect basicDirectionalLightEffect;

        public MultiPassLightingGame()
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

            model = Content.Load<Model>("dude");

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.EnableDefaultLighting();
            basicEffect.AmbientLightColor = Vector3.Zero;

            basicDirectionalLightEffect = Content.Load<LinkedEffect>("BasicDirectionalLight");
            basicDirectionalLightEffect.Find<IDirectionalLight>().SpecularColor = basicEffect.DirectionalLight0.SpecularColor;
            basicDirectionalLightEffect.Find<IDirectionalLight>().Direction = basicEffect.DirectionalLight0.Direction;
            basicDirectionalLightEffect.Find<IDirectionalLight>().DiffuseColor = basicEffect.DirectionalLight0.DiffuseColor;

            directionalLightEffect = new DirectionalLightEffect(GraphicsDevice);
            directionalLightEffect.Lights[0].DiffuseColor = basicEffect.DirectionalLight1.DiffuseColor;
            directionalLightEffect.Lights[0].Direction = basicEffect.DirectionalLight1.Direction;
            directionalLightEffect.Lights[0].SpecularColor = basicEffect.DirectionalLight1.SpecularColor;

            directionalLightEffect.Lights[1].DiffuseColor = basicEffect.DirectionalLight2.DiffuseColor;
            directionalLightEffect.Lights[1].Direction = basicEffect.DirectionalLight2.Direction;
            directionalLightEffect.Lights[1].SpecularColor = basicEffect.DirectionalLight2.SpecularColor;
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

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                modelBatch.Begin(camera.View, camera.Projection);
                modelBatch.Draw(model, world, basicEffect);
                modelBatch.End();
            }
            else
            {
                modelBatch.Begin(camera.View, camera.Projection);
                modelBatch.Draw(model, world, basicDirectionalLightEffect);
                modelBatch.End();

                modelBatch.Begin(0, camera.View, camera.Projection, BlendState.Additive, null, DepthStencilState.DepthRead, null);
                modelBatch.Draw(model, world, directionalLightEffect);
                modelBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
