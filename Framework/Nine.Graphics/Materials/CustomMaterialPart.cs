#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Drawing;
#if SILVERLIGHT
using Effect = Microsoft.Xna.Framework.Graphics.SilverlightEffect;
using EffectParameter = Microsoft.Xna.Framework.Graphics.SilverlightEffectParameter;
using EffectParameterCollection = Microsoft.Xna.Framework.Graphics.SilverlightEffectParametersCollection;
#endif
#endregion

namespace Nine.Graphics.Materials
{
    /// <summary>
    /// Represents a basic building block of a material group.
    /// </summary>
    [ContentSerializable]
    [ContentProperty("DefaultShaderCode")]    
    public class CustomMaterialPart : MaterialPart, IEffectParameterProvider
    {
        /// <summary>
        /// Gets or sets the shader code when material usage is default.
        /// </summary>
        public string DefaultShaderCode { get; set; }

        /// <summary>
        /// Gets a dictionary containing all the HLSL shader code with different material usage.
        /// </summary>
        [ContentSerializerIgnore]
        public IDictionary<MaterialUsage, string> ShaderCodes
        {
            get { return shaderCodes ?? (shaderCodes = new Dictionary<MaterialUsage, string>()); }
        }
        private IDictionary<MaterialUsage, string> shaderCodes;

        /// <summary>
        /// Gets the parameters unique to this custom material instance.
        /// </summary>
        [ContentSerializerIgnore]
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
        protected internal override void EndApplyLocalParameters()
        {
            parameters.EndApplyLocalParameters();
        }

        /// <summary>
        /// Gets the shader code for this material part based on material usage.
        /// </summary>
        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            if (usage == MaterialUsage.Default && !string.IsNullOrEmpty(DefaultShaderCode))
                return DefaultShaderCode;
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
            return new CustomMaterialPart();
        }

        #region IEffectParameterProvider
        IEnumerable<EffectParameter> IEffectParameterProvider.GetParameters()
        {
            if (MaterialGroup != null && MaterialGroup.Effect != null)
            {
                var suffix = string.Concat("_", ParameterSuffix);
                foreach (EffectParameter parameter in MaterialGroup.Effect.Parameters)
                    if (parameter.Name.EndsWith(suffix))
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