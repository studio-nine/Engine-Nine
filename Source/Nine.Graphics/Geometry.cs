namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Geometric representation of a model.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    public class Geometry : Transformable, IGeometry, ISurface
    {
        /// <summary>
        /// Gets the bounding box.
        /// </summary>
        [Nine.Serialization.BinarySerializable]
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
        
        /// <summary>
        /// Gets a read only list of vertex positions.
        /// </summary>
        public ReadOnlyCollection<Vector3> Positions 
        {
            get { return positionsCollection; } 
        }
        [Nine.Serialization.BinarySerializable]
        internal Vector3[] positions;
        private ReadOnlyCollection<Vector3> positionsCollection;

        /// <summary>
        /// Gets a read-only list of geometry indices.
        /// </summary>
        public ReadOnlyCollection<ushort> Indices
        {
            get { return indicesCollection; }
        }
        [Nine.Serialization.BinarySerializable]
        internal ushort[] indices;
        private ReadOnlyCollection<ushort> indicesCollection;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Geometry"/> class.
        /// </summary>
        internal Geometry() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Geometry"/> class.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public Geometry(IGeometry geometry) : this(GetPositions(geometry), GetIndices(geometry))
        {

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

            this.positions = new Vector3[positions.Count];
            this.positionsCollection = new ReadOnlyCollection<Vector3>(this.positions);
            positions.CopyTo(this.positions, 0);

            if (indices == null)
            {
                this.indices = new ushort[positions.Count];
                for (int i = 0; i < this.indices.Length; ++i)
                    this.indices[i] = (ushort)i;
            }
            else
            {
                this.indices = new ushort[indices.Count];
                indices.CopyTo(this.indices, 0);
            }
            this.indicesCollection = new ReadOnlyCollection<ushort>(this.indices);
        }

        private static ICollection<Vector3> GetPositions(IGeometry geometry)
        {
            Vector3[] positions;
            ushort[] indices;
            geometry.TryGetTriangles(out positions, out indices);
            return positions;
        }

        private static ICollection<ushort> GetIndices(IGeometry geometry)
        {
            Vector3[] positions;
            ushort[] indices;
            geometry.TryGetTriangles(out positions, out indices);
            return indices;
        }

        /// <summary>
        /// Gets the triangle vertices of the target geometry.
        /// </summary>
        public bool TryGetTriangles(out Vector3[] vertices, out ushort[] indices)
        {
            vertices = this.positions;
            indices = this.indices;
            return true;
        }
        
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            throw new NotImplementedException();
        }
    }
}
