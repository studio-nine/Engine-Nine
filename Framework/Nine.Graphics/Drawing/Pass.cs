namespace Nine.Graphics.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Markup;
    using System.Xaml;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Content;
    using Nine.Graphics;

    /// <summary>
    /// A drawing pass represents a single pass in the composition chain.
    /// </summary>
    public abstract class Pass : Nine.Object
    {
        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Pass"/> is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the preferred drawing order of this drawing pass.
        /// </summary>
        public int Order
        {
            get { return order; }
            set
            {
                if (order != value)
                {
                    if (Container != null)
                        Container.PassOrderChanged = true;
                    order = value;
                }
            }
        }
        internal int order;
        #endregion

        #region Field
        /// <summary>
        /// Id for this pass, used for dependency sorting.
        /// </summary>
        internal int Id;
        
        /// <summary>
        /// Keeps track of the parent container.
        /// </summary>
        internal PassCollection Container;

        /// <summary>
        /// Each drawing pass can have several dependent passes. All dependent 
        /// passes are drawn before this passes draws.
        /// </summary>
        internal FastList<Pass> DependentPasses;
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="Pass"/> class.
        /// </summary>
        public Pass()
        {
            this.Enabled = true;
        }

        /// <summary>
        /// Indicats this pass with be executed after the specified pass has been executed.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void AddDependency(Pass pass)
        {
            if (DependentPasses == null)
                DependentPasses = new FastList<Pass>();
            DependentPasses.Add(pass);
            if (Container != null)
                Container.TopologyChanged = true;
        }

        /// <summary>
        /// Gets the view and projection matrices contains the objects to be rendered in this pass.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool TryGetViewFrustum(out Matrix view, out Matrix projection)
        {
            view = projection = new Matrix();
            return false;
        }

        /// <summary>
        /// Gets all the pass types that are required by this pass.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected internal virtual void GetDependentPasses(ICollection<Type> passTypes) { }

        /// <summary>
        /// Gets all the passes that are going to be rendered.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void GetActivePasses(IList<Pass> result)
        {
            if (Enabled)
                result.Add(this);
        }

        /// <summary>
        /// Prepares a render target to hold the result of this pass.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual RenderTarget2D PrepareRenderTarget(DrawingContext context, Texture2D input, SurfaceFormat? preferredFormat)
        {
            GraphicsDevice graphics = context.GraphicsDevice;

            if (input != null)
            {
                // Create a render target similar to the input texture and draw the scene onto it.
                return RenderTargetPool.GetRenderTarget(graphics, input.Width, input.Height, preferredFormat ?? input.Format, DepthFormat.Depth24Stencil8);
            }
            
            // Create a render target similar to the back buffer and draw the scene onto it.
            return RenderTargetPool.GetRenderTarget(graphics, graphics.Viewport.Width, graphics.Viewport.Height, preferredFormat ?? graphics.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        /// <param name="drawables">
        /// A list of drawables about to be drawed in this drawing pass.
        /// </param>
        public abstract void Draw(DrawingContext context, IList<IDrawableObject> drawables);
        #endregion
    }
}