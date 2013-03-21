namespace Nine.Graphics.Materials
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Represents a basic building block of a material group.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    [ContentProperty("ShaderCode")]    
    public class CustomMaterialPart : MaterialPart, IEffectParameterProvider
    {
        /// <summary>
        /// Gets or sets the usages of the default shader code.
        /// </summary>
        [Nine.Serialization.NotBinarySerializable]
        public string ShaderUsages
        {
            get { return usages; }
            set 
            {
                if (!string.IsNullOrEmpty(usages = value))
                    shaderUsages = value.Split(',').Select(str => (MaterialUsage)Enum.Parse(typeof(MaterialUsage), str.Trim(), false)).ToArray();
            }
        }
        private string usages;
        private MaterialUsage[] shaderUsages;

        /// <summary>
        /// Gets or sets the shader code when material usage is default.
        /// </summary>
        [Nine.Serialization.NotBinarySerializable]
        public string ShaderCode { get; set; }

        /// <summary>
        /// Gets a dictionary containing all the HLSL shader code with different material usage.
        /// </summary>
        [Nine.Serialization.NotBinarySerializable]
        public IDictionary<MaterialUsage, string> ShaderCodes
        {
            get { return shaderCodes ?? (shaderCodes = new Dictionary<MaterialUsage, string>()); }
        }
        private IDictionary<MaterialUsage, string> shaderCodes;

        /// <summary>
        /// Gets the parameters unique to this custom material instance.
        /// </summary>
        public CustomMaterialParameterCollection Parameters
        {
            get { return parameters; }
        }
        private CustomMaterialParameterCollection parameters = new CustomMaterialParameterCollection();

        /// <summary>
        /// Called when this material part is bound to a material group.
        /// </summary>
        protected internal override void OnBind()
        {
            parameters.Bind(this);
        }

        /// <summary>
        /// Applies all the global shader parameters before drawing any primitives.
        /// </summary>
        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            parameters.ApplyGlobalParameters(context, MaterialGroup);
        }

        /// <summary>
        /// Applies all the local shader parameters before drawing any primitives.
        /// </summary>
        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            parameters.BeginApplyLocalParameters(context, MaterialGroup);
        }

        /// <summary>
        /// Restores any local shader parameters changes after drawing the promitive.
        /// </summary>
        protected internal override void EndApplyLocalParameters(DrawingContext context)
        {
            parameters.EndApplyLocalParameters();
        }

        /// <summary>
        /// Gets the shader code for this material part based on material usage.
        /// </summary>
        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            if (!string.IsNullOrEmpty(ShaderCode))
            {
                if (usage == MaterialUsage.Default)
                    return ShaderCode;
                if (shaderUsages != null && shaderUsages.Contains(usage))
                    return ShaderCode;
            }
            string code = null;
            if (shaderCodes != null)
                shaderCodes.TryGetValue(usage, out code);            
            return code;
        }

        /// <summary>
        /// Copies data from an existing object to this object.
        /// </summary>
        protected internal override MaterialPart Clone()
        {
            var result = new CustomMaterialPart();
            result.usages = usages;
            result.shaderUsages = shaderUsages;
            result.ShaderCode = ShaderCode;
            result.shaderCodes = ShaderCodes;
            result.parameters = parameters;
            return result;
        }

        #region IEffectParameterProvider
        IEnumerable<EffectParameter> IEffectParameterProvider.GetParameters()
        {
            if (MaterialGroup != null && MaterialGroup.Effect != null && ParameterSuffix != null)
            {
                foreach (EffectParameter parameter in MaterialGroup.Effect.Parameters)
                    if (parameter.Name.EndsWith(ParameterSuffix))
                        yield return parameter;
            }
        }

        EffectParameter IEffectParameterProvider.GetParameter(string name)
        {
            return GetParameter(name);
        }
        #endregion
    }
}