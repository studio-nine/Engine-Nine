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
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Geometric representation of a model.
    /// </summary>
    public class Geometry : IGeometry, ISurface
    {
        /// <summary>
        /// Gets the bounding sphere.
        /// </summary>
        [ContentSerializer]
        public BoundingSphere BoundingSphere
        {
            get
            {
                if (boundingSphere == null)
                    boundingSphere = BoundingSphere.CreateFromPoints(Positions);
                return boundingSphere.Value;
            }
            internal set { boundingSphere = value; }
        }
        private BoundingSphere? boundingSphere;

        /// <summary>
        /// Gets the bounding box.
        /// </summary>
        [ContentSerializer]
        public BoundingBox BoundingBox
        {
            get
            {
                if (boundingBox == null)
                    boundingBox = BoundingBox.CreateFromPoints(Positions);
                return boundingBox.Value;
            }
            internal set { boundingBox = value; }
        }
        private BoundingBox? boundingBox;

        Matrix? IGeometry.Transform { get { return null; } }

        /// <summary>
        /// Gets a readonly list of vertex positions.
        /// </summary>
        [ContentSerializer]
        public Vector3[] Positions { get; internal set; }

        /// <summary>
        /// Gets a read-only list of geometry indices.
        /// </summary>
        [ContentSerializer]
        public ushort[] Indices { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Geometry"/> class.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public Geometry(IGeometry geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");

            Positions = geometry.Positions;
            Indices = geometry.Indices;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Geometry"/> class.
        /// </summary>
        /// <param name="positions">The positions.</param>
        /// <param name="indices">The indices.</param>
        public Geometry(ICollection<Vector3> positions, ICollection<ushort> indices)
        {
            if (positions == null)
                throw new ArgumentNullException("positions");

            Positions = new Vector3[positions.Count];
            positions.CopyTo(Positions, 0);

            if (indices == null)
            {
                Indices = new ushort[positions.Count];
                for (int i = 0; i < Indices.Length; i++)
                    Indices[i] = (ushort)i;
            }
            else
            {
                Indices = new ushort[indices.Count];
                indices.CopyTo(Indices, 0);
            }
        }


        /// <summary>
        /// Used by XNA content serialzier
        /// </summary>
        internal Geometry() { }
        
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            throw new NotImplementedException();
        }
    }
}
