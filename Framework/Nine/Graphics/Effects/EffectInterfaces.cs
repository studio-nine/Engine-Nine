#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Effects
{
#if !WINDOWS_PHONE

    public interface IEffectShadowMap
    {
        float DepthBias { get; set; }
        float ShadowIntensity { get; set; }
        Matrix LightProjection { get; set; }
        Matrix LightView { get; set; }
        Texture2D ShadowMap { get; set; }
    }

    public interface IEffectSplatterTexture
    {
        Texture2D TextureX { get; }
        Texture2D TextureY { get; }
        Texture2D TextureZ { get; }
        Texture2D TextureW { get; }
        Texture2D SplatterTexture { get; set; }
        Vector2 SplatterTextureScale { get; set; }
    }

    public interface IEffectTextureTransform
    {
        Matrix Transform { get; set; }
    }

#endif
}