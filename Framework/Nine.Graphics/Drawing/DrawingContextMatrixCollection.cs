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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.ObjectModel;
using System.Collections.Generic;
#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// Contains commonly used matrices in a drawing context.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DrawingContextMatrixCollection
    {
        internal DrawingContextMatrixCollection() { }

        private const int ViewFrustumDirty = 1 << 0;
        private const int ViewProjectionDirty = 1 << 1;
        private const int ViewTransposeDirty = 1 << 3;
        private const int ViewInverseTransposeDirty = 1 << 4;
        private const int ProjectionInverseDirty = 1 << 5;
        private const int ProjectionTransposeDirty = 1 << 6;
        private const int ProjectionInverseTransposeDirty = 1 << 7;

        private int matricesDirtyMask = int.MaxValue;

        /// <summary>
        /// Gets the view matrix for this drawing operation.
        /// </summary>
        internal Matrix View
        {
            get { return view; }
            set
            {
                view = value;
                matricesDirtyMask |= (ViewFrustumDirty | ViewProjectionDirty);
                Matrix.Invert(ref view, out viewInverse);
                eyePosition.X = viewInverse.M41;
                eyePosition.Y = viewInverse.M42;
                eyePosition.Z = viewInverse.M43;
            }
        }
        internal Matrix view;

        /// <summary>
        /// Gets the projection matrix for this drawing operation.
        /// </summary>
        internal Matrix Projection
        {
            get { return projection; }
            set
            {
                projection = value;
                matricesDirtyMask |= (ViewFrustumDirty | ViewProjectionDirty);
            }
        }
        internal Matrix projection;

        /// <summary>
        /// Gets the view projection matrix.
        /// </summary>
        public Matrix ViewProjection
        {
            get
            {
                if ((matricesDirtyMask & ViewProjectionDirty) != 0)
                {
                    Matrix.Multiply(ref view, ref projection, out viewProjection);
                    matricesDirtyMask |= ~ViewProjectionDirty;
                }
                return viewProjection;
            }
        }
        private Matrix viewProjection;

        /// <summary>
        /// Gets the inverse view matrix.
        /// </summary>
        public Matrix ViewInverse
        {
            get { return viewInverse; }
        }
        internal Matrix viewInverse;
        internal Vector3 eyePosition;

        /// <summary>
        /// Gets the transposed view matrix.
        /// </summary>
        public Matrix ViewTranspose
        {
            get
            {
                if ((matricesDirtyMask & ViewTransposeDirty) != 0)
                {
                    Matrix.Transpose(ref view, out viewTranspose);
                    matricesDirtyMask |= ~ViewTransposeDirty;
                }
                return viewTranspose;
            }
        }
        private Matrix viewTranspose;

        /// <summary>
        /// Gets the inverse transpose view matrix.
        /// </summary>
        public Matrix ViewInverseTranspose
        {
            get
            {
                if ((matricesDirtyMask & ViewInverseTransposeDirty) != 0)
                {
                    Matrix.Invert(ref view, out viewInverseTranspose);
                    Matrix.Transpose(ref viewInverseTranspose, out viewInverseTranspose);
                    matricesDirtyMask |= ~ViewInverseTransposeDirty;
                }
                return viewInverseTranspose;
            }
        }
        private Matrix viewInverseTranspose;
        
        /// <summary>
        /// Gets the inverse Projection matrix.
        /// </summary>
        public Matrix ProjectionInverse
        {
            get
            {
                if ((matricesDirtyMask & ProjectionInverseDirty) != 0)
                {
                    Matrix.Invert(ref projection, out projectionInverse);
                    matricesDirtyMask |= ~ProjectionInverseDirty;
                }
                return projectionInverse;
            }
        }
        private Matrix projectionInverse;

        /// <summary>
        /// Gets the transposed Projection matrix.
        /// </summary>
        public Matrix ProjectionTranspose
        {
            get
            {
                if ((matricesDirtyMask & ProjectionTransposeDirty) != 0)
                {
                    Matrix.Transpose(ref projection, out projectionTranspose);
                    matricesDirtyMask |= ~ProjectionTransposeDirty;
                }
                return projectionTranspose;
            }
        }
        private Matrix projectionTranspose;

        /// <summary>
        /// Gets the inverse transpose Projection matrix.
        /// </summary>
        public Matrix ProjectionInverseTranspose
        {
            get
            {
                if ((matricesDirtyMask & ProjectionInverseTransposeDirty) != 0)
                {
                    Matrix.Invert(ref projection, out projectionInverseTranspose);
                    Matrix.Transpose(ref projectionInverseTranspose, out projectionInverseTranspose);
                    matricesDirtyMask |= ~ProjectionInverseTransposeDirty;
                }
                return projectionInverseTranspose;
            }
        }
        private Matrix projectionInverseTranspose;

        /// <summary>
        /// Gets the view frustum for this drawing operation.
        /// </summary>
        internal BoundingFrustum ViewFrustum
        {
            get
            {
                if ((matricesDirtyMask & ViewFrustumDirty) != 0)
                {
                    frustum.Matrix = ViewProjection;
                    matricesDirtyMask |= ~ViewFrustumDirty;
                }
                return frustum;
            }
        }
        private BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);
    }
}