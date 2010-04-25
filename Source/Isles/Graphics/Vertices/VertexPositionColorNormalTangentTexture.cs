#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics.Vertices
{
    [Serializable]
    public struct VertexPositionColorNormalTangentTexture
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
        /// Color
        /// </summary>
        public Color Color;

        /// <summary>
        /// Stride size.
        /// </summary>
        public static int SizeInBytes
        {
            // 4 bytes per float:
            // 3 floats pos, 2 floats uv, 3 floats normal and 3 float tangent.
            get { return 4 * (3 + 2 + 3 + 3 + 1); }
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
            new VertexElement(0, 0, VertexElementFormat.Vector3,
                VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0, 12, VertexElementFormat.Vector2,
                VertexElementMethod.Default,
                VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(0, 20, VertexElementFormat.Vector3,
                VertexElementMethod.Default, VertexElementUsage.Normal, 0),
            new VertexElement(0, 32, VertexElementFormat.Vector3,
                VertexElementMethod.Default, VertexElementUsage.Tangent, 0),
            new VertexElement(0, 44, VertexElementFormat.Color,
                VertexElementMethod.Default, VertexElementUsage.Color, 0),
        };
        #endregion
    }
}
