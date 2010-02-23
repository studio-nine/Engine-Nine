#region File Description
//-----------------------------------------------------------------------------
// BloomComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Graphics.Filters
{
    /// <summary>
    /// Converts a heightmap to the corresponding normal map.
    /// </summary>
    public sealed class HeightmapFilter : Filter
    {
        private Effect effect;


        protected override void LoadContent()
        {
            effect = InternalContents.HeightmapEffect(GraphicsDevice);
        }

        protected override void Begin(Texture2D input, RenderTarget2D renderTarget)
        {
            Vector2 pixelSize;

            pixelSize.X = 1.0f / input.Width;
            pixelSize.Y = 1.0f / input.Height;

            effect.Parameters["PixelSize"].SetValue(pixelSize);
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
        }

        protected override void End()
        {
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
}

