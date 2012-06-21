#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// A drawing pass represents a single pass in the composition chain.
    /// </summary>
    [RuntimeNameProperty("Name")]
    [DictionaryKeyProperty("Name")]
    public abstract class DrawingPass
    {
        #region Properties
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DrawingPass"/> is enabled.
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

        /// <summary>
        /// Gets or sets the view matrix that is specific for this pass. If this
        /// value is null, the view matrix in the drawing context is used, otherwise
        /// this value will override the matrix currently in the drawing context.
        /// </summary>
        public Matrix? View { get; set; }

        /// <summary>
        /// Gets or sets the projection matrix that is specific for this pass. If this
        /// value is null, the projection matrix in the drawing context is used, otherwise
        /// this value will override the matrix currently in the drawing context.
        /// </summary>        
        public Matrix? Projection { get; set; }
        #endregion

        #region Field
        /// <summary>
        /// Keeps a reference to a render target in case some drawing passes put 
        /// the result onto an intermediate render target.
        /// This value is set to null when the drawing pass should draw everything
        /// onto the screen directly.
        /// </summary>
        private RenderTarget2D renderTarget;

        /// <summary>
        /// Id for this pass, used for dependency sorting.
        /// </summary>
        internal int Id;
        
        /// <summary>
        /// Keeps track of the parent container.
        /// </summary>
        internal DrawingPassCollection Container;

        /// <summary>
        /// Each drawing pass can have several dependent passes. All dependent 
        /// passes are drawn before this passes draws.
        /// </summary>
        internal FastList<DrawingPass> DependentPasses;

        /// <summary>
        /// Stores drawables that are queried from the current view frustum each frame.
        /// </summary>
        internal static FastList<IDrawableObject> DynamicDrawables = new FastList<IDrawableObject>();
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingPass"/> class.
        /// </summary>
        public DrawingPass()
        {
            this.Enabled = true;
        }

        /// <summary>
        /// Indicats this pass with be executed after the specified pass has been executed.
        /// </summary>
        public void AddDependency(DrawingPass pass)
        {
            if (DependentPasses == null)
                DependentPasses = new FastList<DrawingPass>();
            DependentPasses.Add(pass);
            if (Container != null)
                Container.TopologyChanged = true;
        }

        /// <summary>
        /// Prepares a preferred render target to hold the result of this drawing pass.
        /// </summary>
        /// <returns>
        /// Returns null to use the default render target settings copied from the backbuffer.
        /// </returns>
        public virtual RenderTarget2D PrepareRenderTarget(Texture2D input)
        {
            return null;
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        /// <param name="drawables">
        /// A list of drawables about to be drawed in this drawing pass.
        /// </param>
        public abstract void Draw(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length);
        #endregion
    }
}