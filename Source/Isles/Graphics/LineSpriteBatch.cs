#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
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
using Isles.Graphics.Vertices;
#endregion


namespace Isles.Graphics
{
    public sealed class LineSpriteBatch : IDisposable
    {
        #region Key
        internal struct Key
        {
            public Texture2D Texture;
            public EffectTechnique Technique;
        }
        #endregion


        private bool hasBegin = false;
        private VertexBuffer vertices;
        private VertexDeclaration declaration;

        public GraphicsDevice GraphicsDevice { get; private set; }
        public bool IsDisposed { get; private set; }
        
        
        public event EventHandler Disposing;


        public LineSpriteBatch(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
        }


        public void Begin() 
        {
            if (IsDisposed)
                throw new ObjectDisposedException("LineSpriteBatch");
        }


        public void Draw(Texture2D texture, Vector3 start, Vector3 end, float width, Color color)
        {

        }


        public void Draw(Texture2D texture, Vector3 start, Vector3 end, float width, Vector2 textureScale, Vector2 textureOffset, Rectangle? sourceRectangle, Color color)
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called before end and draw calls");
        }


        public void Draw(Texture2D texture, Vector3[] lineStrip, float width, Color color)
        {

        }


        public void Draw(Texture2D texture, Vector3[] lineStrip, float width, Vector2 textureScale, Vector2 textureOffset, Rectangle? sourceRectangle, Color color)
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called before end and draw calls");
        }


        public void End()
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called before end and draw calls");

            if (IsDisposed)
                throw new ObjectDisposedException("LineSpriteBatch");
        }


        public void Dispose()
        {
            if (vertices != null)
                vertices.Dispose();

            if (declaration != null)
                declaration.Dispose();

            IsDisposed = true;

            if (Disposing != null)
                Disposing(this, EventArgs.Empty);
        }
    }
}
