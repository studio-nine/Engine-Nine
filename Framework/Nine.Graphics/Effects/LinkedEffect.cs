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
                return effect.GraphicsDevice;
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

            return effect.Parameters[uniqueName + name];
        }

        /// <summary>
        /// Gets the EffectParameter by semantic from the fragment parameter name.
        /// </summary>
        public EffectParameter GetParameterBySemantic(string semantic)
        {
            Effect effect = Effect != null ? Effect : LinkedEffect.CurrentEffect;
            string uniqueName = UniqueName != null ? UniqueName : LinkedEffect.CurrentUniqueName;

            foreach (EffectParameter parameter in effect.Parameters)
            {
                if (parameter.Semantic == semantic && parameter.Name.StartsWith(uniqueName))
                    return parameter;
            }
            return null;
        }

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

    /// <summary>
    /// Represents a collection of LinkedEffectPart.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LinkedEffectPartCollection : ReadOnlyCollection<LinkedEffectPart>
    {
        internal LinkedEffectPartCollection(IList<LinkedEffectPart> parts) : base(parts) { }
    }
    #endregion

    #region LinkedEffect
    /// <summary>
    /// Represents a Effect that is linked from LinkedEffectParts.
    /// </summary>
    public sealed class LinkedEffect : Effect, IEffectMatrices, IEffectSkinned, IEffectTexture, IEffectMaterial,
                                               IEffectLights<IAmbientLight>, IEffectLights<IPointLight>,
                                               IEffectLights<IDirectionalLight>, IEffectLights<ISpotLight>
    {
        internal static LinkedEffect CurrentEffect;
        internal static string CurrentUniqueName;

        internal LinkedEffect(GraphicsDevice graphics, byte[] code) : base(graphics, code) { }
        internal LinkedEffect(Effect cloneSource) : base(cloneSource) { }

        /// <summary>
        /// Gets all the LinkedEffectPart that makes up this LinkedEffect.
        /// </summary>
        public LinkedEffectPartCollection EffectParts { get; internal set; }
    
        /// <summary>
        /// Finds the first accurance of LinkedEffectPart that is of type T.
        /// </summary>
        public T Find<T>() where T :class
        {
            return EffectParts.FirstOrDefault(part => part is T) as T;
        }

        /// <summary>
        /// Finds all the accurances of LinkedEffectPart that is of type T.
        /// </summary>
        public IEnumerable<T> FindAll<T>() where T : class
        {
            foreach (LinkedEffectPart part in EffectParts)
            {
                if (part is T)
                    yield return part as T;
            }
        }

        protected override void OnApply()
        {
            foreach (LinkedEffectPart part in EffectParts)
            {
                part.OnApply();
            }
            base.OnApply();
        }

        public override Effect Clone()
        {
            LinkedEffect effect = new LinkedEffect(this);
            List<LinkedEffectPart> parts = new List<LinkedEffectPart>();

            CurrentEffect = effect;
            foreach (LinkedEffectPart part in EffectParts)
            {
                CurrentUniqueName = part.UniqueName;
                LinkedEffectPart newPart = part.Clone();
                newPart.Effect = effect;
                newPart.UniqueName = part.UniqueName;
                parts.Add(newPart);
                CurrentUniqueName = null;
            }
            CurrentEffect = null;

            effect.EffectParts = new LinkedEffectPartCollection(parts);
            return effect;
        }

        public void EnableDefaultLighting()
        {
            int i = 0;

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

            foreach (var light in FindAll<IDirectionalLight>())
            {
                if (i == 0)
                {
                    // Key light.
                    light.Direction = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f);
                    light.DiffuseColor = new Vector3(1, 0.9607844f, 0.8078432f);
                    light.SpecularColor = new Vector3(1, 0.9607844f, 0.8078432f);
                }
                else if (i == 1)
                {
                    // Fill light.
                    light.Direction = new Vector3(0.7198464f, 0.3420201f, 0.6040227f);
                    light.DiffuseColor = new Vector3(0.9647059f, 0.7607844f, 0.4078432f);
                    light.SpecularColor = Vector3.Zero;
                }
                else if (i == 2)
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

                i++;
            }
        }

        private ReadOnlyCollection<IPointLight> pointLights;
        private ReadOnlyCollection<IAmbientLight> ambientLights;
        private ReadOnlyCollection<IDirectionalLight> directionalLights;
        private ReadOnlyCollection<ISpotLight> spotLights;

        #region Interfaces
        Matrix projection;
        public Matrix Projection
        {
            get { return projection; }
            set
            {
                projection = value;
                foreach (IEffectMatrices part in FindAll<IEffectMatrices>())
                    part.Projection = value;
            }
        }

        Matrix view;
        public Matrix View
        {
            get { return view; }
            set
            {
                view = value;
                foreach (IEffectMatrices part in FindAll<IEffectMatrices>())
                    part.View = value;
            }
        }

        Matrix world;
        public Matrix World
        {
            get { return world; }
            set
            {
                world = value;
                foreach (IEffectMatrices part in FindAll<IEffectMatrices>())
                    part.World = value;
            }
        }

        bool IEffectSkinned.SkinningEnabled
        {
            get
            {
                IEffectSkinned part = Find<IEffectSkinned>();
                return part != null ? part.SkinningEnabled : false;
            }
            set
            {
                foreach (IEffectSkinned part in FindAll<IEffectSkinned>())
                    part.SkinningEnabled = value;
            }
        }

        void IEffectSkinned.SetBoneTransforms(Matrix[] boneTransforms)
        {
            foreach (IEffectSkinned part in FindAll<IEffectSkinned>())
                part.SetBoneTransforms(boneTransforms);
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
            // FIXME: Token isn't identical for the same shader.
            byte[] token = input.ReadObject<byte[]>();
            byte[] effectCode = input.ReadObject<byte[]>();
            string[] uniqueNames = input.ReadObject<string[]>();
            int count = input.ReadInt32();
            
            GraphicsDevice graphics = input.ContentManager.ServiceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice;

            LinkedEffect effect;
            LinkedEffectToken key = new LinkedEffectToken() { Graphics = graphics, Token = token };

            if (!Dictionary.TryGetValue(key, out effect))
            {
                Dictionary.Add(key, effect = new LinkedEffect(graphics, effectCode));
            }
            else
            {
                effect = (LinkedEffect)effect.Clone();
            }

            List<LinkedEffectPart> parts = new List<LinkedEffectPart>(count);

            LinkedEffect.CurrentEffect = effect;
            for (int i = 0; i < count; i++)
            {
                LinkedEffect.CurrentUniqueName = uniqueNames[i];
                LinkedEffectPart part = input.ReadObject<LinkedEffectPart>();
                part.Effect = effect;
                part.UniqueName = uniqueNames[i];
                parts.Add(part);
            }
            LinkedEffect.CurrentEffect = null;

            effect.EffectParts = new LinkedEffectPartCollection(parts);
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