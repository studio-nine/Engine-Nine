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

namespace Nine.Graphics
{
    #region IEffectInstance
    /// <summary>
    /// Defines a wrapper around effect. Each effect instance stores
    /// a local copy of effect parameter values, this values are pushed to
    /// the underlying effect when Apply is called.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IEffectInstance
    {
        /// <summary>
        /// Gets the underlying effect.
        /// </summary>
        Effect Effect { get; }

        /// <summary>
        /// Applys the parameter values.
        /// </summary>
        void Apply();
    }

    /// <summary>
    /// An effect instance for internal use only.
    /// </summary>
    class EffectInstance : IEffectInstance
    {
        public Effect Effect { get; set; }
        public void Apply() { }
    }
    #endregion

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
    #endregion

    #region Stock Materials
#if !TEXT_TEMPLATE
    public partial class BasicMaterial : IEffectLights<IDirectionalLight>, IEffectLights<IAmbientLight>, IAmbientLight, IEffectMaterial, IEffectTexture
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

        private void OnClone(BasicMaterial cloned)
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
    }

    public partial class EnvironmentMapMaterial : IEffectLights<IDirectionalLight>, IEffectLights<IAmbientLight>, IAmbientLight, IEffectMaterial, IEffectTexture
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

        private void OnClone(EnvironmentMapMaterial cloned)
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
    }

    public partial class SkinnedMaterial : IEffectLights<IDirectionalLight>, IEffectLights<IAmbientLight>, IAmbientLight, IEffectMaterial, IEffectTexture, IEffectSkinned
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

        private void OnClone(SkinnedMaterial cloned)
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
    }

    public partial class DualTextureMaterial : IEffectTexture
    {
        /// <summary>
        /// Gets whether this material is transparent.
        /// </summary>
        public new bool IsTransparent { get; set; }

        private void OnClone(DualTextureMaterial cloned)
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
    }

    public partial class AlphaTestMaterial : IEffectTexture
    {
        /// <summary>
        /// When overriden, returns whether the rendered object is transparent under
        /// current material settings.
        /// </summary>
        protected override bool IsTransparentValue
        {
            get { return false; }
        }

        private void OnClone(AlphaTestMaterial cloned) { }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Diffuse)
                Texture = texture as Texture2D;
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