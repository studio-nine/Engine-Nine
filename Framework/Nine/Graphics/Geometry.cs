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
        [ContentSerializer]
        public BoundingSphere BoundingSphere { get; private set; }

        [ContentSerializer]
        public BoundingBox BoundingBox { get; private set; }

        [ContentSerializer]
        public Vector3[] Positions { get; private set; }
        
        [ContentSerializer]
        public ushort[] Indices { get; private set; }


        IList<Vector3> IGeometry.Positions { get { return Positions; } }
        IList<ushort> IGeometry.Indices { get { return Indices; } }


        public Geometry(ICollection<Vector3> positions, ICollection<ushort> indices)
        {
            Positions = new Vector3[positions.Count];
            Indices = new ushort[indices.Count];

            positions.CopyTo(Positions, 0);
            indices.CopyTo(Indices, 0);

            BoundingBox =  BoundingBox.CreateFromPoints(Positions);
            BoundingSphere = BoundingSphere.CreateFromPoints(Positions);
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
