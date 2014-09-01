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
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Renderer;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Window is the main host for all <see cref="UIElement">UIElement</see>s, it manages the  user input and is the target for Update/Draw calls.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Content")]
    public class Window : BaseWindow, IContainer, IDebugDrawable
    {
        #region Properties

        public UIElement Content 
        {
            get { return content[0]; }
            set 
            {
                if (content[0] != value)
                {
                    if (content[0] != null)
                        content[0].Window = null;
                    content[0] = value;
                    if (content[0] != null)
                        content[0].Window = this;
                }
            }
        }
        private UIElement[] content = new UIElement[1];

        IList IContainer.Children { get { return content; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Window">Window</see> class.
        /// </summary> 
        public Window(Scene scene)
            : base(scene)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Window">Window</see> class.
        /// </summary>
        /// <param name="content"></param>
        public Window(Scene scene, UIElement content)
            : base(scene)
        {
            this.content[0] = content;
        }

        #endregion 

        #region Methods

        internal void Messure()
        {
            content[0].Measure(new Vector2(Viewport.Width, Viewport.Height));
            content[0].Arrange(Viewport);
        }

        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            if (content[0] == null)
                return;

            base.Draw(context, drawables);

            Messure();

            if (Renderer == null)
                Renderer = new SpriteBatchRenderer(context.GraphicsDevice);
                //Renderer = new DynamicPrimitiveRenderer(context.GraphicsDevice);

            Renderer.elapsedTime = context.ElapsedTime;
            Renderer.Begin(context);
            content[0].Draw(Renderer);
            Renderer.End(context);
        }

        #endregion

        #region IDebugDrawable

        bool IDebugDrawable.Visible
        {
            get { return content[0] != null; }
        }

        void IDebugDrawable.Draw(DrawingContext context, Primitives.DynamicPrimitive primitive)
        {
            this.Renderer.Begin(context);
            content[0].DrawBounds(Renderer, new Color(1, 0, 0, 0.1f));
            this.Renderer.End(context);
        }

        #endregion 
    }
}
