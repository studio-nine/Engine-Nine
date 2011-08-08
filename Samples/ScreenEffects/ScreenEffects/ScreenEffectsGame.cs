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
using Nine.Graphics.Effects.Deferred;
#endif
using Nine.Graphics.ScreenEffects;
using System.ComponentModel;
using Nine.Components;
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
#if !WINDOWS_PHONE
        GraphicsBuffer graphicsBuffer;
#endif

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
            screenEffect.Effects.Add(new AdoptionEffect(GraphicsDevice) { Speed = 2 });
            screenEffect.Effects.Add(new WiggleEffect(GraphicsDevice));
            //screenEffect.Effects.Add(ScreenEffect.CreateBloom(GraphicsDevice, 0.5f, 10.0f));
            //screenEffect.Effects.Add(new ColorMatrixEffect(GraphicsDevice) { Transform = ColorMatrix.CreateBrightness(10f) });

            screenEffect = ScreenEffect.CreateMerged(GraphicsDevice, screenEffect,
                           ScreenEffect.CreateDepthOfField(GraphicsDevice, 2, 0, 0, 0.16f),
                           ScreenEffect.CreateHighDynamicRange(GraphicsDevice, 0.5f, 1f, 4f, 5f, 1));

            graphicsBuffer = new GraphicsBuffer(GraphicsDevice);
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
            graphicsBuffer.Begin();
            {
                modelBatch.Begin(ModelSortMode.Immediate, camera.View, camera.Projection);
                modelBatch.Draw(model, worldModel, graphicsBuffer.Effect);
                modelBatch.End();
            }
            graphicsBuffer.End();

            screenEffect.SetTexture(TextureNames.DepthMap, graphicsBuffer.DepthBuffer);
#endif

            // Update the screen effect
            screenEffect.Update(gameTime.ElapsedGameTime);
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

            base.Draw(gameTime);
        }
    }
}
