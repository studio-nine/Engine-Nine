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
using System.Windows.Forms;
using System.Linq;
using System.IO;
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

        SpriteBatch spriteBatch;

        public ScreenEffectsGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = Screen.PrimaryScreen.Bounds.Width;
            graphics.PreferredBackBufferHeight = Screen.PrimaryScreen.Bounds.Height;
            graphics.IsFullScreen = true;

            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            IsMouseVisible = false;
            Window.AllowUserResizing = false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new InputComponent(Window.Handle));
            Components.Add(new ScreenshotCapturer(GraphicsDevice));

            spriteBatch = new SpriteBatch(GraphicsDevice);

            background = ToTexture(TakeScreenshot());
            /*
            background = DirectX.Interop.CreateFrontBufferTexture(GraphicsDevice);
            DirectX.Interop.GetFrontBuffer(background);
             */
            screenEffect = new ScreenEffect(GraphicsDevice);
            screenEffect.Effects.Add(new AdoptionEffect(GraphicsDevice) { Speed = 2 });
            screenEffect.Effects.Add(new WiggleEffect(GraphicsDevice));
            //screenEffect.Effects.Add(ScreenEffect.CreateBloom(GraphicsDevice, 0.5f, 10.0f));
            //screenEffect.Effects.Add(new ColorMatrixEffect(GraphicsDevice) { Transform = ColorMatrix.CreateBrightness(10f) });

            screenEffect = ScreenEffect.CreateMerged(GraphicsDevice, screenEffect,
                           ScreenEffect.CreateHighDynamicRange(GraphicsDevice, 0.5f, 1f, 4f, 5f, 1));
        }

        private Texture2D ToTexture(System.Drawing.Bitmap image)
        {
            int i = 0;
            Color[] colors = new Color[image.Width * image.Height];
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    var c = image.GetPixel(x, y);
                    colors[i].R = c.R;
                    colors[i].G = c.G;
                    colors[i].B = c.B;
                    colors[i].A = 0xFF;
                    i++;
                }

            var texture = new Texture2D(GraphicsDevice, image.Width, image.Height);
            texture.SetData<Color>(colors);
            return texture;
            /*
            var texture = new Texture2D(GraphicsDevice, image.Width, image.Height);
            texture.SetData<Color>((
                from y in Enumerable.Range(0, image.Height)
                from x in Enumerable.Range(0, image.Width)
                select image.GetPixel(x, y)).Select(c => new Color(c.R, c.G, c.B)).ToArray());
            return texture;
             */
        }

        private System.Drawing.Bitmap TakeScreenshot()
        {
            ScreenCapture sc = new ScreenCapture();
            // capture entire screen, and save it to a file
            System.Drawing.Bitmap img = (System.Drawing.Bitmap)sc.CaptureScreen();
            return img;   
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();

            //DirectX.Interop.GetFrontBuffer(background);

            GraphicsDevice.Clear(Color.DarkSlateGray);

            // Update the screen effect
            screenEffect.Update(gameTime.ElapsedGameTime);

            screenEffect.Begin();
            {
                // Draw the scene between screen effect Begin/End
                spriteBatch.Begin();
                spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, null, Color.White);
                spriteBatch.End();
            }
            screenEffect.End();

            base.Draw(gameTime);
        }
    }
}
