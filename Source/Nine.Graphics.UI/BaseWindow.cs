namespace Nine.Graphics.UI
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Holds the functions for the window to render.
    /// </summary>
    public abstract class BaseWindow : Pass, IGraphicsObject
    {
        internal static readonly RasterizerState WithClipping = new RasterizerState { ScissorTestEnable = true };
        internal static readonly RasterizerState WithoutClipping = new RasterizerState { ScissorTestEnable = false };

        public Nine.Input Input { get; protected set; }
        public Renderer.Renderer Renderer { get; protected set; }

        /// <summary>
        /// Gets or sets the viewport used by <see cref="Window">RootElement</see> to layout its content.
        /// </summary>
        public Rectangle Viewport { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseWindow">RootElement</see> class.
        /// </summary> 
        protected BaseWindow()
            : this(new Nine.Input()) 
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseWindow">RootElement</see> class.
        /// </summary> 
        protected BaseWindow(Nine.Input input)
        {
            if ((Input = input) == null)
                throw new ArgumentNullException("input");
        }

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
