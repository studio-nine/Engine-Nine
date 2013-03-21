namespace Nine.Graphics
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Vertex format for shader vertex format used all over the place.
    /// It contains: Position, Normal vector
    /// </summary>
#if WINDOWS
    [Serializable()]
#endif
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct VertexPositionNormal : IVertexType
    {
        #region Variables
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Stride size.
        /// </summary>
        public static int SizeInBytes
        {
            // 4 bytes per float:
            // 3 floats pos, 2 floats uv, 3 floats normal and 3 float tangent.
            get { return 4 * (3 + 3); }
        }
        #endregion

        #region Generate vertex declaration
        /// <summary>
        /// Vertex elements for Mesh.Clone
        /// </summary>
        public static readonly VertexElement[] VertexElements = new VertexElement[]
        {
            // Construct new vertex declaration with tangent info
            // First the normal stuff (we should already have that)
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        };
        #endregion

        public VertexDeclaration VertexDeclaration
        {
            get { return new Microsoft.Xna.Framework.Graphics.VertexDeclaration(VertexElements); }
        }
    }
}
