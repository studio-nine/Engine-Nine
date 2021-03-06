namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    public class BasicMaterial : Material
    {
        #region Properties
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Vector3 DiffuseColor
        {
            get { return diffuseColor.HasValue ? diffuseColor.Value : Constants.DiffuseColor; }
            set { diffuseColor = (value == Constants.DiffuseColor ? (Vector3?)null : value); }
        }
        internal Vector3? diffuseColor;

        public Vector3 EmissiveColor
        {
            get { return emissiveColor.HasValue ? emissiveColor.Value : Constants.EmissiveColor; }
            set { emissiveColor = (value == Constants.EmissiveColor ? (Vector3?)null : value); }
        }
        internal Vector3? emissiveColor;

        public Vector3 SpecularColor
        {
            get { return specularColor.HasValue ? specularColor.Value : Constants.SpecularColor; }
            set { specularColor = (value == Constants.SpecularColor ? (Vector3?)null : value); }
        }
        internal Vector3? specularColor;
        
        public float SpecularPower
        {
            get { return specularPower.HasValue ? specularPower.Value : Constants.SpecularPower; }
            set { specularPower = (value == Constants.SpecularPower ? (float?)null : value); }
        }
        private float? specularPower;

        public bool VertexColorEnabled
        {
            get { return vertexColorEnabled; }
            set { vertexColorEnabled = value; }
        }
        private bool vertexColorEnabled;

        public bool LightingEnabled
        {
            get { return lightingEnabled; }
            set { lightingEnabled = value;}
        }
        private bool lightingEnabled;

        public bool PreferPerPixelLighting
        {
            get { return preferPerPixelLighting; }
            set { preferPerPixelLighting = value; }
        }
        private bool preferPerPixelLighting;

#if WINDOWS
        [TypeConverter(typeof(Nine.Graphics.Design.SamplerStateConverter))]
#endif
        public SamplerState SamplerState
        {
            get { return samplerState; }
            set { samplerState = value; }
        }
        private SamplerState samplerState;
        #endregion

        #region Fields
        private BasicEffect effect;
        private EffectPass pass;

        private static Texture2D previousTexture;
        #endregion

        #region Methods
        public BasicMaterial(GraphicsDevice graphics)
        {
            effect = GraphicsResources<BasicEffect>.GetInstance(graphics, typeof(BasicMaterial));
            pass = effect.CurrentTechnique.Passes[0];
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

        protected override void OnBeginApply(DrawingContext context, Material previousMaterial)
        {
            // Check if the previous material used is a also a BasicMaterial. If so, we can
            // guarantee that the current shader has all the correct global parameters, and
            // there's no need to set them again.
            var previousBasicMaterial = previousMaterial as BasicMaterial;
            if (previousBasicMaterial == null || previousBasicMaterial.lightingEnabled != lightingEnabled)
            {
                // Update parameters that are global and shared between different instances.
                // This includes view projection matrices and global directional lights.
                effect.View = context.View;
                effect.Projection = context.Projection;

                var context3D = context as DrawingContext3D;
                if (lightingEnabled)
                    MaterialHelper.ApplyLights(context3D, effect);
                MaterialHelper.ApplyFog(context3D, effect);
            }
            
            // Update per instance shader parameters only when the value does not equal to
            // the default one. Remember to reset it back to defaults during EndApply.
            // Since most objects will have their materials left at the default state, 
            // most of these parameter won't be updated to the shader.
            if (alpha != Constants.Alpha)
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
            if (previousBasicMaterial == null || texture != previousTexture)
                previousTexture = effect.Texture = texture;

            // Update shader parameters that are always different for each instance.
            effect.World = world;
            
            // Update shader parameters that has little or no overhead.
            effect.TextureEnabled = texture != null;
            effect.LightingEnabled = lightingEnabled;
            effect.PreferPerPixelLighting = preferPerPixelLighting;
            effect.VertexColorEnabled = vertexColorEnabled;
            
            // Finally apply the shader.
            pass.Apply();

            // Update sampler state
            if (samplerState != null)
                context.graphics.SamplerStates[0] = samplerState;
        }

        protected override void OnEndApply(DrawingContext context)
        {
            if (alpha != Constants.Alpha)
                effect.Alpha = Constants.Alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = Constants.DiffuseColor;
            if (emissiveColor.HasValue)
                effect.EmissiveColor = Constants.EmissiveColor;
            if (specularColor.HasValue)
                effect.SpecularColor = Constants.SpecularColor;
            if (specularPower.HasValue)
                effect.SpecularPower = Constants.SpecularPower;

            if (samplerState != null)
                GraphicsDevice.SamplerStates[0] = context.SamplerState;
        }

        protected override Material OnResolveMaterial(MaterialUsage usage, Material existingInstance)
        {
#if !WINDOWS_PHONE
            if (usage == MaterialUsage.Depth)
            {
                var result = (existingInstance as DepthMaterial) ?? new DepthMaterial(GraphicsDevice);
                result.AlphaTestEnabled = (texture != null && IsTransparent);
                return result;
            }

            if (usage == MaterialUsage.DepthAndNormal)
            {
                var result = (existingInstance as DepthAndNormalMaterial) ?? new DepthAndNormalMaterial(GraphicsDevice);
                result.specularPower = specularPower;
                return result;
            }
#endif
            return null;
        }
        #endregion
    }
}