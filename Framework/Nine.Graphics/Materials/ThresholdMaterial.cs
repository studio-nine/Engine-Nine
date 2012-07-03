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
    public partial class ThresholdMaterial
    {
        public float Threshold { get; set; }

        partial void OnCreated()
        {
            Threshold = 0.5f;
        }

        partial void BeginApplyLocalParameters(DrawingContext context, ThresholdMaterial previousMaterial)
        {
            effect.Threshold.SetValue(Threshold);
        }
    }
}