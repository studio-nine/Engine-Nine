namespace Nine.Graphics.Materials.MaterialParts
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Defines a material part that provides normals from normal maps
    /// </summary>
    public class NormalMapMaterialPart : MaterialPart
    {      
        private EffectParameter textureParameter;

        /// <summary>
        /// Gets or sets the normal map.
        /// </summary>
        public Texture2D NormalMap { get; set; }

        protected internal override void OnBind()
        {
            textureParameter = GetParameter("Texture");
        }

        protected internal override void GetDependentParts(MaterialUsage usage, IList<Type> result)
        {
            result.Add(typeof(TangentTransformMaterialPart));
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            if (textureParameter != null)
                textureParameter.SetValue(NormalMap);
        }

        protected internal override MaterialPart Clone()
        {
            return new NormalMapMaterialPart() { NormalMap = this.NormalMap };
        }

        protected internal override void OnResolveMaterialPart(MaterialUsage usage, MaterialPart existingInstance)
        {
            var part = ((NormalMapMaterialPart)existingInstance);
            part.NormalMap = this.NormalMap;
        }

        public override void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.NormalMap)
                NormalMap = texture as Texture2D;
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return (usage == MaterialUsage.Default || usage == MaterialUsage.DepthAndNormal || usage == MaterialUsage.Normal) ? GetShaderCode("NormalMap") : null;
        }
    }
}
