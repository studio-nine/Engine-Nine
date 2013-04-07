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
            Input.MouseWheel += MouseWheel;
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
        // TODO: Control Tabbing with Focus

        void MouseMove(object sender, MouseEventArgs e)
        {
            if (content == null)
                return;

            UIElement result = null;
            if (TryGetElement(new Vector2(e.X, e.Y), out result))
            {
                result.InvokeMouseMove(this, new MouseEventArgs(e.Button, e.X, e.Y, e.WheelDelta));
            }
        }

        void MouseDown(object sender, MouseEventArgs e)
        {
            if (content == null)
                return;

            UIElement result = null;
            if (TryGetElement(new Vector2(e.X, e.Y), out result))
            {
                result.InvokeMouseDown(this, new MouseEventArgs(e.Button, e.X, e.Y, e.WheelDelta));
            }
        }

        void MouseWheel(object sender, MouseEventArgs e)
        {
            if (content == null)
                return;

            UIElement result = null;
            if (TryGetElement(new Vector2(e.X, e.Y), out result))
            {
                result.InvokeMouseWheel(this, new MouseEventArgs(e.Button, e.X, e.Y, e.WheelDelta));
            }
        }

        private bool TryGetElement(Vector2 point, out UIElement output)
        {
            UIElement result = null;
            ContainerTraverser.Traverse<UIElement>(content,
                (d) =>
                {
                    if (d.HitTest(point))
                    {
                        result = d;
                        var container = d as IContainer;
                        if (container != null)
                            if (container.Children.Count > 0)
                                return TraverseOptions.Continue;
                        return TraverseOptions.Stop;
                    }
                    else
                        return TraverseOptions.Skip;
                });

            output = result;
            if (result != null)
                return true;
            else
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
    }
}
