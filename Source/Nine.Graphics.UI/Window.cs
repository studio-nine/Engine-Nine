#region License
/* The MIT License
 *
 * Copyright (c) 2013 Engine Nine
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
    using Microsoft.Xna.Framework.Input;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Primitives;

    /// <summary>
    /// RootElement is the main host for all <see cref="UIElement">UIElement</see>s, it manages the renderer, user input and is the target for Update/Draw calls.
    /// </summary>
    [ContentProperty("Content")]
    public class Window : Pass, IGraphicsObject, Nine.IUpdateable
    {
        internal static readonly RasterizerState WithClipping = new RasterizerState { ScissorTestEnable = true };
        internal static readonly RasterizerState WithoutClipping = new RasterizerState { ScissorTestEnable = false };

        private DynamicPrimitive dynamicPrimitive;

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

        /// <summary>
        /// Gets or sets the viewport used by <see cref="Window">RootElement</see> to layout its content.
        /// </summary>
        public Rectangle Viewport { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Window">RootElement</see> class.
        /// </summary> 
        public Window() { }

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
                if (dynamicPrimitive == null)
                    dynamicPrimitive = new DynamicPrimitive(context.GraphicsDevice);

                content.OnRender(dynamicPrimitive);

                dynamicPrimitive.Draw(context, null);
                dynamicPrimitive.Clear();
            }
        }

        #endregion

        #region Input

        public void Update(float elapsedTime)
        {
            var mouse = Mouse.GetState();

            if (content.HitTest(new Vector2(mouse.X, mouse.Y)))
            {

            }
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
    }
}
