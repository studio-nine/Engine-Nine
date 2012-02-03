#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Graphics.Effects;
using Nine.Graphics.ObjectModel;
using Model = Microsoft.Xna.Framework.Graphics.Model;
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
        BasicEffect solidEffect;
        BasicEffect surfaceEffect;
        DrawableSurface surface;
        DirectionalLightEffect directionalLightEffect1;
        DirectionalLightEffect directionalLightEffect2;
        PointLightEffect pointLightEffect;
        SpotLightEffect spotLightEffect;

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

            solidEffect = new BasicEffect(GraphicsDevice);
            solidEffect.TextureEnabled = true;
            solidEffect.LightingEnabled = true;
            solidEffect.AmbientLightColor = Vector3.Zero;
            solidEffect.DirectionalLight0.Enabled = false;

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.EnableDefaultLighting();
            basicEffect.AmbientLightColor = Vector3.Zero;

            directionalLightEffect1 = new DirectionalLightEffect(GraphicsDevice);

            directionalLightEffect1.Lights[0].DiffuseColor = basicEffect.DirectionalLight0.DiffuseColor;
            directionalLightEffect1.Lights[0].Direction = basicEffect.DirectionalLight0.Direction;
            directionalLightEffect1.Lights[0].SpecularColor = basicEffect.DirectionalLight0.SpecularColor;

            directionalLightEffect1.Lights[1].DiffuseColor = basicEffect.DirectionalLight1.DiffuseColor;
            directionalLightEffect1.Lights[1].Direction = basicEffect.DirectionalLight1.Direction;
            directionalLightEffect1.Lights[1].SpecularColor = basicEffect.DirectionalLight1.SpecularColor;

            directionalLightEffect1.Lights[2].DiffuseColor = Vector3.Zero;
            directionalLightEffect1.Lights[3].DiffuseColor = Vector3.Zero;

            directionalLightEffect2 = new DirectionalLightEffect(GraphicsDevice);
            directionalLightEffect2.Lights[0].DiffuseColor = basicEffect.DirectionalLight2.DiffuseColor;
            directionalLightEffect2.Lights[0].Direction = basicEffect.DirectionalLight2.Direction;
            directionalLightEffect2.Lights[0].SpecularColor = basicEffect.DirectionalLight2.SpecularColor;

            directionalLightEffect2.Lights[1].DiffuseColor = Vector3.Zero;
            directionalLightEffect2.Lights[2].DiffuseColor = Vector3.Zero;
            directionalLightEffect2.Lights[3].DiffuseColor = Vector3.Zero;


            // Create a terrain based on the terrain geometry loaded from file
            surface = new DrawableSurface(GraphicsDevice, 1, 32, 32, 8);
            surface.ConvertVertexType<VertexPositionNormalTangentBinormalTexture>(InitializeSurfaceVertices);
            surface.Position = -surface.BoundingBox.GetCenter();

            surfaceEffect = new BasicEffect(GraphicsDevice);
            surfaceEffect.Texture = Content.Load<Texture2D>("box");
            surfaceEffect.TextureEnabled = true;
            surfaceEffect.LightingEnabled = true;
            surfaceEffect.PreferPerPixelLighting = true;
            surfaceEffect.AmbientLightColor = Vector3.One * 0.1f;
            surfaceEffect.DirectionalLight0.DiffuseColor = Vector3.One * 0.1f;
            surfaceEffect.DirectionalLight0.SpecularColor = Vector3.Zero;
            surfaceEffect.DirectionalLight0.Direction = new Vector3(0, - 0.707107f, - 0.707107f);

            pointLightEffect = new PointLightEffect(GraphicsDevice);
            pointLightEffect.Texture = Content.Load<Texture2D>("box");
            pointLightEffect.Lights[0].DiffuseColor = new Vector3(1, 1, 0);
            pointLightEffect.Lights[0].Position = new Vector3(2, 2, 1f);
            pointLightEffect.Lights[0].Range = 5;
            pointLightEffect.Lights[0].SpecularColor = Vector3.One;

            pointLightEffect.Lights[1].DiffuseColor = Vector3.Zero;
            pointLightEffect.Lights[2].DiffuseColor = Vector3.Zero;
            pointLightEffect.Lights[3].DiffuseColor = Vector3.Zero;

            spotLightEffect = new SpotLightEffect(GraphicsDevice);
            spotLightEffect.Texture = Content.Load<Texture2D>("box");
            spotLightEffect.Lights[0].DiffuseColor = new Vector3(1, 0, 0);
            spotLightEffect.Lights[0].Position = new Vector3(0, 0, 1);
            spotLightEffect.Lights[0].Direction = new Vector3(-1, 0, 0);
            spotLightEffect.Lights[0].Range = 16;
            spotLightEffect.Lights[0].Falloff = 2;
            spotLightEffect.Lights[0].SpecularColor = Vector3.One;

            spotLightEffect.Lights[1].DiffuseColor = Vector3.Zero;
            spotLightEffect.Lights[2].DiffuseColor = Vector3.Zero;
            spotLightEffect.Lights[3].DiffuseColor = Vector3.Zero;
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
                modelBatch.Begin(ModelSortMode.Immediate, camera.View, camera.Projection);
                modelBatch.Draw(model, world, solidEffect);
                modelBatch.DrawSurface(surface, surfaceEffect);
                modelBatch.End();

                modelBatch.Begin(0, camera.View, camera.Projection, BlendState.Additive, null, DepthStencilState.DepthRead, null);
                modelBatch.Draw(model, world, directionalLightEffect1);
                modelBatch.Draw(model, world, directionalLightEffect2);
                modelBatch.DrawSurface(surface, pointLightEffect);
                modelBatch.DrawSurface(surface, spotLightEffect);
                modelBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
