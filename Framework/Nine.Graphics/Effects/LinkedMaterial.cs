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
#if !WINDOWS_PHONE

    using Nine.Graphics.Effects.EffectParts;

    /// <summary>
    /// Defines a material for linked effects.
    /// </summary>
    public sealed class LinkedMaterial : Material, IEffectTexture, IEffectMaterial
    {
        LinkedEffect effect;
        LinkedEffectPart[] effectParts;
        IEffectMaterial material;
        IEffectTexture texture;

        /// <summary>
        /// Gets or sets whether this material is transparent.
        /// </summary>
        public new bool IsTransparent { get; set; }

        /// <summary>
        /// When overriden, returns whether the rendered object is transparent under
        /// current material settings.
        /// </summary>
        protected override bool IsTransparentValue
        {
            get { return Alpha < 1 || IsTransparent; }
        }

        public override bool IsDeferred 
        { 
            get { return effect.GraphicsBufferEffect != null; } 
        }

        /// <summary>
        /// For content serialization.
        /// </summary>
        internal LinkedMaterial() { }

        /// <summary>
        /// Gets the underlying effect.
        /// </summary>
        public override Effect Effect
        {
            get { return effect; }
        }

        /// <summary>
        /// Gets the deferred effect used to generate the graphics buffer.
        /// </summary>
        public override Effect GraphicsBufferEffect
        {
            get { return effect.GraphicsBufferEffect; }
        }

        /// <summary>
        /// Queries the material for the specified interface T.
        /// </summary>
        public override T Find<T>()
        {
            var result = base.Find<T>();
            if (result != null)
                return result;
            
            for (int i = 0; i < effectParts.Length; i++)
            {
                var part = effectParts[i];
                if (part is T)
                    return part as T;
            }

            if (effect is T)
                return effect as T;

            return effect.Find<T>();
        }

        /// <summary>
        /// Finds all the accurances of LinkedEffectPart that is of type T.
        /// </summary>
        public void FindAll<T>(ICollection<T> result) where T : class
        {
            for (int i = 0; i < effectParts.Length; i++)
            {
                var part = effectParts[i];
                if (part is T)
                    result.Add(part as T);
            }
        }

        [ContentSerializer(ElementName="Effect")]
        internal Effect EffectSerializer
        {
            get { return effect; }
            set
            {
                effect = value as LinkedEffect;
                if (effect == null)
                    throw new InvalidOperationException();

                LinkedEffect.CurrentEffect = effect;
                effectParts = new LinkedEffectPart[effect.EffectParts.Count];
                for (int i = 0; i < effect.EffectParts.Count; i++)
                {
                    var part = effect.EffectParts[i];

                    // LinkedEffectPart contains nothing useful to clone.
                    if (part.GetType() == typeof(LinkedEffectPart) || !part.IsMaterial)
                        continue;

                    LinkedEffect.CurrentUniqueName = part.UniqueName;
                    LinkedEffectPart newPart = part.Clone();
                    newPart.Effect = effect;
                    newPart.UniqueName = part.UniqueName;
                    effectParts[i] = newPart;
                    LinkedEffect.CurrentUniqueName = null;
                }
                LinkedEffect.CurrentEffect = null;

                EffectParts = new ReadOnlyCollection<LinkedEffectPart>(effectParts);
            }
        }
        

        /// <summary>
        /// Gets or sets the effect parts.
        /// </summary>
        public ReadOnlyCollection<LinkedEffectPart> EffectParts { get; private set; }
        
        [ContentSerializer(ElementName = "EffectParts")]
        internal IList<LinkedEffectPart> EffectPartsSerializer
        {
            get { return effectParts.ToList(); }
            set
            {
                if (value != null)
                {
                    for (int i = 0; i < value.Count; i++)
                    {
                        if (i < effectParts.Length && value[i] != null && effectParts[i] != null)
                        {
                            if (value[i].GetType() != effectParts[i].GetType())
                            {
                                throw new ContentLoadException(
                                    string.Format("Effect part type mismatch. Expected {0}, got {1}.",
                                    effectParts[i].GetType().ToString(), value[i].GetType().ToString()));
                            }
                            value[i].OnApply(effectParts[i]);
                        }
                    }
                }

                material = effectParts.OfType<IEffectMaterial>().FirstOrDefault();
                texture = effectParts.OfType<IEffectTexture>().FirstOrDefault();
            }
        }

        public LinkedMaterial(LinkedEffect effect)
        {
            if (effect == null)
                throw new ArgumentNullException("effect");

            this.EffectSerializer = effect;
        }

        public override void Apply()
        {
            for (int i = 0; i < effectParts.Length; i++)
            {
                if (effectParts[i] != null)
                {
                    effectParts[i].OnApply(effect.EffectParts[i]);
                    if (effect.GraphicsBufferEffect != null &&
                        effect.GraphicsBufferEffect.EffectParts.Count > i &&
                        effect.GraphicsBufferEffect.EffectParts[i] != null)
                    {
                        effectParts[i].OnApply(effect.GraphicsBufferEffect.EffectParts[i]);
                    }
                }
            }
        }

        public override Material Clone()
        {
            return new LinkedMaterial(effect)
            {
                IsTransparent = IsTransparent,
                DepthAlphaEnabled = DepthAlphaEnabled,
                EffectPartsSerializer = effectParts,
            };
        }

        /// <summary>
        /// Gets or sets the primiary diffuse texture of the current effect.
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D Texture
        {
            get { return texture != null ? texture.Texture : null; }
            set
            {
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectTexture;
                    if (part == null)
                        continue;
                    part.Texture = value;
                }
            }
        }

        /// <summary>
        /// Sets the texture with the specified texture usage.
        /// </summary>
        public void SetTexture(TextureUsage usage, Texture texture)
        {
            for (int i = 0; i < effectParts.Length; i++)
            {
                var part = effectParts[i] as IEffectTexture;
                if (part == null)
                    continue;
                part.SetTexture(usage, texture);
            }
        }

        /// <summary>
        /// Gets or sets the diffuse color of the effect.
        /// </summary>
        [ContentSerializerIgnore]
        public Vector3 DiffuseColor
        {
            get { return material != null ? material.DiffuseColor : Vector3.One; }
            set
            {
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMaterial;
                    if (part == null)
                        continue;
                    part.DiffuseColor = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the emissive color of the effect.
        /// </summary>
        [ContentSerializerIgnore]
        public Vector3 EmissiveColor
        {
            get { return material != null ? material.EmissiveColor : Vector3.Zero; }
            set
            {
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMaterial;
                    if (part == null)
                        continue;
                    part.EmissiveColor = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the specular color of the effect.
        /// </summary>
        [ContentSerializerIgnore]
        public Vector3 SpecularColor
        {
            get { return material != null ? material.SpecularColor : Vector3.Zero; }
            set
            {
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMaterial;
                    if (part == null)
                        continue;
                    part.SpecularColor = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the specular power of the effect.
        /// </summary>
        [ContentSerializerIgnore]
        public float SpecularPower
        {
            get { return material != null ? material.SpecularPower : 16; }
            set
            {
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMaterial;
                    if (part == null)
                        continue;
                    part.SpecularPower = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the opaque of the effect.
        /// </summary>
        [ContentSerializerIgnore]
        public float Alpha
        {
            get { return material != null ? material.Alpha : 1; }
            set
            {
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMaterial;
                    if (part == null)
                        continue;
                    part.Alpha = value;
                }
            }
        }
    }
    
#endif
}