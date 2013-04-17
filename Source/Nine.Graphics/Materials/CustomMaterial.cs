namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Serialization;

    /// <summary>
    /// Represents a type of material that are build from custom shader files.
    /// </summary>
    [BinarySerializable]
    [ContentProperty("ShaderCode")]
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
        [NotBinarySerializable]
        public string ShaderCode
        {
            get { return shaderCode; }
            set
            {
                if (!string.IsNullOrEmpty(shaderCode = value) && serviceProvider != null)
                {
                    Source = new Effect(
                        serviceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice,
                        Extensions.TryInvokeContentPipelineMethod<byte[]>(
                            "EffectCompiler", "Compile", new object[] { shaderCode }));

                    var serializationSharing = serviceProvider.TryGetService<ISerializationSharing>();
                    if (serializationSharing != null)
                        serializationSharing.Share(Source, null);
                }
            }
        }
        private string shaderCode;
        private IServiceProvider serviceProvider;

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
#if WINDOWS
        [TypeConverter(typeof(Nine.Graphics.Design.SamplerStateConverter))]
#endif
        public SamplerState SamplerState { get; set; }
                
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMaterial"/> class for serialization.
        /// </summary>
        internal CustomMaterial(IServiceProvider serviceProvider) 
        {
            this.serviceProvider = serviceProvider; 
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
                
                context.graphics.Textures[0] = texture;
                if (SamplerState != null)
                    context.graphics.SamplerStates[0] = SamplerState;

                parameters.BeginApplyLocalParameters(context, this);
                source.CurrentTechnique.Passes[0].Apply();
            }
        }

        protected override void OnEndApply(DrawingContext context)
        {
            if (source != null)
                parameters.EndApplyLocalParameters();
            if (SamplerState != null)
                context.graphics.SamplerStates[0] = context.SamplerState;
        }

        /// <summary>
        /// Assigns an event handler to a custom material semantics to setup
        /// the shader parameter for each instance.
        /// </summary>
        public static void Bind(string semantics, Action<EffectParameter, DrawingContext, Material> onApply)
        {
            CustomMaterialParameterBinding.Bindings.Add(semantics, CustomMaterialParameterBinding.Bind(onApply));
        }

        /// <summary>
        /// Assigns an event handler to a custom material semantics to setup
        /// the shader global parameter that are shared across the drawing context.
        /// </summary>
        public static void BindGlobal(string semantics, Action<EffectParameter, DrawingContext, Material> onApply)
        {
            CustomMaterialParameterBinding.Bindings.Add(semantics, CustomMaterialParameterBinding.BindGlobal(onApply));
        }

        IEnumerable<EffectParameter> IEffectParameterProvider.GetParameters()
        {
            return source != null ? source.Parameters : Enumerable.Empty<EffectParameter>();
        }

        EffectParameter IEffectParameterProvider.GetParameter(string name)
        {
            return source != null ? source.Parameters[name] : null;
        }
    }
}