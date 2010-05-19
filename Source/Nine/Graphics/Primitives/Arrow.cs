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

namespace Nine.Graphics.Primitives
{
    internal class Arrow : IDisposable, IGeometry
    {
        Cylinder body;
        Centrum cap;

        float bodyHeight;
        float capHeight;


        #region Initialization
        public Arrow(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, 0.25f, 0.25f, 0.75f, 0.1f, 8)
        {
        }


        public Arrow(GraphicsDevice graphicsDevice,
                                 float capHeight, float capDiameter,
                                 float bodyHeight, float bodyDiameter, int tessellation)
        {
            this.capHeight = capHeight;
            this.bodyHeight = bodyHeight;

            cap = new Centrum(graphicsDevice, capHeight, capDiameter, tessellation);
            body = new Cylinder(graphicsDevice, bodyHeight, bodyDiameter, tessellation);
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Arrow()
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
                if (cap != null)
                    cap.Dispose();

                if (cap != null)
                    cap.Dispose();
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
        public void Draw(Matrix world, Matrix view, Matrix projection, Color color)
        {
            cap.Draw(Matrix.CreateTranslation(0, bodyHeight + capHeight / 2, 0) * world, view, projection, color);
            body.Draw(Matrix.CreateTranslation(0, bodyHeight / 2, 0) * world, view, projection, color);
        }


        #endregion

        #region IGeometry Members

        public IList<Vector3> Positions
        {
            get 
            {
                // We only return the body
                return (body as IGeometry).Positions;
            }
        }

        public IList<ushort> Indices
        {
            get
            {
                return (body as IGeometry).Indices;
            }
        }

        #endregion
    }
}
