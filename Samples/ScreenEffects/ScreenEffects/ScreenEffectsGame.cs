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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Graphics;
#if !WINDOWS_PHONE
using Nine.Graphics.Effects;
#endif
using Nine.Graphics.ScreenEffects;
using System.ComponentModel;
#endregion

namespace ScreenEffects
{
    [Category("Graphics")]
    [DisplayName("Screen Effects")]
    [Description("This sample demenstrates how to create full screen post processing effects.")]
    public class ScreenEffectsGame : Microsoft.Xna.Framework.Game
    {
        ScreenEffect screenEffect;
        Texture2D background;
        Model model;
        ModelBatch modelBatch;
        ModelViewerCamera camera;
        SpriteBatch spriteBatch;
        Effect modelEffect;
        Effect depthEffect;

        public ScreenEffectsGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
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

            model = Content.Load<Model>("dude");
            background = Content.Load<Texture2D>("glacier");
            
            camera = new ModelViewerCamera(GraphicsDevice);
            camera.Center = Vector3.UnitZ * 15;
            camera.Radius = camera.MaxRadius;

            BasicEffect basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.EnableDefaultLighting();
            modelEffect = basicEffect;


#if WINDOWS_PHONE
            screenEffect = ScreenEffect.CreateAdditive(GraphicsDevice, 1);
#else
            screenEffect = new ScreenEffect(GraphicsDevice);
            //screenEffect.Effects.Add(new AntiAliasEffect(GraphicsDevice));
            //screenEffect.Effects.Add(new AdoptionEffect(GraphicsDevice) { Speed = 2 });
            //screenEffect.Effects.Add(new WiggleEffect(GraphicsDevice));
            //screenEffect.Effects.Add(ScreenEffect.CreateBloom(GraphicsDevice, 0.5f, 10.0f));
            //screenEffect.Effects.Add(new ColorMatrixEffect(GraphicsDevice) { Transform = ColorMatrix.CreateBrightness(10f) });
            //screenEffect = ScreenEffect.CreateHighDynamicRange(GraphicsDevice, 0.5f, 1f, 4f, 5f, 1);
            screenEffect = ScreenEffect.CreateDepthOfField(GraphicsDevice, 2, 0, 0, 0.16f);

            depthEffect = new DepthEffect(GraphicsDevice);
#endif
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            Matrix worldModel = Matrix.CreateTranslation(Vector3.UnitY * -32);
            Window.Title = RenderTargetPool.ActiveRenderTargets.ToString() + " " + RenderTargetPool.TotalRenderTargets.ToString();


            GraphicsDevice.Clear(Color.DarkSlateGray);

#if !WINDOWS_PHONE
            RenderTarget2D depth = RenderTargetPool.AddRef(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 
                                                           false, SurfaceFormat.Single, GraphicsDevice.PresentationParameters.DepthStencilFormat);

            depth.Begin();
            {
                GraphicsDevice.Clear(Color.White);

                modelBatch.Begin(ModelSortMode.Immediate, camera.View, camera.Projection);
                modelBatch.Draw(model, worldModel, depthEffect);
                modelBatch.End();
            }
            depth.End();

            screenEffect.SetTexture(TextureNames.DepthMap, depth);
#endif

            // Update the screen effect
            screenEffect.Update(gameTime);
            screenEffect.Enabled = !Keyboard.GetState().IsKeyDown(Keys.Space);

            screenEffect.Begin();
            {
                // Draw the scene between screen effect Begin/End
                spriteBatch.Begin();
                spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, null, Color.White);
                spriteBatch.End();

                modelBatch.Begin(ModelSortMode.Immediate, camera.View, camera.Projection);
                modelBatch.Draw(model, worldModel, modelEffect);
                modelBatch.End();
            }
            screenEffect.End();

#if !WINDOWS_PHONE
            RenderTargetPool.Release(depth);
#endif

            base.Draw(gameTime);
        }
    }
}
