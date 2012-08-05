namespace Nine.Graphics.Materials
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Design;

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
        /// Gets or sets the shader code for this custom material.
        /// </summary>
        [ContentSerializerIgnore]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string ShaderCode { get; set; }

        /// <summary>
        /// Gets the parameters unique to this custom material instance.
        /// </summary>
        public CustomMaterialParameterCollection Parameters
        {
            get { return parameters; }
        }
        private CustomMaterialParameterCollection parameters = new CustomMaterialParameterCollection();

        /// <summary>
        /// Gets or sets the sampler state for this custom material.
        /// </summary>
        [TypeConverter(typeof(SamplerStateConverter))]
        public SamplerState SamplerState { get; set; }

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

        protected override void OnBeginApply(DrawingContext context, Material previousMaterial)
        {
            if (source != null)
            {
                var previous = previousMaterial as CustomMaterial;
                if (previous == null || previous.source != source)
                    parameters.ApplyGlobalParameters(context, this);
                
                context.GraphicsDevice.Textures[0] = texture;
                if (SamplerState != null)
                    context.GraphicsDevice.SamplerStates[0] = SamplerState;

                parameters.BeginApplyLocalParameters(context, this);
                source.CurrentTechnique.Passes[0].Apply();
            }
        }

        protected override void OnEndApply(DrawingContext context)
        {
            if (source != null)
                parameters.EndApplyLocalParameters();
            if (SamplerState != null)
                context.GraphicsDevice.SamplerStates[0] = context.Settings.DefaultSamplerState;
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
    class CustomEffectReader : ContentTypeReader
    {
        // To avoid conflict with the built-in effect reader, we walkaround this 
        // by setting the target type of something unique.
        public CustomEffectReader() : base(typeof(CustomEffectReader)) { }

        protected override object Read(ContentReader input, object existingInstance)
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
                    EffectExtensions.SetValue(effect.Parameters[pair.Key], pair.Value);
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
            existingInstance.texture = input.ReadObject<Microsoft.Xna.Framework.Graphics.Texture2D>();
            existingInstance.IsTransparent = input.ReadBoolean();
            existingInstance.TwoSided = input.ReadBoolean();
            existingInstance.SamplerState = input.ReadObject<SamplerState>();
            var dictionary = input.ReadRawObject<Dictionary<string, object>>();
            if (dictionary != null)
                existingInstance.Parameters.AddRange(dictionary);
            return existingInstance;
        }
    }
    #endregion
}