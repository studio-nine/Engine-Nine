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
    public interface IFilter
    {
        void Draw(GraphicsDevice graphics, Texture2D input, RenderTarget2D renderTarget);
    }


    public abstract class Filter : IFilter
    {
        public GraphicsDevice GraphicsDevice { get; private set; }


        SpriteBatch spriteBatch;
        RenderTarget2D renderTarget;


        public void Draw(GraphicsDevice graphics, Texture2D input, RenderTarget2D renderTarget)
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

            Begin(input, renderTarget);

            Rectangle destination = Rectangle.Empty;

            if (renderTarget == null)
            {
                destination.Width = GraphicsDevice.PresentationParameters.BackBufferWidth;
                destination.Height = GraphicsDevice.PresentationParameters.BackBufferHeight;
            }
            else
            {
                destination.Width = renderTarget.Width;
                destination.Height = renderTarget.Height;
            }

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
        protected abstract void Begin(Texture2D input, RenderTarget2D renderTarget);
        protected abstract void End();
    }
}

