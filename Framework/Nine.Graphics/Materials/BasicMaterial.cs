#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ObjectModel;
using DirectionalLight = Nine.Graphics.ObjectModel.DirectionalLight;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.Materials
{
    [ContentSerializable]
    public class BasicMaterial : Material
    {
        #region Properties
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Vector3 DiffuseColor
        {
            get { return diffuseColor.HasValue ? diffuseColor.Value : MaterialConstants.DiffuseColor; }
            set { diffuseColor = (value == MaterialConstants.DiffuseColor ? (Vector3?)null : value); }
        }
        private Vector3? diffuseColor;

        public Vector3 EmissiveColor
        {
            get { return emissiveColor.HasValue ? emissiveColor.Value : MaterialConstants.EmissiveColor; }
            set { emissiveColor = (value == MaterialConstants.EmissiveColor ? (Vector3?)null : value); }
        }
        private Vector3? emissiveColor;

        public Vector3 SpecularColor
        {
            get { return specularColor.HasValue ? specularColor.Value : MaterialConstants.SpecularColor; }
            set { specularColor = (value == MaterialConstants.SpecularColor ? (Vector3?)null : value); }
        }
        private Vector3? specularColor;
        
        public float SpecularPower
        {
            get { return specularPower.HasValue ? specularPower.Value : MaterialConstants.SpecularPower; }
            set { specularPower = (value == MaterialConstants.SpecularPower ? (float?)null : value); }
        }
        private float? specularPower;

        public bool VertexColorEnabled { get; set; }
        public bool LightingEnabled { get; set; }
        public bool PreferPerPixelLighting { get; set; }
        #endregion

        #region Fields
        private BasicEffect effect;
        private MaterialLightHelper lightHelper;
        private MaterialFogHelper fogHelper;

        private static Texture2D previousTexture;
        #endregion

        #region Methods
        public BasicMaterial(GraphicsDevice graphics)
        {
            effect = GraphicsResources<BasicEffect>.GetInstance(graphics);
            GraphicsDevice = graphics;
        }

        public override T Find<T>()
        {
            if (typeof(T) == typeof(IEffectMatrices) || typeof(T) == typeof(IEffectFog))
            {
                return effect as T;
            }
            return base.Find<T>();
        }

        public override void BeginApply(DrawingContext context)
        {
            // Check if the previous material used is a also a BasicMaterial. If so, we can
            // guarantee that the current shader has all the correct global parameters, and
            // there's no need to set them again.
            var previousBasicMaterial = context.PreviousMaterial as BasicMaterial;
            if (previousBasicMaterial == null)
            {
                // Update parameters that are global and shared between different instances.
                // This includes view projection matrices and global directional lights.
                effect.View = context.View;
                effect.Projection = context.Projection;

                if (LightingEnabled)
                    lightHelper.Apply(context, effect);
                fogHelper.Apply(context, effect);
            }
            
            // Update per instance shader parameters only when the value does not equal to
            // the default one. Remember to reset it back to defaults during EndApply.
            // Since most objects will have their materials left at the default state, 
            // most of these parameter won't be updated to the shader.
            if (alpha != MaterialConstants.Alpha)
                effect.Alpha = alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = diffuseColor.Value;
            if (emissiveColor.HasValue)
                effect.EmissiveColor = emissiveColor.Value;
            if (specularColor.HasValue)
                effect.SpecularColor = specularColor.Value;
            if (specularPower.HasValue)
                effect.SpecularPower = specularPower.Value;
            
            // Setting the texture parameter has a small overhead, so compare it upfront.
            if (previousBasicMaterial == null || Texture != previousTexture)
                previousTexture = effect.Texture = Texture;

            // Update shader parameters that are always different for each instance.
            effect.World = World;

            // Update shader parameters that has little or no overhead.
            effect.TextureEnabled = Texture != null;
            effect.LightingEnabled = LightingEnabled;
            effect.PreferPerPixelLighting = PreferPerPixelLighting;
            effect.VertexColorEnabled = VertexColorEnabled;

            // Finally apply the shader.
            effect.CurrentTechnique.Passes[0].Apply();
        }

        public override void EndApply(DrawingContext context)
        {
            if (alpha != MaterialConstants.Alpha)
                effect.Alpha = MaterialConstants.Alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = MaterialConstants.DiffuseColor;
            if (emissiveColor.HasValue)
                effect.EmissiveColor = MaterialConstants.EmissiveColor;
            if (specularColor.HasValue)
                effect.SpecularColor = MaterialConstants.SpecularColor;
            if (specularPower.HasValue)
                effect.SpecularPower = MaterialConstants.SpecularPower;
        }

        protected override Material OnResolveMaterial(MaterialUsage usage)
        {
            if (usage == MaterialUsage.Depth)
                return new DepthMaterial(GraphicsDevice) { TextureEnabled = Texture != null && IsTransparent };
            return null;
        }
        #endregion
    }
}