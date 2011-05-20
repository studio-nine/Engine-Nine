#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ScreenEffects;
#endregion

namespace Nine.Graphics.Effects.Deferred
{
    class Quad : IDisposable
    {
        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;

        public int VertexCount { get { return 4; } }
        public int PrimitiveCount { get { return 2; } }

        public Quad(GraphicsDevice graphics)
        {
            VertexBuffer = new VertexBuffer(graphics, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            IndexBuffer = new IndexBuffer(graphics, typeof(ushort), 6, BufferUsage.WriteOnly);

            VertexPositionTexture[] vertices = new VertexPositionTexture[]
            {
                new VertexPositionTexture() { Position = new Vector3(-1, 1, 0), TextureCoordinate = new Vector2(0, 0) },
                new VertexPositionTexture() { Position = new Vector3(1, 1, 0), TextureCoordinate = new Vector2(1, 0) },
                new VertexPositionTexture() { Position = new Vector3(1, -1, 0), TextureCoordinate = new Vector2(1, 1) },
                new VertexPositionTexture() { Position = new Vector3(-1, -1, 0), TextureCoordinate = new Vector2(0, 1) },
            };

            ushort[] indices = new ushort[6] { 0, 1, 2, 0, 2, 3 };

            VertexBuffer.SetData(vertices);
            IndexBuffer.SetData<ushort>(indices);
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
