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
    public class Canvas
    {
        private VertexPositionTexture[] vertices = new VertexPositionTexture[4];

        public List<GraphicsEffect> Layers { get; set; }
        public GraphicsDevice GraphicsDevice { get; private set; }


        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public Matrix Projection { get; private set; }


        public Canvas()
        {
            Projection = Matrix.Identity;
            Layers = new List<GraphicsEffect>();
        }

        public void Draw(GraphicsDevice graphics, GameTime time)
        {
            if (GraphicsDevice == null)
                GraphicsDevice = graphics;
        }
    }
}
