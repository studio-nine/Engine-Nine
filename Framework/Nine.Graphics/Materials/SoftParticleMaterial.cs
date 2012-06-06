#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.Primitives;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.Materials
{
    [NotContentSerializable]
    partial class SoftParticleMaterial
    {
        public float DepthFade
        {
            get { return depthFade.HasValue ? depthFade.Value : MaterialConstants.SoftParticleFade; }
            set { depthFade = (value == MaterialConstants.SoftParticleFade ? (float?)null : value); }
        }
        private float? depthFade;

        partial void ApplyGlobalParameters(DrawingContext context)
        {
            effect.Projection.SetValue(context.Projection);
            effect.projectionInverse.SetValue(context.matrices.ProjectionInverse);
            effect.DepthBuffer.SetValue(context.textures[TextureUsage.DepthBuffer]);
        }

        partial void BeginApplyLocalParameters(DrawingContext context, SoftParticleMaterial previousMaterial)
        {
            effect.Texture.SetValue(Texture);
            if (depthFade.HasValue)
                effect.DepthFade.SetValue(depthFade.Value);
        }

        partial void EndApplyLocalParameters()
        {
            if (depthFade.HasValue)
                effect.DepthFade.SetValue(MaterialConstants.SoftParticleFade);
        }
    }
}
