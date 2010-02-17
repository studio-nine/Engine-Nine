#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics
{
    public interface IModelEffect
    {
        Matrix[] Bones { get; set; }
        
        bool SkinningEnabled { get; set; }
    }


    public abstract class GraphicsEffect : IDisposable
    {
        [ContentSerializerIgnore]
        public Matrix World { get; set; }

        [ContentSerializerIgnore]
        public Matrix View { get; set; }

        [ContentSerializerIgnore]
        public Matrix Projection { get; set; }

        [ContentSerializerIgnore]
        public Texture Texture { get; set; }

        public GraphicsDevice GraphicsDevice { get; private set; }


        public bool IsDisposed { get; private set; }


        public event EventHandler Disposing;


        public GraphicsEffect() 
        {
            World = Matrix.Identity;
            View = Matrix.Identity;
            Projection = Matrix.Identity;
        }

        public bool Begin(GraphicsDevice graphics, GameTime time)
        {
            if (graphics == null)
                throw new ArgumentNullException();

            if (GraphicsDevice == null)
            {
                GraphicsDevice = graphics;

                LoadContent();
            }

            return Begin(time);
        }


        protected abstract void LoadContent();
        protected abstract bool Begin(GameTime time);
        public abstract void End();


        #region IDisposable

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~GraphicsEffect()
        {
            Dispose(false);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            IsDisposed = true;

            if (Disposing != null)
                Disposing(this, EventArgs.Empty);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
        }

        #endregion
    }
}
