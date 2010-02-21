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
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics
{
    internal static class GraphicsExtensions
    {
        public static void SetSpriteBlendMode(this RenderState renderState, SpriteBlendMode blend)
        {
            if (blend == SpriteBlendMode.None)
            {
                renderState.AlphaBlendEnable = false;
            }
            else if (blend == SpriteBlendMode.AlphaBlend)
            {
                renderState.AlphaBlendEnable = true;
                renderState.SourceBlend = Blend.SourceAlpha;
                renderState.DestinationBlend = Blend.InverseSourceAlpha;
            }
            else if (blend == SpriteBlendMode.Additive)
            {
                renderState.AlphaBlendEnable = true;
                renderState.SourceBlend = Blend.SourceAlpha;
                renderState.DestinationBlend = Blend.One;
            }
        }
    }
}
