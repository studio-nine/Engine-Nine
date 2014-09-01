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
        
        internal int ZDepth = -1;

        #region Properties

        /// <summary>
        /// Gets or sets the renderer system.
        /// </summary>
        public Renderer.Renderer Renderer { get; set; }

        /// <summary>
        /// Gets or sets the viewport used by <see cref="BaseWindow">RootElement</see> to layout its content.
        /// </summary>
        public BoundingRectangle Viewport { get; set; }
        // TODO: Why did I change this to Floats?

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseWindow">RootElement</see> class.
        /// </summary> 
        protected BaseWindow(Scene scene)
        {
            // TODO: rework on this design
            scene.GetWindowManager();
        }

        #endregion

        #region Methods

        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            // TODO: How should I handle the Viewport?
            if (this.Viewport == BoundingRectangle.Empty)
                this.Viewport = (BoundingRectangle)context.GraphicsDevice.Viewport.TitleSafeArea;
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
