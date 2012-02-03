#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

#endregion

namespace Nine.Content.Pipeline.Graphics.Effects
{
    /// <summary>
    /// Defines basic building blocks for materials that supports lighting, shadowing and fog.
    /// </summary>
    public abstract class BasicLinkedMaterialContent
    {
        [DefaultValue(true)]
        [ContentSerializer(Optional = true)]
        public virtual bool AmbientLightEnabled { get; set; }

        [DefaultValue(1)]
        [ContentSerializer(Optional = true)]
        public virtual int DirectionalLightCount { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool FogEnabled { get; set; }

        [DefaultValue(true)]
        [ContentSerializer(Optional = true)]
        public virtual bool LightingEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual int PointLightCount { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool PreferDeferredLighting { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual float ShadowBias { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual Vector3 ShadowColor { get; set; }

        [ContentSerializer(Optional=true)]
        public virtual bool ShadowEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual int ShadowMapSampleCount { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual int SpotLightCount { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool VertexColorEnabled { get; set; }

        public BasicLinkedMaterialContent()
        {
            AmbientLightEnabled = true;
            LightingEnabled = true;
            DirectionalLightCount = 1;
            ShadowMapSampleCount = 10;
            ShadowColor = Vector3.One * 0.5f;
            ShadowBias = 0.005f;
        }
    }
}
