#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Tangent vertex format for shader vertex format used all over the place.
    /// It contains: Position, Normal vector, texture coords, tangent vector.
    /// </summary>
#if WINDOWS
    [Serializable()]
#endif
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct VertexPositionNormalTangentBinormalTexture : IVertexType
    {
        #region Variables
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Texture coordinates
        /// </summary>
        public Vector2 TextureCoordinate;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal;
        /// <summary>
        /// Tangent
        /// </summary>
        public Vector3 Tangent;
        /// <summary>
        /// Binormal
        /// </summary>
        public Vector3 Binormal;

        /// <summary>
        /// Stride size, in XNA called SizeInBytes. I'm just conforming with that.
        /// </summary>
        public static int SizeInBytes
        {
            // 4 bytes per float:
            // 3 floats pos, 2 floats uv, 3 floats normal and 3 float tangent.
            get { return 4 * (3 + 2 + 3 + 3 + 3); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create tangent vertex
        /// </summary>
        public VertexPositionNormalTangentBinormalTexture(
            Vector3 setPos,
            float setU, float setV,
            Vector3 setNormal,
            Vector3 setTangent,
            Vector3 setBinormal)
        {
            Position = setPos;
            TextureCoordinate = new Vector2(setU, setV);
            Normal = setNormal;
            Tangent = setTangent;
            Binormal = setBinormal;
        }

        /// <summary>
        /// Create tangent vertex
        /// </summary>
        public VertexPositionNormalTangentBinormalTexture(
            Vector3 setPos,
            Vector2 setUv,
            Vector3 setNormal,
            Vector3 setTangent,
            Vector3 setBinormal)
        {
            Position = setPos;
            TextureCoordinate = setUv;
            Normal = setNormal;
            Tangent = setTangent;
            Binormal = setBinormal;
        }
        #endregion

        #region Generate vertex declaration
        /// <summary>
        /// Vertex elements for Mesh.Clone
        /// </summary>
        public static readonly VertexElement[] VertexElements =
            GenerateVertexElements();

        /// <summary>
        /// Generate vertex declaration
        /// </summary>
        private static VertexElement[] GenerateVertexElements()
        {
            VertexElement[] decl = new VertexElement[]
                {
                    // Construct new vertex declaration with tangent info
                    // First the normal stuff (we should already have that)
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                    new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                    // And now the tangent
                    new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
                    new VertexElement(44, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
                };
            return decl;
        }
        #endregion

        public VertexDeclaration VertexDeclaration
        {
            get { return new VertexDeclaration(VertexElements); }
        }
    }
}
