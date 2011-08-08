#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ParticleEffects;
using Nine;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Represents a pass when drawing the objects.
    /// </summary>    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class GraphicsPass
    {
        /// <summary>
        /// Number of drawables that depends on this pass.
        /// </summary>
        internal int SubscriberCount = 0;

        /// <summary>
        /// Number of passes that depends on this pass.
        /// </summary>
        internal int ReferencingPassCount = 0;

        /// <summary>
        /// Gets a collection of type of<c>GraphicsPass</c> that this pass depend upon.
        /// </summary>
        public virtual IEnumerable<Type> Dependencies { get { return null; } }

        /// <summary>
        /// Gets a read-only collection of the instances of <c>GraphicsPass</c> that 
        /// correspond to the types in Dependencies.
        /// </summary>
        public ReadOnlyCollection<GraphicsPass> Passes { get; private set; }

        internal List<GraphicsPass> dependentPasses = new List<GraphicsPass>();

        /// <summary>
        /// Called when a drawable that requires this graphics pass is added.
        /// </summary>
        protected internal virtual void OnAdded(Drawable subscriber) { }

        /// <summary>
        /// Called when a drawable that requires this graphics pass is removed.
        /// </summary>
        protected internal virtual void OnRemoved(Drawable subscriber) { }

        public abstract void Draw(GraphicsContext context);

        protected GraphicsPass()
        {
            Passes = new ReadOnlyCollection<GraphicsPass>(dependentPasses);
        }
    }
}