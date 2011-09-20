#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
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
    /// <summary>
    /// Defines a list of effect parameter annotation supported by the rendering system.
    /// </summary>
    enum EffectSemantics
    {
        StandardsGlobal,

        Position,
        Direction,
        Attenuation,

        DiffuseTexture,

        Diffuse,
        Specular,
        Ambient,
        Emissive,
        Specularpower,

        Refraction,
        Opacity,
        Environment,
        EnvironmentNormal,
        
        Normal,
        Height,

        RenderColorTarget,
        RenderDepthStencilTarget,

        ViewportPixelSize,
        CameraPosition,
        
        Time,
        ElapsedTime,

        MousePosition,
        LeftMouseDown,

        World,
        View,
        Projection,
        WorldTranspose,
        ViewTranspose,
        ProjectionTranspose,
        WorldView,
        WorldViewProjection,
        WorldInverse,
        ViewInverse,
        ProjectionInverse,
        WorldInverseTranspose,
        ViewInverseTranspose,
        ProjectionInverseTranspose,
        WorldViewInverse,
        WorldViewTranspose,
        WorldViewInverseTranspose,
        WorldViewProjectionInverse,
        WorldViewProjectionTranspose,
        WorldViewProjectionInverseTranspose,
        ViewProjection,
        ViewProjectionTranspose,
        ViewProjectionInverse,
        ViewProjectionInverseTranspose,
    }
}