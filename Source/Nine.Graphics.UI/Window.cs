#region License
/* The MIT License
 *
 * Copyright (c) 2011 Red Badger Consulting
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/
#endregion

namespace Nine.Graphics.UI
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Input;

    /// <summary>
    /// RootElement is the main host for all <see cref = "UIElement">UIElement</see>s, it manages the renderer, user input and is the target for Update/Draw calls.
    /// </summary>
    [ContentProperty("Content")]
    public class Window : Pass, IGraphicsObject, IDebugDrawable
    {
        internal static readonly RasterizerState WithClipping = new RasterizerState { ScissorTestEnable = true };
        internal static readonly RasterizerState WithoutClipping = new RasterizerState { ScissorTestEnable = false };
        
        private readonly IInputManager inputManager;
        private UIElement elementWithMouseCapture;
        private SpriteBatch spriteBatch;

        public UIElement Content 
        {
            get { return content; }
            set 
            {
                if (content != value)
                {
                    if (content != null)
                        content.Window = null;
                    content = value;
                    if (content != null)
                        content.Window = this;
                }
            }
        }
        private UIElement content;

        public IInputManager InputManager
        {
            get { return this.inputManager; }
        }

        /// <summary>
        /// Gets or sets the viewport used by <see cref = "Window">RootElement</see> to layout its content.
        /// </summary>
        public Rectangle Viewport { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref = "Window">RootElement</see> class.
        /// </summary> 
        public Window() { }

        /// <summary>
        /// Initializes a new instance of the <see cref = "Window">RootElement</see> class.
        /// </summary>
        /// <param name = "inputManager">An implementation of <see cref = "IInputManager">IInputManager</see> that can be used to respond to user input.</param>
        public Window(IInputManager inputManager)
        {
            if ((this.inputManager = inputManager) != null)
                this.inputManager.GestureSampled += g => NotifyGesture(content, g);
        }

        #region Methods

        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            if (content == null)
                return;

            if (this.Viewport != context.GraphicsDevice.Viewport.Bounds)
                this.Viewport = context.GraphicsDevice.Viewport.Bounds;

            if (Viewport == null)
                throw new ArgumentNullException("Viewport");

            BoundingRectangle bounds = new BoundingRectangle
            {
                X = Viewport.X,
                Y = Viewport.Y,
                Width = Viewport.Width,
                Height = Viewport.Height,
            };

            content.Measure(new Vector2(bounds.Width, bounds.Height));
            content.Arrange(bounds);

            if (content != null)
            {
                // TODO: We might want to render using PrimitiveBatch to allow non-rectangular shapes.
                if (spriteBatch == null)
                    spriteBatch = new SpriteBatch(context.GraphicsDevice);
                spriteBatch.Begin();
                content.OnRender(spriteBatch);
                spriteBatch.End();
            }
        }

        internal bool CaptureMouse(UIElement element)
        {
            if (this.elementWithMouseCapture == null)
            {
                this.elementWithMouseCapture = element;
                return true;
            }
            return false;
        }

        internal void ReleaseMouseCapture(UIElement element)
        {
            if (this.elementWithMouseCapture == element)
            {
                this.elementWithMouseCapture = null;
            }
        }

        private static bool NotifyGesture(UIElement element, Gesture gesture)
        {
            if (element == null)
                return false;

            var children = element.GetChildren();
            if (children != null)
            {
                var handled = false;
                for (int i = children.Count - 1; i >= 0; i++)
                {
                    if (NotifyGesture(children[i], gesture))
                    {
                        handled = true;
                        break;
                    }
                }

                if (!handled && element is IInputElement && element.HitTest(gesture.Vector2))
                {
                    element.NotifyGesture(gesture);
                    return true;
                }
            }
            return false;
        }

#endregion

        #region IGraphicsObject

        void IGraphicsObject.OnAdded(DrawingContext context)
        {
            context.Passes.Add(this);
            AddDependency(context.MainPass);
        }

        void IGraphicsObject.OnRemoved(DrawingContext context)
        {
            context.Passes.Remove(this);
        }

        #endregion

        #region IDebugDrawable

        bool IDebugDrawable.Visible { get { return true; } }
        void IDebugDrawable.Draw(DrawingContext context, DynamicPrimitive primitive)
        {
            if (content != null)
                content.OnDebugRender(primitive);
        }

        #endregion
    }
}
