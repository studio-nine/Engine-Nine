#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Base class for all drawables.
    /// </summary>
    public abstract class Drawable : Transformable, IUpdateable, IDisposable
    {
        /// <summary>
        /// Gets or sets whether the drawable is visible.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public bool Visible { get; set; }

        /// <summary>
        /// Gets the passes that is required to render this drawable.
        /// </summary>
        [ContentSerializerIgnore]
        public NotificationCollection<Type> Passes { get; internal set; }
        
        /// <summary>
        /// Used by the rendering system to keep track of lights affecting this drawable.
        /// </summary>
        internal List<Light> AffectingLights;
        internal List<Light> MultiPassLights;

        /// <summary>
        /// Initializes a new instance of <c>Drawable</c>.
        /// </summary>
        public Drawable()
        {
            Visible = true;
            Passes = new NotificationCollection<Type>();
        }

        /// <summary>
        /// Updates the internal states of this drawable.
        /// </summary>
        public virtual void Update(TimeSpan elapsedTime)
        {

        }

        /// <summary>
        /// Draws the object using the graphics context.
        /// </summary>
        /// <param name="context"></param>
        public abstract void Draw(GraphicsContext context);

        /// <summary>
        /// Draws the object with the specified effect.
        /// </summary>
        public virtual void Draw(GraphicsContext context, Effect effect)
        {

        }

        /// <summary>
        /// Draws the object with the specified pass.
        /// </summary>
        public virtual void Draw(GraphicsContext context, GraphicsPass pass)
        {

        }
        
        #region IDisposable
        /// <summary>
        /// Disposes any resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) 
        {

        }

        ~Drawable()
        {
            Dispose(false);
        }
        #endregion
    }
}