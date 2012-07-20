#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ObjectModel;
using DirectionalLight = Nine.Graphics.ObjectModel.DirectionalLight;
using Nine.Graphics.Drawing;
using System.ComponentModel;
#endregion

namespace Nine.Graphics.Materials
{
    partial class VertexPassThroughMaterial
    {
        partial void ApplyGlobalParameters(DrawingContext context)
        {
            var halfPixel = new Vector2();
            var vp = context.GraphicsDevice.Viewport;

            halfPixel.X = 0.5f / vp.Width;
            halfPixel.Y = 0.5f / vp.Height;

            effect.halfPixel.SetValue(halfPixel);
        }
    }
}