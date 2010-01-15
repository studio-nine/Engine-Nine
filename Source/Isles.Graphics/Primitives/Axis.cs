#region File Description
//-----------------------------------------------------------------------------
// GeometricPrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Graphics.Primitives
{
    public class Axis : IDisposable
    {
        public Arrow Arrow { get; private set; }

        public Color XAxisColor { get; set; }
        public Color YAxisColor { get; set; }
        public Color ZAxisColor { get; set; }
        

        #region Initialization
        public Axis(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, 0.25f, 0.25f, 0.75f, 0.1f, 8)
        {
        }


        public Axis(GraphicsDevice graphicsDevice,
                                 float capHeight, float capDiameter,
                                 float bodyHeight, float bodyDiameter, int tessellation)
        {
            XAxisColor = Color.Red;
            YAxisColor = Color.Green;
            ZAxisColor = Color.Blue;

            Arrow = new Arrow(graphicsDevice, capHeight, capDiameter, bodyHeight, bodyDiameter, tessellation);
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Axis()
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
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Arrow != null)
                    Arrow.Dispose();
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the primitive model, using a BasicEffect shader with default
        /// lighting. Unlike the other Draw overload where you specify a custom
        /// effect, this method sets important renderstates to sensible values
        /// for 3D model rendering, so you do not need to set these states before
        /// you call it.
        /// </summary>
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            Arrow.Draw(world, view, projection, YAxisColor);

            Arrow.Draw(Matrix.CreateRotationZ(-(float)(Math.PI * 0.5f)) * world, view, projection, XAxisColor);

            Arrow.Draw(Matrix.CreateRotationX((float)(Math.PI * 0.5f)) * world, view, projection, ZAxisColor);
        }


        #endregion
    }
}
