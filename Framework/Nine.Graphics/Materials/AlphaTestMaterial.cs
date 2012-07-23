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
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.Materials
{
    [ContentSerializable]
    public class AlphaTestMaterial : Material, IEffectTexture
    {
        #region Properties
        public Vector3 DiffuseColor
        {
            get { return diffuseColor.HasValue ? diffuseColor.Value : MaterialConstants.DiffuseColor; }
            set { diffuseColor = (value == MaterialConstants.DiffuseColor ? (Vector3?)null : value); }
        }
        private Vector3? diffuseColor;

        public int ReferenceAlpha
        {
            get { return referenceAlpha.HasValue ? referenceAlpha.Value : MaterialConstants.ReferenceAlpha; }
            set { referenceAlpha = (value == MaterialConstants.ReferenceAlpha ? (int?)null : value); }
        }
        private int? referenceAlpha;

        public CompareFunction AlphaFunction
        {
            get { return alphaFunction.HasValue ? alphaFunction.Value : MaterialConstants.AlphaFunction; }
            set { alphaFunction = (value == MaterialConstants.AlphaFunction ? (CompareFunction?)null : value); }
        }
        private CompareFunction? alphaFunction;

        public bool VertexColorEnabled { get; set; }
        #endregion

        #region Fields
        private AlphaTestEffect effect;
        private MaterialFogHelper fogHelper;

        private static Texture2D previousTexture;
        #endregion

        #region Methods
        public AlphaTestMaterial(GraphicsDevice graphics)
        {
            effect = GraphicsResources<AlphaTestEffect>.GetInstance(graphics);
            effect.ReferenceAlpha = MaterialConstants.ReferenceAlpha;
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
            var previousAlphaTestMaterial = previousMaterial as AlphaTestMaterial;
            if (previousAlphaTestMaterial == null)
            {
                effect.View = context.View;
                effect.Projection = context.Projection;

                fogHelper.Apply(context, effect);
            }

            if (alpha != MaterialConstants.Alpha)
                effect.Alpha = alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = diffuseColor.Value;
            if (referenceAlpha.HasValue)
                effect.ReferenceAlpha = referenceAlpha.Value;
            if (alphaFunction.HasValue)
                effect.AlphaFunction = alphaFunction.Value;

            if (previousAlphaTestMaterial == null || previousTexture != Texture)
                previousTexture = effect.Texture = Texture;
            
            effect.World = World;
            effect.VertexColorEnabled = VertexColorEnabled;

            effect.CurrentTechnique.Passes[0].Apply();
        }

        protected override void OnEndApply(DrawingContext context)
        {
            if (alpha != MaterialConstants.Alpha)
                effect.Alpha = MaterialConstants.Alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = MaterialConstants.DiffuseColor;
            if (referenceAlpha.HasValue)
                effect.ReferenceAlpha = MaterialConstants.ReferenceAlpha;
            if (alphaFunction.HasValue)
                effect.AlphaFunction = MaterialConstants.AlphaFunction;
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture) { }
        #endregion
    }
}