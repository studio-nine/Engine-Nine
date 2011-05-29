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

namespace Nine.Graphics
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DrawEventArgs : EventArgs
    {
        internal DrawEventArgs() { }

        public GraphicsDevice GraphicsDevice { get; internal set; }
        public SpriteBatch SpriteBatch { get; internal set; }
        public ModelBatch ModelBatch { get; internal set; }
        public PrimitiveBatch PrimitiveBatch { get; internal set; }
        public ParticleBatch ParticleBatch { get; internal set; }
        public TimeSpan ElapsedTime { get; internal set; }
        public TimeSpan TotalTime { get; internal set; }
        public bool IsRunningSlowly { get; internal set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DrawOverrideEventArgs : DrawEventArgs
    {
        internal DrawOverrideEventArgs() { }

        public Effect Effect { get; internal set; }
    }
}
