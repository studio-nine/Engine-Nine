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


namespace Isles.Graphics
{
    public class Geometry : IGeometry, ISurface
    {
        [ContentSerializer]
        public BoundingSphere BoundingSphere { get; private set; }

        [ContentSerializer]
        public BoundingBox BoundingBox { get; private set; }

        [ContentSerializer]
        public List<Vector3> Positions { get; private set; }
        
        [ContentSerializer]
        public List<ushort> Indices { get; private set; }


        IList<Vector3> IGeometry.Positions { get { return Positions.AsReadOnly(); } }
        IList<ushort> IGeometry.Indices { get { return Indices.AsReadOnly(); } }


        public Geometry(IList<Vector3> positions, IList<ushort> indices)
        {
            Positions = new List<Vector3>(positions);
            Indices = new List<ushort>(indices);

            BoundingBox =  BoundingBox.CreateFromPoints(Positions);
            BoundingSphere = BoundingSphere.CreateFromPoints(Positions);
        }


        /// <summary>
        /// Used by XNA content serialzier
        /// </summary>
        internal Geometry() { }


        public bool TryGetHeightAndNormal(Vector3 position, ref float height, ref Vector3 normal)
        {
            throw new NotImplementedException();
        }
    }
}
