#region File Description
//-----------------------------------------------------------------------------
// SpherePrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace Nine.Graphics.Primitives
{
    /// <summary>
    /// Geometric primitive class for drawing cross.
    /// </summary>
    [ContentSerializable]
    public class Cross : Primitive<VertexPositionNormalTexture>
    {
        /// <summary>
        /// Gets or sets the tessellation of this primitive.
        /// </summary>
        public int Tessellation
        {
            get { return tessellation; }
            set
            {
                if (tessellation != value)
                {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException("Tessellation");
                    tessellation = value;
                    Invalidate();
                }
            }
        }
        private int tessellation = 1;

        /// <summary>
        /// Constructs a new cross primitive, using default settings.
        /// </summary>
        public Cross(GraphicsDevice graphicsDevice) : base(graphicsDevice) 
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormalTexture> primitive)
        {
            return primitive is Cross && ((Cross)primitive).tessellation == tessellation;
        }
        
        protected override void OnBuild()
        {

        }

        private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            AddVertex(position, new VertexPositionNormalTexture() { Position = position, Normal = normal, TextureCoordinate = uv });
        }
    }
}
