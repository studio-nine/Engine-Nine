#region File Description
//-----------------------------------------------------------------------------
// BloomComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Graphics.Filters
{
    public abstract class Filter
    {
        public GraphicsDevice GraphicsDevice { get; private set; }
        public float RenderTargetScale { get; set; }


        SpriteBatch spriteBatch;
        RenderTarget2D renderTarget;


        public Filter()
        {
            RenderTargetScale = 1.0f;
        }

        public Texture2D Process(GraphicsDevice graphics, Texture2D input)
        {
            LoadContent(graphics);

            Rectangle rc;

            rc.X = rc.Y = 0;
            rc.Width = input.Width;
            rc.Height = input.Height;

            // Gets the scaled width and height
            int width = (int)(input.Width * RenderTargetScale);
            int height = (int)(input.Height * RenderTargetScale);
            SurfaceFormat format = GraphicsDevice.PresentationParameters.BackBufferFormat;
            

            if (renderTarget == null || renderTarget.Width != width || renderTarget.Height != height)
            {
                if (renderTarget != null)
                    renderTarget.Dispose();
                renderTarget = new RenderTarget2D(GraphicsDevice, width, height, 1, format);
            }


            Draw(graphics, input, rc, renderTarget);

            return renderTarget.GetTexture();
        }


        public void Draw(GraphicsDevice graphics, Texture2D input)
        {
            LoadContent(graphics);

            Rectangle rc;

            rc.X = rc.Y = 0;
            rc.Width = input.Width;
            rc.Height = input.Height;

            Draw(graphics, input, rc, null);
        }

        public void Draw(GraphicsDevice graphics, Texture2D input, Rectangle destination)
        {
            LoadContent(graphics);

            Draw(graphics, input, destination, null);
        }

        public virtual void Draw(GraphicsDevice graphics, Texture2D input, Rectangle destination, RenderTarget2D renderTarget)
        {
            LoadContent(graphics);

            DepthStencilBuffer previousDepth = null;
            RenderTarget2D previousTarget = null;

            if (renderTarget != null)
            {
                previousDepth = GraphicsDevice.DepthStencilBuffer;
                previousTarget = GraphicsDevice.GetRenderTarget(0) as RenderTarget2D;

                GraphicsDevice.DepthStencilBuffer = null;
                GraphicsDevice.SetRenderTarget(0, renderTarget);
            }


            // Draw fullscreen quad
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);

            Begin(input);

            spriteBatch.Draw(input, destination, Color.White);

            End();

            spriteBatch.End();
                        
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            // Draw next effect
            if (renderTarget != null)
            {
                GraphicsDevice.DepthStencilBuffer = previousDepth;
                GraphicsDevice.SetRenderTarget(0, previousTarget);
            }
        }

        private void LoadContent(GraphicsDevice graphics)
        {
            // Initialize everything if we gets drawed for the first time
            if (GraphicsDevice == null)
            {
                if (graphics == null)
                    throw new ArgumentNullException();

                GraphicsDevice = graphics;

                spriteBatch = new SpriteBatch(GraphicsDevice);

                LoadContent();
            }
        }

        protected abstract void LoadContent();
        protected abstract void Begin(Texture2D input);
        protected abstract void End();
    }
}

