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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LinkedMaterial : Material, IEffectTexture, IEffectMaterial, IEffectFog,
                                                   IEffectLights<IAmbientLight>, IEffectLights<IPointLight>,
                                                   IEffectLights<IDirectionalLight>, IEffectLights<ISpotLight>
    {
        LinkedEffect effect;
        LinkedEffect deferredEffect;
        LinkedEffectPart[] parts;

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
            get { return IsTransparent; }
        }

        public override bool IsDeferred { get { return isDeferred; } }
        private bool isDeferred;

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
        public override Effect DeferredEffect
        {
            get { return deferredEffect; }
        }

        public override T As<T>()
        {
            return base.As<T>() ?? Find<T>() ?? effect.Find<T>();
        }

        /// <summary>
        /// Finds the first accurance of LinkedEffectPart that is of type T.
        /// </summary>
        public T Find<T>() where T : class
        {
            return parts.FirstOrDefault(part => part is T) as T;
        }

        /// <summary>
        /// Finds all the accurances of LinkedEffectPart that is of type T.
        /// </summary>
        public IEnumerable<T> FindAll<T>() where T : class
        {
            foreach (LinkedEffectPart part in parts)
            {
                if (part is T)
                    yield return part as T;
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
                parts = new LinkedEffectPart[effect.EffectParts.Count];
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
                    parts[i] = newPart;
                    LinkedEffect.CurrentUniqueName = null;
                }
                LinkedEffect.CurrentEffect = null;

                EffectParts = new ReadOnlyCollection<LinkedEffectPart>(parts);
            }
        }
        

        /// <summary>
        /// Gets or sets the effect parts.
        /// </summary>
        public ReadOnlyCollection<LinkedEffectPart> EffectParts { get; private set; }
        
        [ContentSerializer(ElementName = "EffectParts")]
        internal IList<LinkedEffectPart> EffectPartsSerializer
        {
            get { return parts.ToList(); }
            set
            {
                if (value != null)
                {
                    for (int i = 0; i < value.Count; i++)
                    {
                        if (value[i] is DeferredLightsEffectPart)
                            isDeferred = true;

                        if (i < parts.Length && value[i] != null && parts[i] != null)
                        {
                            if (value[i].GetType() != parts[i].GetType())
                            {
                                throw new ContentLoadException(
                                    string.Format("Effect part type mismatch. Expected {0}, got {1}.",
                                    parts[i].GetType().ToString(), value[i].GetType().ToString()));
                            }
                            value[i].OnApply(parts[i]);
                        }
                    }
                }
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
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] != null)
                    parts[i].OnApply(effect.EffectParts[i]);
            }
        }

        public override Material Clone()
        {
            return new LinkedMaterial(effect)
            {
                IsTransparent = IsTransparent,
                DepthAlphaEnabled = DepthAlphaEnabled,
                EffectPartsSerializer = parts,
            };
        }

        Texture2D texture;
        Texture2D IEffectTexture.Texture
        {
            get { return texture; }
            set
            {
                texture = value;
                foreach (IEffectTexture part in FindAll<IEffectTexture>())
                    part.Texture = value;
            }
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            foreach (IEffectTexture part in FindAll<IEffectTexture>())
                part.SetTexture(usage, texture);
        }

        Vector3 diffuseColor;
        Vector3 IEffectMaterial.DiffuseColor
        {
            get { return diffuseColor; }
            set
            {
                diffuseColor = value;
                foreach (IEffectMaterial part in FindAll<IEffectMaterial>())
                    part.DiffuseColor = value;
            }
        }

        Vector3 emissiveColor;
        Vector3 IEffectMaterial.EmissiveColor
        {
            get { return emissiveColor; }
            set
            {
                emissiveColor = value;
                foreach (IEffectMaterial part in FindAll<IEffectMaterial>())
                    part.EmissiveColor = value;
            }
        }

        Vector3 specularColor;
        Vector3 IEffectMaterial.SpecularColor
        {
            get { return specularColor; }
            set
            {
                specularColor = value;
                foreach (IEffectMaterial part in FindAll<IEffectMaterial>())
                    part.SpecularColor = value;
            }
        }

        float specularPower = 16;
        float IEffectMaterial.SpecularPower
        {
            get { return specularPower; }
            set
            {
                specularPower = value;
                foreach (IEffectMaterial part in FindAll<IEffectMaterial>())
                    part.SpecularPower = value;
            }
        }

        float alpha = 1;
        float IEffectMaterial.Alpha
        {
            get { return alpha; }
            set
            {
                alpha = value;
                foreach (IEffectMaterial part in FindAll<IEffectMaterial>())
                    part.Alpha = value;
            }
        }

        ReadOnlyCollection<IAmbientLight> IEffectLights<IAmbientLight>.Lights
        {
            get { return ((IEffectLights<IAmbientLight>)effect).Lights; }
        }

        ReadOnlyCollection<IPointLight> IEffectLights<IPointLight>.Lights
        {
            get { return ((IEffectLights<IPointLight>)effect).Lights; }
        }

        ReadOnlyCollection<IDirectionalLight> IEffectLights<IDirectionalLight>.Lights
        {
            get { return ((IEffectLights<IDirectionalLight>)effect).Lights; }
        }

        ReadOnlyCollection<ISpotLight> IEffectLights<ISpotLight>.Lights
        {
            get { return ((IEffectLights<ISpotLight>)effect).Lights; }
        }
        
        Vector3 IEffectFog.FogColor
        {
            get { return effect.FogColor; }
            set { effect.FogColor = value; }
        }

        bool IEffectFog.FogEnabled
        {
            get { return effect.FogEnabled; }
            set { effect.FogEnabled = value; }
        }

        float IEffectFog.FogEnd
        {
            get { return effect.FogEnd; }
            set { effect.FogEnd = value; }
        }

        float IEffectFog.FogStart
        {
            get { return effect.FogStart; }
            set { effect.FogStart = value; }
        }
    }

#endif
}