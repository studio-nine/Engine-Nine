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
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.Materials
{
    #region CustomMaterial
    /// <summary>
    /// Represents a type of material that are build from custom shader files.
    /// </summary>
    [NotContentSerializable]
    [ContentProperty("Parameters")]
    public class CustomMaterial : Material, IEffectParameterProvider
    {
        /// <summary>
        /// Gets or sets the source effect of this custom material.
        /// </summary>
        public Effect Source
        {
            get { return source; }
            set
            {
                if (source != value)
                {
                    source = value;
                    parameters.Bind(this);
                }
            }
        }
        private Effect source;

        /// <summary>
        /// Gets the parameters unique to this custom material instance.
        /// </summary>
        public CustomMaterialParameterCollection Parameters
        {
            get { return parameters; }
        }
        private CustomMaterialParameterCollection parameters = new CustomMaterialParameterCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMaterial"/> class for serialization.
        /// </summary>
        internal CustomMaterial()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMaterial"/> class.
        /// </summary>
        /// <param name="source">The effect.</param>
        public CustomMaterial(Effect source)
        {
            this.Source = source;
        }

        public override void BeginApply(DrawingContext context)
        {
            if (source != null)
            {
                var previous = context.PreviousMaterial as CustomMaterial;
                if (previous == null || previous.source != source)
                    parameters.ApplyGlobalParameters(context, this);
                parameters.BeginApplyLocalParameters(context, this);
                source.CurrentTechnique.Passes[0].Apply();
            }
        }

        public override void EndApply(DrawingContext context)
        {
            if (source != null)
                parameters.EndApplyLocalParameters();
        }

        #region IEffectParameterProvider
        IEnumerable<EffectParameter> IEffectParameterProvider.GetParameters()
        {
            return source != null ? source.Parameters : Enumerable.Empty<EffectParameter>();
        }

        EffectParameter IEffectParameterProvider.GetParameter(string name)
        {
            return source != null ? source.Parameters[name] : null;
        }
        #endregion
    }
    #endregion

    #region CustomEffectReader
    class CustomEffectReader : ContentTypeReader<Effect>
    {
        protected override Effect Read(ContentReader input, Effect existingInstance)
        {
#if SILVERLIGHT
            var graphicsDevice = System.Windows.Graphics.GraphicsDeviceManager.Current.GraphicsDevice;
#else
            var graphicsDevice = input.ContentManager.ServiceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice;
#endif
            var effect = new Effect(graphicsDevice, input.ReadBytes(input.ReadInt32()));
            var parameters = input.ReadObject<Dictionary<string, object>>();
            if (parameters != null)
                foreach (var pair in parameters)
                    effect.Parameters[pair.Key].SetValue(pair.Value);
            return effect;
        }
    }
    #endregion

    #region CustomMaterialReader
    class CustomMaterialReader : ContentTypeReader<CustomMaterial>
    {
        protected override CustomMaterial Read(ContentReader input, CustomMaterial existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new CustomMaterial();
            existingInstance.AttachedProperties = input.ReadObject<System.Collections.Generic.Dictionary<System.Xaml.AttachableMemberIdentifier, System.Object>>();
            existingInstance.IsTransparent = input.ReadBoolean();
            existingInstance.Source = input.ReadObject<Microsoft.Xna.Framework.Graphics.Effect>();
            existingInstance.Texture = input.ReadObject<Microsoft.Xna.Framework.Graphics.Texture2D>();
            existingInstance.IsTransparent = input.ReadBoolean();
            existingInstance.TwoSided = input.ReadBoolean();
            var dictionary = input.ReadRawObject<Dictionary<string, object>>();
            if (dictionary != null)
                existingInstance.Parameters.AddRange(dictionary);
            return existingInstance;
        }
    }
    #endregion
}