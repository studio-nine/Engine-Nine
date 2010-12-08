#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
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
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Vertex format for shader vertex format used all over the place.
    /// It contains: Position, Normal vector, 2 texture coords
    /// </summary>
#if WINDOWS
    [Serializable()]
#endif
    public struct VertexPositionNormalDualTexture : IVertexType
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
        /// Texture coordinates
        /// </summary>
        public Vector2 TextureCoordinate0;
        /// <summary>
        /// Tangent
        /// </summary>
        public Vector2 TextureCoordinate1;

        /// <summary>
        /// Stride size, in XNA called SizeInBytes. I'm just conforming with that.
        /// </summary>
        public static int SizeInBytes
        {
            // 4 bytes per float:
            // 3 floats pos, 4 floats uv, 3 floats normal.
            get { return 4 * (3 + 4 + 3); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create tangent vertex
        /// </summary>
        /// <param name="setPos">Set position</param>
        /// <param name="setUv">Set uv texture coordinates</param>
        /// <param name="setNormal">Set normal</param>
        /// <param name="setTangent">Set tangent</param>
        public VertexPositionNormalDualTexture(
            Vector3 position,
            Vector3 normal,
            Vector2 uv0,
            Vector2 uv1)
        {
            Position = position;
            TextureCoordinate0 = uv0;
            TextureCoordinate1 = uv1;
            Normal = normal;
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
                    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                    new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                    new VertexElement(32, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
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
