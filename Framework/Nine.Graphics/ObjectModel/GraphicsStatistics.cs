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
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines commonly used statistics of the renderer.
    /// </summary>
    public class GraphicsStatistics
    {
        public int VisibleLightCount { get; internal set; }
        public int VisibleObjectCount { get; internal set; }
        public int OpaqueObjectCount { get; internal set; }
        public int TransparentObjectCount { get; internal set; }
        public int LightCount { get; internal set; }
        public int FrameRate { get; internal set; }

        internal GraphicsStatistics()
        {
            Reset();
        }

        internal void Reset()
        {
            VisibleLightCount = 0;
            VisibleObjectCount = 0;
            TransparentObjectCount = 0;
            OpaqueObjectCount = 0;
        }
    }
}