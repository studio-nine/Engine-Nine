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
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Input.Touch;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Controls;
    using Nine.Graphics.UI.Input;
    using Nine.Graphics.UI.Renderer;

    /// <summary>
    /// RootElement is the main host for all <see cref="UIElement">UIElement</see>s, it manages the renderer, user input and is the target for Update/Draw calls.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Content")]
    public class Window : Pass, IGraphicsObject
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

        public Nine.Input Input { get; private set; }

        public IRenderer Renderer { get; private set; }

        /// <summary>
        /// Gets or sets the viewport used by <see cref="Window">RootElement</see> to layout its content.
        /// </summary>
        public Rectangle Viewport { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Window">RootElement</see> class.
        /// </summary> 
        public Window()
            : this(new Nine.Input()) 
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Window">RootElement</see> class.
        /// </summary> 
        public Window(Nine.Input input)
        {
            if ((Input = input) == null)
                throw new ArgumentNullException("input");

            Input.MouseMove += MouseMove;
            Input.MouseDown += MouseDown;
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
                if (Renderer == null)
                    Renderer = new SpriteBatchRenderer(context.GraphicsDevice);

                Renderer.Begin(context);
                content.OnRender(Renderer);
                Renderer.End(context);
            }
        }

        #endregion

        #region Find

        public IList<T> FindAll<T>() where T : class
        {
            List<T> result = new List<T>();
            ContainerTraverser.Traverse<T>(content, result);
            return result;
        }

        #endregion

        #region Input

        // I am not sure on the design of the input yet!
        // Tho it is not going to work like this

        void MouseMove(object sender, MouseEventArgs e)
        {
            UIElement element = null;
            if (content.TryGetElement(e.Position.ToVector2(), out element))
            {
                // Events Enter, Hover and Exit?
            }
        }

        void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                UIElement element = null;
                if (content.TryGetElement(e.Position.ToVector2(), out element))
                {
                    var tryButton = element as Button;
                    if (tryButton != null)
                        tryButton.OnClick();
                }
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
