#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Effects
{
    #region Material
    /// <summary>
    /// Represents a local copy of settings of the specified effect.
    /// </summary>
    public abstract class Material : IEffectInstance
    {
        /// <summary>
        /// Gets whether this material is transparent.
        /// </summary>
        public bool IsTransparent { get { return IsTransparentValue; } }

        /// <summary>
        /// When overriden, returns whether the rendered object is transparent under
        /// current material settings.
        /// </summary>
        protected virtual bool IsTransparentValue { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this material is deferred.
        /// </summary>
        public virtual bool IsDeferred { get { return false; } }

        /// <summary>
        /// Gets or sets a value indicating whether texture alpha test is enabled when
        /// generating depth info. This value is usually used to generate shadow maps.
        /// </summary>
        /// TODO: Pick a better name?
        public bool DepthAlphaEnabled { get; set; }

        /// <summary>
        /// Queries the material for the specified interface T.
        /// </summary>
        public virtual T As<T>() where T : class
        {
            return this as T;
        }

        /// <summary>
        /// Gets the underlying effect.
        /// </summary>
        public abstract Effect Effect { get; }

        /// <summary>
        /// Applys the parameter values.
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public abstract Material Clone();
    }

    class EffectMaterial : Material
    {
        Effect effect;
        public override Effect Effect { get { return effect; } }
        public void SetEffect(Effect effect) { this.effect = effect; }
        public override void Apply() { }
        public override Material Clone() { throw new NotSupportedException(); }
    }
    #endregion

    #region Stock Materials
#if !TEXT_TEMPLATE
    public partial class BasicMaterial : IEffectLights<IDirectionalLight>, IEffectLights<IAmbientLight>, IAmbientLight, IEffectMaterial, IEffectTexture, IEffectFog
    {
        /// <summary>
        /// Gets or sets whether this material is transparent.
        /// </summary>
        /// <remarks>
        /// Typically this value don't need to be set explicitly, but when the texture or
        /// vertex color contains transparent alpha channel and that channel represents
        /// transparency, you can set the value to explicitly turn on alpha blending.
        /// </remarks>
        public new bool IsTransparent { get; set; }

        /// <summary>
        /// When overriden, returns whether the rendered object is transparent under
        /// current material settings.
        /// </summary>
        protected override bool IsTransparentValue
        {
            get { return IsTransparent || Alpha < 1; }
        }

        partial void OnClone(BasicMaterial cloned)
        {
            cloned.IsTransparent = this.IsTransparent;
        }

        /// <summary>
        /// Gets the lights.
        /// </summary>
        public ReadOnlyCollection<IDirectionalLight> Lights
        {
            get
            {
                if (lights == null)
                {
                    lights = new ReadOnlyCollection<IDirectionalLight>(new[] 
                    {
                        new XnaDirectionalLight(effect.DirectionalLight0),
                        new XnaDirectionalLight(effect.DirectionalLight1),
                        new XnaDirectionalLight(effect.DirectionalLight2),
                    });
                }
                LightingEnabled = true;
                PreferPerPixelLighting = true;
                return lights;
            }
        }
        private ReadOnlyCollection<IDirectionalLight> lights;

        ReadOnlyCollection<IAmbientLight> IEffectLights<IAmbientLight>.Lights
        {
            get
            {
                if (ambientLights == null)
                    ambientLights = new ReadOnlyCollection<IAmbientLight>(new[] { this });
                return ambientLights;
            }
        }
        private ReadOnlyCollection<IAmbientLight> ambientLights;

        Vector3 IAmbientLight.AmbientLightColor
        {
            get { return effect.AmbientLightColor; }
            set { effect.AmbientLightColor = value; }
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Diffuse)
                Texture = texture as Texture2D;
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

    public partial class EnvironmentMapMaterial : IEffectLights<IDirectionalLight>, IEffectLights<IAmbientLight>, IAmbientLight, IEffectMaterial, IEffectTexture, IEffectFog
    {
        /// <summary>
        /// Gets whether this material is transparent.
        /// </summary>
        public new bool IsTransparent { get; set; }

        /// <summary>
        /// When overriden, returns whether the rendered object is transparent under
        /// current material settings.
        /// </summary>
        protected override bool IsTransparentValue
        {
            get { return IsTransparent || Alpha < 1; }
        }

        partial void OnClone(EnvironmentMapMaterial cloned)
        {
            cloned.IsTransparent = this.IsTransparent;
        }

        /// <summary>
        /// Gets the lights.
        /// </summary>
        public ReadOnlyCollection<IDirectionalLight> Lights
        {
            get
            {
                if (lights == null)
                {
                    lights = new ReadOnlyCollection<IDirectionalLight>(new[] 
                    {
                        new XnaDirectionalLight(effect.DirectionalLight0),
                        new XnaDirectionalLight(effect.DirectionalLight1),
                        new XnaDirectionalLight(effect.DirectionalLight2),
                    });
                }
                return lights;
            }
        }
        private ReadOnlyCollection<IDirectionalLight> lights;

        ReadOnlyCollection<IAmbientLight> IEffectLights<IAmbientLight>.Lights
        {
            get
            {
                if (ambientLights == null)
                    ambientLights = new ReadOnlyCollection<IAmbientLight>(new[] { this });
                return ambientLights;
            }
        }
        private ReadOnlyCollection<IAmbientLight> ambientLights;

        Vector3 IAmbientLight.AmbientLightColor
        {
            get { return effect.AmbientLightColor; }
            set { effect.AmbientLightColor = value; }
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Diffuse)
                Texture = texture as Texture2D;
        }

        Vector3 IEffectMaterial.SpecularColor { get { return Vector3.Zero; } set { } }
        float IEffectMaterial.SpecularPower { get { return 0; } set { } }
        
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

    public partial class SkinnedMaterial : IEffectLights<IDirectionalLight>, IEffectLights<IAmbientLight>, IAmbientLight, IEffectMaterial, IEffectTexture, IEffectSkinned, IEffectFog
    {
        /// <summary>
        /// Gets whether this material is transparent.
        /// </summary>
        public new bool IsTransparent { get; set; }

        /// <summary>
        /// When overriden, returns whether the rendered object is transparent under
        /// current material settings.
        /// </summary>
        protected override bool IsTransparentValue
        {
            get { return IsTransparent || Alpha < 1; }
        }

        partial void OnClone(SkinnedMaterial cloned)
        {
            cloned.IsTransparent = this.IsTransparent;
        }

        /// <summary>
        /// Gets the lights.
        /// </summary>
        public ReadOnlyCollection<IDirectionalLight> Lights
        {
            get
            {
                if (lights == null)
                {
                    lights = new ReadOnlyCollection<IDirectionalLight>(new[] 
                    {
                        new XnaDirectionalLight(effect.DirectionalLight0),
                        new XnaDirectionalLight(effect.DirectionalLight1),
                        new XnaDirectionalLight(effect.DirectionalLight2),
                    });
                }
                PreferPerPixelLighting = true;
                return lights;
            }
        }
        private ReadOnlyCollection<IDirectionalLight> lights;

        ReadOnlyCollection<IAmbientLight> IEffectLights<IAmbientLight>.Lights
        {
            get
            {
                if (ambientLights == null)
                    ambientLights = new ReadOnlyCollection<IAmbientLight>(new[] { this });
                return ambientLights;
            }
        }
        private ReadOnlyCollection<IAmbientLight> ambientLights;

        Vector3 IAmbientLight.AmbientLightColor
        {
            get { return effect.AmbientLightColor; }
            set { effect.AmbientLightColor = value; }
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Diffuse)
                Texture = texture as Texture2D;
        }

        public bool SkinningEnabled { get { return true; } set { } }

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            effect.SetBoneTransforms(boneTransforms);
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

    public partial class DualTextureMaterial : IEffectTexture, IEffectFog
    {
        /// <summary>
        /// Gets whether this material is transparent.
        /// </summary>
        public new bool IsTransparent { get; set; }

        partial void OnClone(DualTextureMaterial cloned)
        {
            cloned.IsTransparent = this.IsTransparent;
        }

        /// <summary>
        /// When overriden, returns whether the rendered object is transparent under
        /// current material settings.
        /// </summary>
        protected override bool IsTransparentValue
        {
            get { return IsTransparent || Alpha < 1; }
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Diffuse)
                Texture = texture as Texture2D;
            else if (usage == TextureUsage.Dual)
                Texture2 = texture as Texture2D;
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

    public partial class AlphaTestMaterial : IEffectTexture, IEffectFog
    {
        /// <summary>
        /// When overriden, returns whether the rendered object is transparent under
        /// current material settings.
        /// </summary>
        protected override bool IsTransparentValue
        {
            get { return false; }
        }

        partial void OnClone(AlphaTestMaterial cloned) { }
        partial void OnCreate() { DepthAlphaEnabled = true; }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Diffuse)
                Texture = texture as Texture2D;
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

    #region XnaDirectionalLight
    class XnaDirectionalLight : IDirectionalLight
    {
        DirectionalLight light;

        public XnaDirectionalLight(DirectionalLight light)
        {
            this.light = light;
        }

        public Vector3 Direction
        {
            get { return light.Direction; }
            set { light.Direction = value; }
        }

        public Vector3 DiffuseColor
        {
            get { return light.DiffuseColor; }
            set { light.DiffuseColor = value; light.Enabled = value != Vector3.Zero; }
        }

        public Vector3 SpecularColor
        {
            get { return light.SpecularColor; }
            set { light.SpecularColor = value; }
        }
    }
    #endregion
#endif
    #endregion
}