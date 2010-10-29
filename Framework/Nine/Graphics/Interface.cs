#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Interface for game camera
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// Gets the camera view matrix
        /// </summary>
        Matrix View { get; }

        /// <summary>
        /// Gets the camera projection matrix
        /// </summary>
        Matrix Projection { get; }
    }

    /// <summary>
    /// Gets or sets skinning parameters for the current effect.
    /// </summary>
    public interface IEffectSkinned
    {
        /// <summary>
        /// Gets or sets if vertex skinning is enabled by th effect.
        /// </summary>
        bool SkinningEnabled { set; }

        /// <summary>
        /// Sets the bones transforms for the skinned effect.
        /// </summary>
        void SetBoneTransforms(Matrix[] boneTransforms);
    }

    /// <summary>
    /// Gets or sets material parameters for the current effect.
    /// </summary>
    public interface IEffectMaterial
    {
        /// <summary>
        /// Gets or sets the diffuse color of the effect.
        /// </summary>
        Vector3 DiffuseColor { get; set; }

        /// <summary>
        /// Gets or sets the emissive color of the effect.
        /// </summary>
        Vector3 EmissiveColor { get; set; }

        /// <summary>
        /// Gets or sets the specular color of the effect.
        /// </summary>
        Vector3 SpecularColor { get; set; }

        /// <summary>
        /// Gets or sets the specular power of the effect.
        /// </summary>
        float SpecularPower { get; set; }
    }

    /// <summary>
    /// Gets or sets texture parameters for the current effect.
    /// </summary>
    public interface IEffectTexture
    {
        /// <summary>
        /// Gets whether texture is enabled for the current effect.
        /// </summary>
        bool TextureEnabled { get; }
        
        /// <summary>
        /// Gets or sets the primiary diffuse texture of the current effect.
        /// </summary>
        Texture2D Texture { get; set; }

        /// <summary>
        /// Sets the texture with the specified key.
        /// </summary>
        void SetTexture(string name, Texture texture);
    }
}