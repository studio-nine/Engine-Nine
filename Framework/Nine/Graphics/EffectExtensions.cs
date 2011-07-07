#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
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
using Nine.Graphics.Effects;
using System.Collections.ObjectModel;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Gets or sets skinning parameters for the current effect.
    /// </summary>
    public interface IEffectSkinned
    {
        /// <summary>
        /// Gets or sets if vertex skinning is enabled by this effect.
        /// </summary>
        bool SkinningEnabled { get; set; }

        /// <summary>
        /// Sets the bones transforms for the skinned effect.
        /// </summary>
        void SetBoneTransforms(Matrix[] boneTransforms);
    }

    /// <summary>
    /// Gets or sets lighting parameters for the current effect.
    /// </summary>
    public interface IEffectLights<T>
    {
        /// <summary>
        /// Gets a read only collection of lights exposed by this effect.
        /// </summary>
        ReadOnlyCollection<T> Lights { get; }
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
    /// Contains the names of commonly used texture types.
    /// </summary>
    public static class TextureNames
    {
        /// <summary>
        /// The texture type name used for diffuse texture.
        /// </summary>
        public static readonly string Diffuse = "Diffuse";

        /// <summary>
        /// The texture type name used for ambient texture.
        /// </summary>
        public static readonly string Ambient = "Ambient";

        /// <summary>
        /// The texture type name used for emissive texture.
        /// </summary>
        public static readonly string Emissive = "Emissive";

        /// <summary>
        /// The texture type name used for specular texture.
        /// </summary>
        public static readonly string Specular = "Specular";

        /// <summary>
        /// The texture type name used for detail texture.
        /// </summary>
        public static readonly string Detail = "Refraction";

        /// <summary>
        /// The texture type name used for overlay texture.
        /// </summary>
        public static readonly string Overlay = "Overlay";

        /// <summary>
        /// The texture type name used for dual texture.
        /// </summary>
        public static readonly string Dual = "Dual";

        /// <summary>
        /// The texture type name used for reflection texture.
        /// </summary>
        public static readonly string Reflection = "Reflection";

        /// <summary>
        /// The texture type name used for refraction texture.
        /// </summary>
        public static readonly string Refraction = "Refraction";

        /// <summary>
        /// The texture type name used for lightmap texture.
        /// </summary>
        public static readonly string Lightmap = "Lightmap";

        /// <summary>
        /// The texture type name used for luminance texture.
        /// </summary>
        public static readonly string Luminance = "Luminance";

        /// <summary>
        /// The texture type name used for bloom texture.
        /// </summary>
        public static readonly string Bloom = "Bloom";

        /// <summary>
        /// The texture type name used for blur texture.
        /// </summary>
        public static readonly string Blur = "Blur";

        /// <summary>
        /// The texture type name used for shadowmap texture.
        /// </summary>
        public static readonly string ShadowMap = "ShadowMap";

        /// <summary>
        /// The texture type name used for normalmap texture.
        /// </summary>
        public static readonly string NormalMap = "NormalMap";

        /// <summary>
        /// The texture type name used for bump texture.
        /// </summary>
        public static readonly string BumpMap = "BumpMap";

        /// <summary>
        /// The texture type name used for DepthMap texture.
        /// </summary>
        public static readonly string DepthMap = "DepthMap";

        /// <summary>
        /// The texture type name used for environment texture.
        /// </summary>
        public static readonly string EnvironmentMap = "EnvironmentMap";

        /// <summary>
        /// The texture type name used for light buffer texture.
        /// </summary>
        public static readonly string LightBuffer = "LightBuffer";
    }

    /// <summary>
    /// Gets or sets texture parameters for the current effect.
    /// </summary>
    public interface IEffectTexture
    {
        /// <summary>
        /// Gets or sets the primiary diffuse texture of the current effect.
        /// </summary>
        Texture2D Texture { get; set; }

        /// <summary>
        /// Sets the texture with the specified texture type name.
        /// See <c>TextureNames</c> for a list of predefined names.
        /// </summary>
        void SetTexture(string name, Texture texture);
    }

    
    /// <summary>
    /// Contains extension methods for Effects.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class EffectExtensions
    {
        internal static T As<T>(this Effect effect) where T : class
        {
#if WINDOWS_PHONE
            return effect as T;
#else
            LinkedEffect linked;
            T result = effect as T;
            if (result != null)
                return result;
            linked = effect as LinkedEffect;
            if (linked != null)
                result = linked.Find<T>();
            return result;
#endif
        }

        internal static void CopyMaterialsFrom(this Effect effect, Effect sourceEffect)
        {
            if (effect == null || sourceEffect == null)
                return;

            Vector3 DiffuseColor = Vector3.One;
            Vector3 SpecularColor = Vector3.Zero;
            Vector3 EmissiveColor = Vector3.Zero;
            float SpecularPower = 0;

            Texture2D Texture = sourceEffect.GetTexture();
            if (Texture != null)
                effect.SetTexture(Texture);

            // Extract from source
            if (sourceEffect is IEffectMaterial)
            {
                IEffectMaterial source = sourceEffect as IEffectMaterial;
                DiffuseColor = source.DiffuseColor;
                EmissiveColor = source.EmissiveColor;
                SpecularColor = source.SpecularColor;
                SpecularPower = source.SpecularPower;
            }
            else if (sourceEffect is BasicEffect)
            {
                BasicEffect source = sourceEffect as BasicEffect;
                DiffuseColor = source.DiffuseColor;
                EmissiveColor = source.EmissiveColor;
                SpecularColor = source.SpecularColor;
                SpecularPower = source.SpecularPower;
            }
            else if (sourceEffect is SkinnedEffect)
            {
                SkinnedEffect source = sourceEffect as SkinnedEffect;
                DiffuseColor = source.DiffuseColor;
                EmissiveColor = source.EmissiveColor;
                SpecularColor = source.SpecularColor;
                SpecularPower = source.SpecularPower;
            }
            else if (sourceEffect is EnvironmentMapEffect)
            {
                EnvironmentMapEffect source = sourceEffect as EnvironmentMapEffect;
                DiffuseColor = source.DiffuseColor;
                EmissiveColor = source.EmissiveColor;
            }
            else if (sourceEffect is DualTextureEffect)
            {
                DualTextureEffect source = sourceEffect as DualTextureEffect;
                DiffuseColor = source.DiffuseColor;
            }
            else if (sourceEffect is AlphaTestEffect)
            {
                AlphaTestEffect source = sourceEffect as AlphaTestEffect;
                DiffuseColor = source.DiffuseColor;
            }


            // Apply to target
            if (effect is IEffectMaterial)
            {
                IEffectMaterial target = effect as IEffectMaterial;
                target.DiffuseColor = DiffuseColor;
                target.EmissiveColor = EmissiveColor;
                target.SpecularColor = SpecularColor;
                target.SpecularPower = SpecularPower;
            }
            else if (effect is BasicEffect)
            {
                BasicEffect target = effect as BasicEffect;
                target.DiffuseColor = DiffuseColor;
                target.EmissiveColor = EmissiveColor;
                target.SpecularColor = SpecularColor;
                target.SpecularPower = SpecularPower;
            }
            else if (effect is SkinnedEffect)
            {
                SkinnedEffect target = effect as SkinnedEffect;
                target.DiffuseColor = DiffuseColor;
                target.EmissiveColor = EmissiveColor;
                target.SpecularColor = SpecularColor;
                target.SpecularPower = SpecularPower;
            }
            else if (effect is EnvironmentMapEffect)
            {
                EnvironmentMapEffect target = effect as EnvironmentMapEffect;
                target.DiffuseColor = DiffuseColor;
                target.EmissiveColor = EmissiveColor;
            }
            else if (effect is DualTextureEffect)
            {
                DualTextureEffect target = effect as DualTextureEffect;
                target.DiffuseColor = DiffuseColor;
            }
            else if (effect is AlphaTestEffect)
            {
                AlphaTestEffect target = effect as AlphaTestEffect;
                target.DiffuseColor = DiffuseColor;
            }
        }

        internal static Texture2D GetTexture(this Effect sourceEffect)
        {
            Texture2D texture = null;

            if (sourceEffect is IEffectTexture)
            {
                IEffectTexture source = sourceEffect as IEffectTexture;
                texture = source.Texture;
            }
            else if (sourceEffect is BasicEffect)
            {
                BasicEffect source = sourceEffect as BasicEffect;
                texture = source.Texture;
            }
            else if (sourceEffect is SkinnedEffect)
            {
                SkinnedEffect source = sourceEffect as SkinnedEffect;
                texture = source.Texture;
            }
            else if (sourceEffect is EnvironmentMapEffect)
            {
                EnvironmentMapEffect source = sourceEffect as EnvironmentMapEffect;
                texture = source.Texture;
            }
            else if (sourceEffect is DualTextureEffect)
            {
                DualTextureEffect source = sourceEffect as DualTextureEffect;
                texture = source.Texture;
            }
            else if (sourceEffect is AlphaTestEffect)
            {
                AlphaTestEffect source = sourceEffect as AlphaTestEffect;
                texture = source.Texture;
            }

            return texture;
        }

        internal static void SetTexture(this Effect effect, Texture2D texture)
        {
            if (effect is IEffectTexture)
            {
                IEffectTexture source = effect as IEffectTexture;
                source.Texture = texture;
            }
            else if (effect is BasicEffect)
            {
                BasicEffect source = effect as BasicEffect;
                source.Texture = texture;
            }
            else if (effect is SkinnedEffect)
            {
                SkinnedEffect source = effect as SkinnedEffect;
                source.Texture = texture;
            }
            else if (effect is EnvironmentMapEffect)
            {
                EnvironmentMapEffect source = effect as EnvironmentMapEffect;
                source.Texture = texture;
            }
            else if (effect is DualTextureEffect)
            {
                DualTextureEffect source = effect as DualTextureEffect;
                source.Texture = texture;
            }
            else if (effect is AlphaTestEffect)
            {
                AlphaTestEffect source = effect as AlphaTestEffect;
                source.Texture = texture;
            }
        }
    }
}