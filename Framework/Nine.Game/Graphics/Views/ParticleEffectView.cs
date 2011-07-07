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
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Views
{
    public class ParticleEffectView : IUpdateable, IDrawableView
    {
        [ContentSerializer(Optional = true)]
        public bool Visible { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 Position { get; set; }

        [ContentSerializer(Optional = true)]
        public ParticleEffect ParticleEffect { get; set; }

        public void Update(TimeSpan elapsedTime)
        {
            if (ParticleEffect != null)
                ParticleEffect.Update(elapsedTime);
        }

        public void Draw(GraphicsContext context)
        {
            if (ParticleEffect == null || Visible)
            {   
                context.ParticleBatch.Draw(ParticleEffect);
            }
        }

        public void Draw(GraphicsContext context, Effect effect)
        {

        }
    }
}