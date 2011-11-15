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
#if SILVERLIGHT
using Effect = Microsoft.Xna.Framework.Graphics.SilverlightEffect;
using EffectParameter = Microsoft.Xna.Framework.Graphics.SilverlightEffectParameter;
using EffectParameterCollection = Microsoft.Xna.Framework.Graphics.SilverlightEffectParametersCollection;
#endif
#endregion

namespace Nine.Graphics.Effects
{
#if !WINDOWS_PHONE

    using Nine.Graphics.Effects.EffectParts;

    #region LinkedEffectPart
    /// <summary>
    /// Represents a basic effect fragment that makes up an LinkedEffect.
    /// </summary>
    public class LinkedEffectPart
    {
        internal LinkedEffect Effect;
        internal string UniqueName;

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                Effect effect = Effect != null ? Effect : LinkedEffect.CurrentEffect;
                return effect != null ? effect.GraphicsDevice : null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this LinkedEffectPart shall be copied to the
        /// LinkedMaterial.
        /// If this value is true, you must override OnApply(LinkedEffectPart part) to 
        /// push all the parameters to the target part.
        /// The default value is false.
        /// </summary>
        /// 
        /// TODO: Pick a better name?
        public virtual bool IsMaterial { get { return false; } }

        /// <summary>
        /// Gets the EffectParameter with the name from the fragment parameter name.
        /// </summary>
        public EffectParameter GetParameter(string name)
        {
            Effect effect = Effect != null ? Effect : LinkedEffect.CurrentEffect;
            string uniqueName = UniqueName != null ? UniqueName : LinkedEffect.CurrentUniqueName;

            return effect != null && uniqueName != null ? effect.Parameters[uniqueName + name] : null;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the EffectParameter by semantic from the fragment parameter name.
        /// </summary>
        public EffectParameter GetParameterBySemantic(string semantic)
        {
            Effect effect = Effect != null ? Effect : LinkedEffect.CurrentEffect;
            string uniqueName = UniqueName != null ? UniqueName : LinkedEffect.CurrentUniqueName;
            
            if (effect == null || uniqueName == null)
                return null;

            foreach (EffectParameter parameter in effect.Parameters)
            {
                if (parameter.Semantic == semantic && parameter.Name.StartsWith(uniqueName))
                    return parameter;
            }
            return null;
        }
#endif

        /// <summary>
        /// Applies the effect state just prior to rendering the effect.
        /// </summary>
        protected internal virtual void OnApply()
        {

        }

        /// <summary>
        /// Applies the effect state to another instance of effect part of the same type.
        /// </summary>
        protected internal virtual void OnApply(LinkedEffectPart part)
        {

        }

        /// <summary>
        /// Copies data from an existing object to this object.
        /// </summary>
        protected internal virtual LinkedEffectPart Clone()
        {
            return new LinkedEffectPart();
        }
    }
    #endregion

    #region LinkedEffect
    /// <summary>
    /// Represents a Effect that is linked from LinkedEffectParts.
    /// </summary>
    public sealed class LinkedEffect : Effect, IEffectMatrices, IEffectSkinned, IEffectTexture, IEffectMaterial, IEffectFog,
                                               IEffectLights<IAmbientLight>, IEffectLights<IPointLight>,
                                               IEffectLights<IDirectionalLight>, IEffectLights<ISpotLight>
    {
        internal static LinkedEffect CurrentEffect;
        internal static string CurrentUniqueName;

        internal LinkedEffect(GraphicsDevice graphics, byte[] code) : base(graphics, code) { }
#if !SILVERLIGHT
        internal LinkedEffect(Effect cloneSource) : base(cloneSource) { }
#endif

        /// <summary>
        /// Gets the linked effect used to render the graphics buffer in deferred lighting.
        /// </summary>
        public LinkedEffect GraphicsBufferEffect { get; internal set; }

        /// <summary>
        /// Gets all the LinkedEffectPart that makes up this LinkedEffect.
        /// </summary>
        public ReadOnlyCollection<LinkedEffectPart> EffectParts { get; internal set; }
        internal LinkedEffectPart[] effectParts;
    
        /// <summary>
        /// Finds the first accurance of LinkedEffectPart that is of type T.
        /// </summary>
        public T Find<T>() where T :class
        {
            for (int i = 0; i < effectParts.Length; i++)
            {
                var part = effectParts[i];
                if (part is T)
                    return part as T;
            }
            return null;
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

        protected override void OnApply()
        {
            for (int i = 0; i < effectParts.Length; i++)
            {
                effectParts[i].OnApply();
            }
            base.OnApply();
        }

#if !SILVERLIGHT
        public override Effect Clone()
        {
            LinkedEffect effect = new LinkedEffect(this);
            effect.effectParts = new LinkedEffectPart[effectParts.Length];

            CurrentEffect = effect;
            for (int i = 0; i < effectParts.Length; i++)
            {
                var part = effectParts[i];
                CurrentUniqueName = part.UniqueName;
                LinkedEffectPart newPart = part.Clone();
                newPart.Effect = effect;
                newPart.UniqueName = part.UniqueName;
                effect.effectParts[i] = newPart;
                CurrentUniqueName = null;
            }
            CurrentEffect = null;

            effect.EffectParts = new ReadOnlyCollection<LinkedEffectPart>(effect.effectParts);
            return effect;
        }
#endif

        public void EnableDefaultLighting()
        {
            var material = Find<IEffectMaterial>();
            if (material != null)
            {
                material.DiffuseColor = Vector3.One;
                material.EmissiveColor = Vector3.Zero;
                material.SpecularColor = Vector3.One;
            }

            var ambient = Find<IAmbientLight>();
            if (ambient != null)
                ambient.AmbientLightColor = new Vector3(0.05333332f, 0.09882354f, 0.1819608f);

            var currentDirectionalLight = 0;
            for (int i = 0; i < effectParts.Length; i++)
            {
                var light = effectParts[i] as IDirectionalLight;
                if (light == null)
                    continue;

                if (currentDirectionalLight == 0)
                {
                    // Key light.
                    light.Direction = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f);
                    light.DiffuseColor = new Vector3(1, 0.9607844f, 0.8078432f);
                    light.SpecularColor = new Vector3(1, 0.9607844f, 0.8078432f);
                }
                else if (currentDirectionalLight == 1)
                {
                    // Fill light.
                    light.Direction = new Vector3(0.7198464f, 0.3420201f, 0.6040227f);
                    light.DiffuseColor = new Vector3(0.9647059f, 0.7607844f, 0.4078432f);
                    light.SpecularColor = Vector3.Zero;
                }
                else if (currentDirectionalLight == 2)
                {
                    // Back light.
                    light.Direction = new Vector3(0.4545195f, -0.7660444f, 0.4545195f);
                    light.DiffuseColor = new Vector3(0.3231373f, 0.3607844f, 0.3937255f);
                    light.SpecularColor = new Vector3(0.3231373f, 0.3607844f, 0.3937255f);
                }
                else
                {
                    // Disable light
                    light.DiffuseColor = Vector3.Zero;
                    light.SpecularColor = Vector3.Zero;
                }
                currentDirectionalLight++;
            };
        }

        private ReadOnlyCollection<IPointLight> pointLights;
        private ReadOnlyCollection<IAmbientLight> ambientLights;
        private ReadOnlyCollection<IDirectionalLight> directionalLights;
        private ReadOnlyCollection<ISpotLight> spotLights;

        #region Interfaces
        /// <summary>
        /// Gets or sets the projection matrix in the current effect.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
            set
            {
                projection = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMatrices;
                    if (part == null)
                        continue;
                    part.Projection = projection;
                }
            }
        }
        Matrix projection;

        /// <summary>
        /// Gets or sets the view matrix in the current effect.
        /// </summary>
        public Matrix View
        {
            get { return view; }
            set
            {
                view = value;
                projection = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMatrices;
                    if (part == null)
                        continue;
                    part.View = view;
                }
            }
        }
        Matrix view;

        /// <summary>
        /// Gets or sets the world matrix in the current effect.
        /// </summary>
        public Matrix World
        {
            get { return world; }
            set
            {
                world = value;
                projection = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMatrices;
                    if (part == null)
                        continue;
                    part.World = world;
                }
            }
        }
        Matrix world;

        /// <summary>
        /// Gets or sets if vertex skinning is enabled by this effect.
        /// </summary>
        public bool SkinningEnabled
        {
            get { return skinningEnabled; }
            set
            {
                skinningEnabled = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectSkinned;
                    if (part == null)
                        continue;
                    part.SkinningEnabled = skinningEnabled;
                }
            }
        }
        bool skinningEnabled;

        /// <summary>
        /// Sets the bones transforms for the skinned effect.
        /// </summary>
        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            for (int i = 0; i < effectParts.Length; i++)
            {
                var part = effectParts[i] as IEffectSkinned;
                if (part == null)
                    continue; 
                part.SetBoneTransforms(boneTransforms);
            }
        }

        /// <summary>
        /// Gets or sets the primiary diffuse texture of the current effect.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set
            {
                texture = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectTexture;
                    if (part == null)
                        continue;
                    part.Texture = texture;
                }
            }
        }
        Texture2D texture;

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
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set
            {
                diffuseColor = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMaterial;
                    if (part == null)
                        continue;
                    part.DiffuseColor = diffuseColor;
                }
            }
        }
        Vector3 diffuseColor;

        /// <summary>
        /// Gets or sets the emissive color of the effect.
        /// </summary>
        public Vector3 EmissiveColor
        {
            get { return emissiveColor; }
            set
            {
                emissiveColor = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMaterial;
                    if (part == null)
                        continue;
                    part.EmissiveColor = emissiveColor;
                }
            }
        }
        Vector3 emissiveColor;

        /// <summary>
        /// Gets or sets the specular color of the effect.
        /// </summary>
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set
            {
                specularColor = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMaterial;
                    if (part == null)
                        continue;
                    part.SpecularColor = specularColor;
                }
            }
        }
        Vector3 specularColor;

        /// <summary>
        /// Gets or sets the specular power of the effect.
        /// </summary>
        public float SpecularPower
        {
            get { return specularPower; }
            set
            {
                specularPower = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMaterial;
                    if (part == null)
                        continue;
                    part.SpecularPower = specularPower;
                }
            }
        }
        float specularPower = 16;

        /// <summary>
        /// Gets or sets the opaque of the effect.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }
            set
            {
                alpha = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectMaterial;
                    if (part == null)
                        continue;
                    part.Alpha = alpha;
                }
            }
        }
        float alpha = 1;

        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public Vector3 FogColor
        {
            get { return fogColor; }
            set
            {
                fogColor = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectFog;
                    if (part == null)
                        continue;
                    part.FogColor = fogColor;
                }
            }
        }
        Vector3 fogColor = Vector3.One;

        /// <summary>
        /// Enables or disables fog.
        /// </summary>
        public bool FogEnabled
        {
            get { return fogEnabled; }
            set
            {
                fogEnabled = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectFog;
                    if (part == null)
                        continue;
                    part.FogEnabled = fogEnabled;
                }
            }
        }
        bool fogEnabled;

        /// <summary>
        /// Gets or sets maximum z value for fog.
        /// </summary>
        public float FogEnd
        {
            get { return fogEnd; }
            set
            {
                fogEnd = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectFog;
                    if (part == null)
                        continue;
                    part.FogEnd = fogEnd;
                }
            }
        }
        float fogEnd;

        /// <summary>
        /// Gets or sets minimum z value for fog.
        /// </summary>
        public float FogStart
        {
            get { return fogStart; }
            set
            {
                fogStart = value;
                for (int i = 0; i < effectParts.Length; i++)
                {
                    var part = effectParts[i] as IEffectFog;
                    if (part == null)
                        continue;
                    part.FogStart = fogStart;
                }
            }
        }
        float fogStart;

        ReadOnlyCollection<IPointLight> IEffectLights<IPointLight>.Lights
        {
            get { return pointLights ?? (pointLights = new ReadOnlyCollection<IPointLight>(EffectParts.OfType<IPointLight>().ToList())); }
        }

        ReadOnlyCollection<IDirectionalLight> IEffectLights<IDirectionalLight>.Lights
        {
            get { return directionalLights ?? (directionalLights = new ReadOnlyCollection<IDirectionalLight>(EffectParts.OfType<IDirectionalLight>().ToList())); }
        }

        ReadOnlyCollection<ISpotLight> IEffectLights<ISpotLight>.Lights
        {
            get { return spotLights ?? (spotLights = new ReadOnlyCollection<ISpotLight>(EffectParts.OfType<ISpotLight>().ToList())); }
        }

        ReadOnlyCollection<IAmbientLight> IEffectLights<IAmbientLight>.Lights
        {
            get { return ambientLights ?? (ambientLights = new ReadOnlyCollection<IAmbientLight>(EffectParts.OfType<IAmbientLight>().ToList())); }
        }        
        #endregion
    }
    #endregion

    #region LinkedEffectReader
    /// <summary>
    /// Content reader for LinkedEffect.
    /// </summary>
    class LinkedEffectReader : ContentTypeReader<LinkedEffect>
    {
        static Dictionary<LinkedEffectToken, LinkedEffect> Dictionary = new Dictionary<LinkedEffectToken, LinkedEffect>();

        protected override LinkedEffect Read(ContentReader input, LinkedEffect existingInstance)
        {
            byte[] token = input.ReadObject<byte[]>();
            byte[] effectCode = input.ReadObject<byte[]>();
            string[] uniqueNames = input.ReadObject<string[]>();
            int count = input.ReadInt32();
            
#if SILVERLIGHT
            var graphicsDevice = System.Windows.Graphics.GraphicsDeviceManager.Current.GraphicsDevice;

            LinkedEffect effect = new LinkedEffect(graphicsDevice, effectCode);
#else
            var graphicsDevice = input.ContentManager.ServiceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice;

            LinkedEffect effect;
            LinkedEffectToken key = new LinkedEffectToken() { Graphics = graphicsDevice, Token = token };
            
            if (!Dictionary.TryGetValue(key, out effect))
            {
                Dictionary.Add(key, effect = new LinkedEffect(graphicsDevice, effectCode));
            }
            else
            {
                effect = (LinkedEffect)effect.Clone();
            }
#endif

            effect.effectParts = new LinkedEffectPart[count];
            LinkedEffect.CurrentEffect = effect;
            for (int i = 0; i < count; i++)
            {
                LinkedEffect.CurrentUniqueName = uniqueNames[i];
                LinkedEffectPart part = input.ReadObject<LinkedEffectPart>();
                part.Effect = effect;
                part.UniqueName = uniqueNames[i];
                effect.effectParts[i] = part;
            }
            LinkedEffect.CurrentEffect = null;

            effect.EffectParts = new ReadOnlyCollection<LinkedEffectPart>(effect.effectParts);
            effect.GraphicsBufferEffect = input.ReadObject<LinkedEffect>();
            return effect;
        }
    }

    class LinkedEffectToken
    {
        public byte[] Token;
        public GraphicsDevice Graphics;

        public override bool Equals(object obj)
        {
            if (obj is LinkedEffectToken)
            {
                LinkedEffectToken token = (LinkedEffectToken)obj;
                return token.Graphics == Graphics && Enumerable.SequenceEqual(token.Token, Token);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Graphics.GetHashCode();
        }
    }
    #endregion

#endif
}