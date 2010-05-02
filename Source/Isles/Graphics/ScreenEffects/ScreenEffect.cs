#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Graphics.ScreenEffects
{
    #region IScreenEffect
    /// <summary>
    /// Interface for implementing a custom post processing screen effect.
    /// </summary>
    public interface IScreenEffect
    {
        /// <summary>
        /// Draw the input texture onto the screen with a custom effect.
        /// </summary>
        /// <param name="texture">Input texture to be processed.</param>        
        void Draw(Texture2D texture);
    }
    #endregion

    #region ScreenEffectCollection
    /// <summary>
    /// A collection of Effect and IScreenEffect instances.
    /// </summary>
    public sealed class ScreenEffectCollection : Collection<object>
    {
        /// <summary>
        /// Adds a new Effect instance.
        /// </summary>
        /// <param name="effect">The effect to be added.</param>
        public void Add(Effect effect)
        {
            Add((object)effect);
        }

        /// <summary>
        /// Adds an IScreenEffect instance.
        /// </summary>
        /// <param name="effect">The effect to be added.</param>
        public void Add(IScreenEffect effect)
        {
            Add((object)effect);
        }

        protected override void InsertItem(int index, object item)
        {
            if (!(item is Effect) && !(item is IScreenEffect))
                throw new ArgumentException();

            base.InsertItem(index, item);
        }
    }
    #endregion

    #region ScreenEffect
    /// <summary>
    /// Performs post processing effects.
    /// </summary>
    public sealed class ScreenEffect : IDisposable
    {
        /// <summary>
        /// Gets the effects used during post processing.
        /// </summary>
        public ScreenEffectCollection Effects { get; private set; }

        /// <summary>
        /// Gets the GraphicsDevice associated with this instance.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }


        int current = 0;
        List<RenderTarget2D> renderTargets = new List<RenderTarget2D>(2);


        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        /// <param name="graphics">A GraphicsDevice instance.</param>
        public ScreenEffect(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;

            renderTargets.Add(new RenderTarget2D(
                                              GraphicsDevice,
                                              GraphicsDevice.PresentationParameters.BackBufferWidth,
                                              GraphicsDevice.PresentationParameters.BackBufferHeight, false,
                                              GraphicsDevice.PresentationParameters.BackBufferFormat,
                                              GraphicsDevice.PresentationParameters.DepthStencilFormat));

            Effects = new ScreenEffectCollection();
        }

        /// <summary>
        /// Begins the rendering of the scene to be post processed.
        /// </summary>
        /// <returns></returns>
        public bool Begin()
        {
            if (Effects.Count < 1)
                throw new InvalidOperationException("Must contain at least one effect");

            // We need 2 render target if we have more then 2 effects
            if (Effects.Count > 1 && renderTargets.Count == 1)
            {
                renderTargets.Add(new RenderTarget2D(
                                              GraphicsDevice,
                                              GraphicsDevice.PresentationParameters.BackBufferWidth,
                                              GraphicsDevice.PresentationParameters.BackBufferHeight, false,
                                              GraphicsDevice.PresentationParameters.BackBufferFormat,
                                              GraphicsDevice.PresentationParameters.DepthStencilFormat));
            }

            return renderTargets[current].Begin();
        }

        /// <summary>
        /// Ends the rendering of the scene, applying all the post processing effects.
        /// </summary>
        public void End()
        {
            Rectangle screen;

            screen.X = 0;
            screen.Y = 0;
            screen.Width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            screen.Height = GraphicsDevice.PresentationParameters.BackBufferHeight;


            Texture2D backbuffer = renderTargets[current].End();


            // Draw first n - 1 effects to render target
            for (int i = 0; i < Effects.Count - 1; i++)
            {
                // Switch to next render target
                current = (current + 1) % renderTargets.Count;

                renderTargets[current].Begin();

                Draw(screen, backbuffer, Effects[i]);

                backbuffer = renderTargets[current].End();
            }

            // Draw last effect to the backbuffer
            Draw(screen, backbuffer, Effects[Effects.Count - 1]);
        }

        private void Draw(Rectangle screen, Texture2D backbuffer, object item)
        {
            if (item is Effect)
            {
                GraphicsDevice.DrawSprite(backbuffer, screen, null, Color.White, item as Effect);
            }
            else if (item is IScreenEffect)
            {
                (item as IScreenEffect).Draw(backbuffer);
            }
        }

        /// <summary>
        /// Disposes any resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            foreach (RenderTarget2D renderTarget in renderTargets)
            {
                if (renderTarget != null)
                    renderTarget.Dispose();
            }
        }
    }
    #endregion
}

