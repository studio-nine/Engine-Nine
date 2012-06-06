#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Base class for all drawables.
    /// </summary>
    public abstract class Drawable : Transformable, IDrawableObject, IDisposable
    {
        /// <summary>
        /// Gets or sets whether the drawable is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
        
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
        /// Draws the object using the graphics context.
        /// </summary>
        /// <param name="context"></param>
        public virtual void Draw(DrawingContext context) { }

        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        public abstract void Draw(DrawingContext context, Material material);

        /// <summary>
        /// Draws the object with the specified effect.
        /// </summary>
        public virtual void DrawEffect(DrawingContext context, Effect effect) { }

        /// <summary>
        /// Perform any updates before this object is rendered.
        /// </summary>
        /// <param name="context"></param>
        public virtual void BeginDraw(DrawingContext context) { }

        /// <summary>
        /// Perform any updates after this object is rendered.
        /// </summary>
        /// <param name="context"></param>
        public virtual void EndDraw(DrawingContext context) { }
        
        #region IDisposable
        /// <summary>
        /// Disposes any resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) 
        {
            IsDisposed = true;
        }

        ~Drawable()
        {
            Dispose(false);
            IsDisposed = true;
        }
        #endregion
    }
}