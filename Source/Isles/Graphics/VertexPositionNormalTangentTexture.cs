#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
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

namespace Isles.Graphics
{
    /// <summary>
    /// Tangent vertex format for shader vertex format used all over the place.
    /// It contains: Position, Normal vector, texture coords, tangent vector.
    /// </summary>
    [Serializable]
    public struct VertexPositionNormalTangentTexture : IVertexType
    {
        // Grabbed from racing game :)

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
        /// Stride size, in XNA called SizeInBytes. I'm just conforming with that.
        /// </summary>
        public static int SizeInBytes
        {
            // 4 bytes per float:
            // 3 floats pos, 2 floats uv, 3 floats normal and 3 float tangent.
            get { return 4 * (3 + 2 + 3 + 3); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create tangent vertex
        /// </summary>
        /// <param name="setPos">Set position</param>
        /// <param name="setU">Set u texture coordinate</param>
        /// <param name="setV">Set v texture coordinate</param>
        /// <param name="setNormal">Set normal</param>
        /// <param name="setTangent">Set tangent</param>
        public VertexPositionNormalTangentTexture(
            Vector3 setPos,
            float setU, float setV,
            Vector3 setNormal,
            Vector3 setTangent)
        {
            Position = setPos;
            TextureCoordinate = new Vector2(setU, setV);
            Normal = setNormal;
            Tangent = setTangent;
        }

        /// <summary>
        /// Create tangent vertex
        /// </summary>
        /// <param name="setPos">Set position</param>
        /// <param name="setUv">Set uv texture coordinates</param>
        /// <param name="setNormal">Set normal</param>
        /// <param name="setTangent">Set tangent</param>
        public VertexPositionNormalTangentTexture(
            Vector3 setPos,
            Vector2 setUv,
            Vector3 setNormal,
            Vector3 setTangent)
        {
            Position = setPos;
            TextureCoordinate = setUv;
            Normal = setNormal;
            Tangent = setTangent;
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
