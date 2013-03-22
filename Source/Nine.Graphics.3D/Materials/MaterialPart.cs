namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Represents a basic building block of a material group.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    public abstract class MaterialPart : Nine.Object
    {
        [Nine.Serialization.BinarySerializable]
        internal string ParameterSuffix;

        /// <summary>
        /// Gets the EffectParts from the parent LinkedEffectContent.
        /// </summary>
        public MaterialGroup MaterialGroup { get; internal set; }

        /// <summary>
        /// Called when this material part is bound to a material group.
        /// </summary>
        protected internal abstract void OnBind();
        
        /// <summary>
        /// Gets the shader code for this material part based on material usage.
        /// </summary>
        protected internal abstract string GetShaderCode(MaterialUsage usage);

        /// <summary>
        /// Puts the dependent parts into the result list.
        /// </summary>
        protected internal virtual void GetDependentParts(MaterialUsage usage, IList<Type> result) { }

        /// <summary>
        /// Puts the dependent textures into the result list.
        /// </summary>
        protected internal virtual void GetDependentPasses(ICollection<Type> passTypes) { }

        /// <summary>
        /// Sets the texture based on the texture usage.
        /// </summary>
        public virtual void SetTexture(TextureUsage textureUsage, Texture texture) { }

        /// <summary>
        /// Applies all the global shader parameters before drawing any primitives.
        /// </summary>
        protected internal virtual void ApplyGlobalParameters(DrawingContext3D context) { }

        /// <summary>
        /// Applies all the local shader parameters before drawing any primitives.
        /// </summary>
        protected internal virtual void BeginApplyLocalParameters(DrawingContext3D context, MaterialGroup material) { }

        /// <summary>
        /// Restores any local shader parameters changes after drawing the promitive.
        /// </summary>
        protected internal virtual void EndApplyLocalParameters(DrawingContext3D context) { }

        /// <summary>
        /// Gets the material with the specified usage that is attached to this material.
        /// </summary>
        protected internal virtual void OnResolveMaterialPart(MaterialUsage usage, MaterialPart existingInstance) { }

        /// <summary>
        /// Notifies the material system that the shader has changed and needs recompile.
        /// </summary>
        protected void NotifyShaderChanged()
        {
            MaterialGroup.OnShaderChanged();
        }

        /// <summary>
        /// Copies data from an existing object to this object. 
        /// </summary>
        protected internal abstract MaterialPart Clone();

        /// <summary>
        /// Gets the EffectParameter with the name from the fragment parameter name.
        /// </summary>
        protected EffectParameter GetParameter(string parameterName)
        {
            return MaterialGroup.Effect.Parameters[string.Concat(parameterName, ParameterSuffix)];
        }

        /// <summary>
        /// Gets the index of the sampler with the name from the fragment parameter name.
        /// </summary>
        /// <returns>
        /// The index of the sampler, or a negative value if the parameter is not found.
        /// </returns>
        protected void GetTextureParameter(string parameterName, out EffectParameter parameter, out int index)
        {
            var parameters = MaterialGroup.Effect.Parameters;
            parameter = null;
            index = -1;

            parameter = parameters[string.Concat(parameterName, ParameterSuffix)];
            if (parameter == null)
                return;
            if (!IsTexture(parameter))
            {
                parameter = null;
                return;
            }
            
            for (int i = 0; i < parameters.Count; ++i)
            {
                if (IsTexture(parameters[i]))
                    index++;
                if (parameters[i] == parameter)
                    return;
            }

            // If we cannot find the parameter, something must went wrong.
            throw new InvalidOperationException();
        }

        private bool IsTexture(EffectParameter parameter)
        {
            return parameter.ParameterClass == EffectParameterClass.Object && (
                   parameter.ParameterType == EffectParameterType.Texture ||
                   parameter.ParameterType == EffectParameterType.Texture1D ||
                   parameter.ParameterType == EffectParameterType.Texture2D ||
                   parameter.ParameterType == EffectParameterType.Texture3D ||
                   parameter.ParameterType == EffectParameterType.TextureCube);
        }

        /// <summary>
        /// Gets the EffectParameter by semantic from the fragment parameter name.
        /// </summary>
        protected EffectParameter GetParameterBySemantic(string semantic)
        {
            foreach (EffectParameter parameter in MaterialGroup.Effect.Parameters)
                if (parameter.Semantic == semantic && parameter.Name.EndsWith(ParameterSuffix))
                    return parameter;
            return null;
        }

        #region GetShaderCode
        internal static bool IsContentRead = false;

        /// <summary>
        /// Gets the shader coder from the content pipeline assembly.
        /// </summary>
        internal static string GetShaderCode(string resourceKey)
        {
            return System.Text.Encoding.UTF8.GetString(
                MaterialGroup.TryInvokeContentPipelineMethod<byte[]>(
                "MaterialPartShaders", "get_" + resourceKey));
        }
        #endregion
    }
}