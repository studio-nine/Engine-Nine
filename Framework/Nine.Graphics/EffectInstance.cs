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
    #region EffectInstance
    /// <summary>
    /// Defines a wrapper around effect. Each effect instance stores
    /// a local copy of effect parameter values, this values are pushed to
    /// the underlying effect when Apply is called.
    /// </summary>
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

        /// <summary>
        /// Clones a exact copy of this effect instance.
        /// </summary>
        IEffectInstance Clone();
    }

    public class EffectInstance : IEffectInstance
    {
        public Effect Effect { get; internal set; }

        public EffectInstance(Effect effect)
        {
            if (effect == null)
                throw new ArgumentNullException("effect");
            this.Effect = effect;
        }

        public void Apply() { }
        public IEffectInstance Clone() { return new EffectInstance(Effect); }
    }
    #endregion

    #region Stock Effects
#if !TEXT_TEMPLATE
    public partial class BasicEffectInstance : IEffectLights<IDirectionalLight>, IEffectLights<IAmbientLight>, IAmbientLight, IEffectMaterial, IEffectTexture
    {
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

        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.Diffuse)
                Texture = texture as Texture2D;
        }
    }

    public partial class EnvironmentMapEffectInstance : IEffectLights<IDirectionalLight>, IEffectLights<IAmbientLight>, IAmbientLight, IEffectMaterial, IEffectTexture
    {
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

        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.Diffuse)
                Texture = texture as Texture2D;
        }

        Vector3 IEffectMaterial.SpecularColor { get { return Vector3.Zero; } set { } }
        float IEffectMaterial.SpecularPower { get { return 0; } set { } }
    }

    public partial class SkinnedEffectInstance : IEffectLights<IDirectionalLight>, IEffectLights<IAmbientLight>, IAmbientLight, IEffectMaterial, IEffectTexture, IEffectSkinned
    {
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

        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.Diffuse)
                Texture = texture as Texture2D;
        }

        public bool SkinningEnabled { get { return true; } set { } }

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            effect.SetBoneTransforms(boneTransforms);
        }
    }

    public partial class DualTextureEffectInstance : IEffectTexture
    {
        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.Diffuse)
                Texture = texture as Texture2D;
            else if (name == TextureNames.Dual)
                Texture2 = texture as Texture2D;
        }
    }

    public partial class AlphaTestEffectInstance : IEffectTexture
    {
        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.Diffuse)
                Texture = texture as Texture2D;
        }
    }
#endif

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
    #endregion
}