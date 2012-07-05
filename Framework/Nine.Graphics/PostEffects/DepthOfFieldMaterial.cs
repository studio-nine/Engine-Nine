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
using System.ComponentModel;
#endregion

namespace Nine.Graphics.Materials
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class DepthOfFieldMaterial : ISceneObject
    {
        public float FocalLength { get; set; }
        public float FocalPlane { get; set; }
        public float FocalDistance { get; set; }

        partial void OnCreated()
        {
            FocalDistance = 0.5f;
        }

        partial void ApplyGlobalParameters(DrawingContext context)
        {
            
        }

        partial void BeginApplyLocalParameters(DrawingContext context, DepthOfFieldMaterial previousMaterial)
        {
            effect.FocalDistance.SetValue(FocalDistance);
            effect.FocalLength.SetValue(FocalLength);
            effect.FocalPlane.SetValue(FocalPlane);
        }

        public override void SetTexture(TextureUsage textureUsage, Texture texture)
        {
            if (textureUsage == TextureUsage.Blur)
                effect.BlurTexture.SetValue(texture);
            else if (textureUsage == TextureUsage.DepthBuffer)
                effect.DepthTexture.SetValue(texture);
        }

        void ISceneObject.OnAdded(DrawingContext context)
        {
            //context.MainPass.Passes.Add(new BasicPass());
        }

        void ISceneObject.OnRemoved(DrawingContext context)
        {

        }
    }
}