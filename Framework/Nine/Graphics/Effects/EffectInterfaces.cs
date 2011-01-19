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
    
    public interface IEffectAmbientLight
    {
        /// <summary>
        /// Gets or sets the ambient light color.
        /// </summary>
        Vector3 AmbientLightColor { get; set; }
    }
    
    public interface IEffectDirectionalLight
    {
        Vector3 Direction { get; set; }
        Vector3 DiffuseColor { get; set; }
        Vector3 SpecularColor { get; set; }
    }

    public interface IEffectPointLight
    {
        Vector3 Position { get; set; }
        Vector3 DiffuseColor { get; set; }
        Vector3 SpecularColor { get; set; }
        float Range { get; set; }
        float Attenuation { get; set; }
    }

    public interface IEffectSpotLight
    {
        Vector3 Position { get; set; }
        Vector3 Direction { get; set; }
        Vector3 DiffuseColor { get; set; }
        Vector3 SpecularColor { get; set; }

        float Range { get; set; }
        float Attenuation { get; set; }
        float InnerAngle { get; set; }
        float OuterAngle { get; set; }
        float Falloff { get; set; }
    }

    public interface IEffectShadowMap
    {
        float DepthBias { get; set; }
        Matrix LightProjection { get; set; }
        Matrix LightView { get; set; }
        Vector3 ShadowColor { get; set; }
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