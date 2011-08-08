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

namespace Nine.Graphics.Primitives
{
    public class Quad : Primitive<VertexPositionNormalTexture>
    {
        public Quad(GraphicsDevice graphics)
        {
            AddVertex(new Vector3(-1, 1, 0), new VertexPositionNormalTexture() { Position = new Vector3(-1, 1, 0), Normal = Vector3.UnitZ, TextureCoordinate = new Vector2(0, 0) });
            AddVertex(new Vector3(1, 1, 0), new VertexPositionNormalTexture() { Position = new Vector3(1, 1, 0), Normal = Vector3.UnitZ, TextureCoordinate = new Vector2(1, 0) });
            AddVertex(new Vector3(1, -1, 0), new VertexPositionNormalTexture() { Position = new Vector3(1, -1, 0), Normal = Vector3.UnitZ, TextureCoordinate = new Vector2(1, 1) });
            AddVertex(new Vector3(-1, -1, 0), new VertexPositionNormalTexture() { Position = new Vector3(-1, -1, 0), Normal = Vector3.UnitZ, TextureCoordinate = new Vector2(0, 1) });

            AddIndex(0, 1, 2, 0, 2, 3);

            InitializePrimitive(graphics);
        }
    }
}
