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
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines an interface for material that can be assigned to each drawable view.
    /// </summary>
    public interface IMaterial
    {
        /// <summary>
        /// Gets whether the drawable is transparent.
        /// </summary>
        bool IsTransparent { get; }
        
        /// <summary>
        /// Gets the effect instance used by this material.
        /// </summary>
        IEffectInstance Effect { get; }
    }

    /// <summary>
    /// Defines an interface for objects that receives lights and shadows.
    /// </summary>
    public interface ILightable
    {
        /// <summary>
        /// Gets whether the drawable casts shadow.
        /// </summary>
        bool CastShadow { get; }

        /// <summary>
        /// Gets whether the drawable receives shadow.
        /// </summary>
        bool ReceiveShadow { get; }

        /// <summary>
        /// Gets the max number of affecting lights.
        /// </summary>
        //int MaxAffectingLights { get; }
    }
}