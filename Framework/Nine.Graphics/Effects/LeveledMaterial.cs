#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Effects
{
    /// <summary>
    /// Defines a material with level of details.
    /// </summary>
    [ContentSerializable]
    public class LeveledMaterial : Material
    {
        /// <summary>
        /// Gets the current material level.
        /// </summary>
        public Material CurrentMaterial { get; private set; }

        /// <summary>
        /// Gets a list of materials that are sorted from lowest quanlity to highest quanlity.
        /// </summary>
        public IList<Material> MaterialLevels { get; private set; }

        /// <summary>
        /// Gets or sets the distance at which to use the highest quanlity material.
        /// </summary>
        public float LevelOfDetailStart { get; set; }

        /// <summary>
        /// Gets or sets the distance at which to use the lowest quanlity material.
        /// </summary>
        public float LevelOfDetailEnd { get; set; }

        /// <summary>
        /// Gets or sets the overall material quality that is multiplied with the quanlity
        /// determined by LevelOfDetailStart and LevelOfDetailEnd.
        /// </summary>
        public float MaterialQuality { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeveledMaterial"/> class.
        /// </summary>
        public LeveledMaterial()
        {
            MaterialLevels = new List<Material>();
            LevelOfDetailStart = 100;
            LevelOfDetailEnd = 1000;
            MaterialQuality = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeveledMaterial"/> class.
        /// </summary>
        public LeveledMaterial(IEnumerable<Material> materialLevels) : this()
        {
            MaterialLevels.AddRange(materialLevels);
            UpdateLevelOfDetail(0);
        }
        
        /// <summary>
        /// Manually updates the level of detail of this leveled material.
        /// </summary>
        public void UpdateLevelOfDetail(float distanceToEye)
        {
            if (MaterialLevels.Count > 0)
            {
                float lod = 1 - (distanceToEye - LevelOfDetailStart) / (LevelOfDetailEnd - LevelOfDetailStart);
                lod = MathHelper.Clamp(MaterialQuality * lod, 0, 0.99f);
                Material candidate = MaterialLevels[(int)(MaterialLevels.Count * lod)];

                if (candidate != CurrentMaterial)
                {
                    var previousMaterial = CurrentMaterial;
                    CurrentMaterial = candidate;
                    DepthAlphaEnabled = CurrentMaterial.DepthAlphaEnabled;
                    TwoSided = CurrentMaterial.TwoSided;
                    OnMaterialChanged(previousMaterial);
                }
            }
            else
            {
                CurrentMaterial = null;
                DepthAlphaEnabled = false;
                TwoSided = false;
            }
        }

        /// <summary>
        /// Gets the underlying effect.
        /// </summary>
        public override Effect Effect
        {
            get { return CurrentMaterial != null ? CurrentMaterial.Effect : null; }
        }

        /// <summary>
        /// Gets the deferred effect used to generate the graphics buffer.
        /// If null is returned, the default graphics buffer effect is used.
        /// </summary>
        public override Effect GraphicsBufferEffect
        {
            get { return CurrentMaterial != null ? CurrentMaterial.GraphicsBufferEffect : null; }
        }

        /// <summary>
        /// Gets a value indicating whether this material uses deferred lighting.
        /// </summary>
        public override bool IsDeferred
        {
            get { return CurrentMaterial != null ? CurrentMaterial.IsDeferred : false; }
        }

        /// <summary>
        /// When overriden, returns whether the rendered object is transparent under
        /// current material settings.
        /// </summary>
        protected override bool IsTransparentValue
        {
            get { return CurrentMaterial != null ? CurrentMaterial.IsTransparent : false; }
        }

        /// <summary>
        /// Applys the parameter values to the underlying effect.
        /// </summary>
        public override void Apply()
        {
            if (CurrentMaterial != null)
                CurrentMaterial.Apply();
        }

        /// <summary>
        /// Queries the material for the specified interface T.
        /// </summary>
        public override T Find<T>()
        {
            return CurrentMaterial != null ? CurrentMaterial.Find<T>() : base.Find<T>();
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        public override Material Clone()
        {
            return new LeveledMaterial(MaterialLevels)
            {
                LevelOfDetailEnd = LevelOfDetailEnd,
                LevelOfDetailStart = LevelOfDetailStart,
                MaterialQuality = MaterialQuality,
            };
        }

        /// <summary>
        /// Called when a new material level is used.
        /// </summary>
        protected virtual void OnMaterialChanged(Material previousMaterial) { }
    }
}