#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics.ParticleEffects;
#if !WINDOWS_PHONE
using Nine.Graphics.Effects;
#endif
#endregion

namespace Nine.Graphics.Passes
{
#if !WINDOWS_PHONE

    public class ShadowMapPass : GraphicsPass
    {
        private BasicDrawPass basicDrawPass;

        public ILight Light { get; set; }
        public ShadowMap ShadowMap { get; private set; }

        public override void Draw(GraphicsContext context, ISpatialQuery<object> drawables)
        {
            if (Light == null || !Light.CastShadow)
            {
                return;
            }
            if (ShadowMap == null)
            {
                ShadowMap = new ShadowMap(context.GraphicsDevice, 1024);
            }
            if (basicDrawPass == null)
            {
                basicDrawPass = new BasicDrawPass();
                basicDrawPass.DrawTransparentObjects = false;
                basicDrawPass.Effect = ShadowMap.Effect;
            }
            ShadowMap.Begin();
            basicDrawPass.Draw(context, drawables);
            ShadowMap.End();
        }
    }

#endif
}