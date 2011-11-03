#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines an area of fog.
    /// </summary>
    [ContentSerializable]
    public class Fog : Transformable, IEffectFog
    {
        public float FogStart { get; set; }
        public float FogEnd { get; set; }
        public Vector3 FogColor { get; set; }
        public bool FogEnabled { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fog"/> class.
        /// </summary>
        public Fog()
        {
            FogStart = 1000;
            FogEnd = 10000;
            FogEnabled = true;
            FogColor = Vector3.One;
        }
    }
}