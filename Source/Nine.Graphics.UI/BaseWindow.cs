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

        // TODO: ???
        public WindowManager Manager
        {
            get { return manager; }
        }
        internal WindowManager manager;

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
        protected BaseWindow()
        {

        }

        #endregion

        #region Methods

        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {

        }

        protected internal virtual void MouseMove(object sender, MouseEventArgs e) { }
        protected internal virtual void MouseUp(object sender, MouseEventArgs e) { }
        protected internal virtual void MouseDown(object sender, MouseEventArgs e) { }
        protected internal virtual void MouseWheel(object sender, MouseEventArgs e) { }
        protected internal virtual void KeyDown(object sender, KeyboardEventArgs e) { }
        protected internal virtual void KeyUp(object sender, KeyboardEventArgs e) { }
        protected internal virtual void ButtonUp(object sender, GamePadEventArgs e) { }
        protected internal virtual void ButtonDown(object sender, GamePadEventArgs e) { }
        protected internal virtual void GestureSampled(object sender, GestureEventArgs e) { }

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
