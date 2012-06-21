#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Components;
using Nine.Studio;
using Nine.Studio.Extensibility;
using Nine.Studio.Visualizers;
#endregion

namespace Nine.Graphics.ParticleEffects.Design
{
    [Export(typeof(IFactory))]
    [LocalizedCategory("Graphics")]
    [LocalizedDisplayName("ParticleEffect")]
    public class ParticleEffectFactory : Factory<ParticleEffect, object>
    {

    }

    [Default]
    [Export(typeof(IVisualizer))]
    public class ParticleEffectVisualizer : GraphicsVisualizer<ParticleEffect>
    {
        [EditorBrowsable()]
        [LocalizedDisplayName("ShowWireframe")]
        public bool ShowWireframe { get; set; }

        public ParticleEffectVisualizer()
        {
            DisplayName = string.Format(Strings.ViewFormat, Strings.ParticleEffect);
        }

        protected override void Draw(TimeSpan elapsedTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
        }
    }
}
