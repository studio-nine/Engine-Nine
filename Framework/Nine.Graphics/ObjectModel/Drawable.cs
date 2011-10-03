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
using Nine.Graphics.Effects;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Base class for all drawables.
    /// </summary>
    public abstract class Drawable : Transformable, IDrawableObject, IUpdateable, IDisposable
    {
        /// <summary>
        /// Gets or sets whether the drawable is visible.
        /// </summary>
        [ContentSerializer]
        public bool Visible { get; set; }
        
        /// <summary>
        /// Gets the material used by this drawable.
        /// </summary>
        public Material Material { get { return MaterialValue; } }

        /// <summary>
        /// When overriden, returns the material used by this drawable.
        /// </summary>
        protected virtual Material MaterialValue { get { return null; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Drawable"/> class.
        /// </summary>
        public Drawable()
        {
            Visible = true;
        }

        /// <summary>
        /// Updates the internal states of this drawable.
        /// </summary>
        public virtual void Update(TimeSpan elapsedTime) { }

        /// <summary>
        /// Draws the object using the graphics context.
        /// </summary>
        /// <param name="context"></param>
        public abstract void Draw(GraphicsContext context);

        /// <summary>
        /// Draws the object with the specified effect.
        /// </summary>
        public virtual void Draw(GraphicsContext context, Effect effect) { }

        /// <summary>
        /// Perform any updates before this object is rendered.
        /// </summary>
        /// <param name="context"></param>
        public virtual void BeginDraw(GraphicsContext context) { }

        /// <summary>
        /// Perform any updates after this object is rendered.
        /// </summary>
        /// <param name="context"></param>
        public virtual void EndDraw(GraphicsContext context) { }
        
        #region IDisposable
        /// <summary>
        /// Disposes any resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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