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
    [NotContentSerializable]
    partial class SkyBoxMaterial
    {
        public Vector3 Color { get; set; }
        public new TextureCube Texture { get; set; }

        partial void OnCreated()
        {
            Color = Vector3.One;
        }

        partial void ApplyGlobalParameters(DrawingContext context)
        {
            Matrix positionIndependentView = context.View;
            positionIndependentView.Translation = Vector3.Zero;

            effect.worldViewProjection.SetValue(positionIndependentView * context.Projection);
        }

        partial void BeginApplyLocalParameters(DrawingContext context, SkyBoxMaterial previousMaterial)
        {
            effect.Color.SetValue(Color);
            effect.Texture.SetValue(Texture);
        }
    }
}