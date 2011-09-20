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
    using ParameterBinding = Dictionary<EffectSemantics, Action<CustomEffect, EffectParameter>>;

    #region CustomEffect
    /// <summary>
    /// Enables automatic custom effect parameter binding using DirectX Standard Annotation and Semantic.
    /// </summary>
    public sealed class CustomEffect : Effect, IEffectMatrices, IEffectMaterial, IEffectTexture, IUpdateable
    {
        #region IEffectMatrices
        private uint DirtyMask = 0;
        private const uint WorldDirtyMask = 1 << 0;
        private const uint ViewDirtyMask = 1 << 1;
        private const uint ProjectionDirtyMask = 1 << 2;

        private Matrix world;
        private Matrix view;
        private Matrix projection;

        public Matrix World
        {
            get { return world; }
            set { world = value; DirtyMask |= WorldDirtyMask; }
        }

        public Matrix View
        {
            get { return view; }
            set { view = value; DirtyMask |= ViewDirtyMask; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; DirtyMask |= ProjectionDirtyMask; }
        }
        #endregion

        #region IEffectMaterial
        public Vector3 DiffuseColor { get; set; }
        public Vector3 EmissiveColor { get; set; }
        public Vector3 SpecularColor { get; set; }
        public float SpecularPower { get; set; }
        public float Alpha { get; set; }
        #endregion

        #region IEffectTexture
        public Texture2D Texture { get; set; }
        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture) { }
        #endregion

        #region IUpdateable
        float elapsedTime;
        float totalTime;

        public void Update(TimeSpan elapsedTime)
        {
            this.totalTime += (float)elapsedTime.TotalSeconds;
            this.elapsedTime = (float)elapsedTime.TotalSeconds;
        }
        #endregion

        #region Parameters
        List<KeyValuePair<EffectSemantics, EffectParameter>> parameters;        
        #endregion

        #region Initialization
        internal CustomEffect(GraphicsDevice graphics, byte[] effectCode) : base(graphics, effectCode)
        {
            EffectSemantics semantic;
            parameters = new List<KeyValuePair<EffectSemantics, EffectParameter>>(Parameters.Count);
            foreach (var parameter in Parameters)
            {
                if (TryGetSemantic(parameter, out semantic))
                    parameters.Add(new KeyValuePair<EffectSemantics, EffectParameter>(semantic, parameter));
            }
        }

        private bool TryGetSemantic(EffectParameter parameter, out EffectSemantics semantic)
        {
#if XBOX
            try
            {
                // TODO: Try catch is slow, move this to the content pipeline?
                semantic = (EffectSemantics)Enum.Parse(typeof(EffectSemantics), parameter.Semantic, true);
                return true;
            }
            catch (Exception)
            {
                semantic = EffectSemantics.Ambient;
                return false;
            }
#else
            return Enum.TryParse<EffectSemantics>(parameter.Semantic, true, out semantic);
#endif
        }
        #endregion

        #region ParameterBinding
        protected override void OnApply()
        {
            // Temperory workaround for shaders that does not have a pixel shader.
            GraphicsDevice.Textures[0] = Texture;

            Action<CustomEffect, EffectParameter> apply = null;

            for (int i = 0; i < parameters.Count; i++)
            {
                if (ParameterBindings.TryGetValue(parameters[i].Key, out apply) && apply != null)
                {
                    apply(this, parameters[i].Value);
                }
            }

            DirtyMask = 0;
            base.OnApply();
        }

        static ParameterBinding ParameterBindings = new ParameterBinding
        {
            #region WorldViewProjection
            { EffectSemantics.World,                    (effect, parameter) =>
                                                        {
                                                            if ((effect.DirtyMask & WorldDirtyMask) != 0)
                                                                parameter.SetValue(effect.world);
                                                        }                                                               },

            { EffectSemantics.WorldInverse,             (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Invert(ref effect.world, out mx);
                                                                parameter.SetValue(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.WorldInverseTranspose,    (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Invert(ref effect.world, out mx);
                                                                parameter.SetValueTranspose(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.WorldTranspose,           (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask)) != 0)
                                                                parameter.SetValueTranspose(effect.world);
                                                        }                                                               },

            { EffectSemantics.View,                     (effect, parameter) =>
                                                        {
                                                            if ((effect.DirtyMask & ViewDirtyMask) != 0)
                                                                parameter.SetValue(effect.world);
                                                        }                                                               },

            { EffectSemantics.ViewInverse,              (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (ViewDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Invert(ref effect.world, out mx);
                                                                parameter.SetValue(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.ViewInverseTranspose,     (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (ViewDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Invert(ref effect.world, out mx);
                                                                parameter.SetValueTranspose(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.ViewTranspose,            (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (ViewDirtyMask)) != 0)
                                                                parameter.SetValueTranspose(effect.world);
                                                        }                                                               },
                                                        

            { EffectSemantics.Projection,               (effect, parameter) =>
                                                        {
                                                            if ((effect.DirtyMask & ProjectionDirtyMask) != 0)
                                                                parameter.SetValue(effect.world);
                                                        }                                                               },

            { EffectSemantics.ProjectionInverse,        (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (ProjectionDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Invert(ref effect.world, out mx);
                                                                parameter.SetValue(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.ProjectionInverseTranspose, (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (ProjectionDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Invert(ref effect.world, out mx);
                                                                parameter.SetValueTranspose(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.ProjectionTranspose,      (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (ProjectionDirtyMask)) != 0)
                                                                parameter.SetValueTranspose(effect.world);
                                                        }                                                               },

            { EffectSemantics.WorldView,                (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask | ViewDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.world, ref effect.view, out mx);
                                                                parameter.SetValue(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.WorldViewInverse,         (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask | ViewDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.world, ref effect.view, out mx);
                                                                Matrix.Invert(ref mx, out mx);
                                                                parameter.SetValue(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.WorldViewInverseTranspose,(effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask | ViewDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.world, ref effect.view, out mx);
                                                                Matrix.Invert(ref mx, out mx);
                                                                parameter.SetValueTranspose(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.WorldViewTranspose,       (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask | ViewDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.world, ref effect.view, out mx);
                                                                parameter.SetValueTranspose(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.ViewProjection,           (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (ProjectionDirtyMask | ViewDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.view, ref effect.projection, out mx);
                                                                parameter.SetValue(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.ViewProjectionInverse,         (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (ProjectionDirtyMask | ViewDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.view, ref effect.projection, out mx);
                                                                Matrix.Invert(ref mx, out mx);
                                                                parameter.SetValue(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.ViewProjectionInverseTranspose,(effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (ProjectionDirtyMask | ViewDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.view, ref effect.projection, out mx);
                                                                Matrix.Invert(ref mx, out mx);
                                                                parameter.SetValueTranspose(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.ViewProjectionTranspose,       (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (ProjectionDirtyMask | ViewDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.view, ref effect.projection, out mx);
                                                                parameter.SetValueTranspose(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.WorldViewProjection,      (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask | ViewDirtyMask | ProjectionDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.world, ref effect.view, out mx);
                                                                Matrix.Multiply(ref mx, ref effect.projection, out mx);
                                                                parameter.SetValue(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.WorldViewProjectionInverse, (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask | ViewDirtyMask | ProjectionDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.world, ref effect.view, out mx);
                                                                Matrix.Multiply(ref mx, ref effect.projection, out mx);
                                                                Matrix.Invert(ref mx, out mx);
                                                                parameter.SetValue(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.WorldViewProjectionInverseTranspose, (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask | ViewDirtyMask | ProjectionDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.world, ref effect.view, out mx);
                                                                Matrix.Multiply(ref mx, ref effect.projection, out mx);
                                                                Matrix.Invert(ref mx, out mx);
                                                                parameter.SetValueTranspose(mx);
                                                            }
                                                        }                                                               },

            { EffectSemantics.WorldViewProjectionTranspose, (effect, parameter) => 
                                                        {
                                                            if ((effect.DirtyMask & (WorldDirtyMask | ViewDirtyMask | ProjectionDirtyMask)) != 0)
                                                            {
                                                                Matrix mx;
                                                                Matrix.Multiply(ref effect.world, ref effect.view, out mx);
                                                                Matrix.Multiply(ref mx, ref effect.projection, out mx);
                                                                parameter.SetValueTranspose(mx);
                                                            }
                                                        }                                                               },
            #endregion
            
            #region Time
            { EffectSemantics.ElapsedTime, (effect, parameter) => parameter.SetValue(effect.elapsedTime) },
            { EffectSemantics.Time, (effect, parameter) => parameter.SetValue(effect.totalTime) },
            #endregion

            #region Material
            { EffectSemantics.DiffuseTexture, (effect, parameter) => parameter.SetValue(effect.Texture) },
            { EffectSemantics.Diffuse, (effect, parameter) =>
                {
                    if (parameter.ParameterType == EffectParameterType.Texture2D)
                        parameter.SetValue(effect.Texture);
                } },
            #endregion
        };
        #endregion
    }
    #endregion
    
    #region CustomEffectReader
    class CustomEffectReader : ContentTypeReader<CustomEffect>
    {
        protected override CustomEffect Read(ContentReader input, CustomEffect existingInstance)
        {
            var effect =  new CustomEffect(input.ContentManager.ServiceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice,
                                           input.ReadBytes(input.ReadInt32()));
            var parameters = input.ReadObject<Dictionary<string, object>>();
            if (parameters != null)
                foreach (var pair in parameters)
                    effect.Parameters[pair.Key].SetValue(pair.Value);
            return effect;
        }
    }
    #endregion
}