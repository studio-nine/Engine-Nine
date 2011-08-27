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
    /// Represents a drawable object that can be rendered using the renderer.
    /// </summary>
    public interface IDrawableObject
    {
        /// <summary>
        /// Gets whether this object is visible.
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// Gets the material of the object.
        /// A value of null indicates the object does not have any materials
        /// for external use.
        /// </summary>
        Material Material { get; }

        /// <summary>
        /// Perform any updates before this object is drawed.
        /// </summary>
        void BeginDraw(GraphicsContext context);

        /// <summary>
        /// Perform any updates after this object is drawed.
        /// </summary>
        void EndDraw(GraphicsContext context);

        /// <summary>
        /// Draws the object using the graphics context.
        /// </summary>
        void Draw(GraphicsContext context);

        /// <summary>
        /// Draws the object with the specified effect.
        /// </summary>
        void Draw(GraphicsContext context, Effect effect);
    }

    /// <summary>
    /// Defines an interface for objects that receives lights and shadows.
    /// </summary>
    public interface ILightable
    {
        /// <summary>
        /// Gets whether lighting is enabled on this drawable.
        /// </summary>
        bool LightingEnabled { get; }

        /// <summary>
        /// Gets whether the lighting system should draw multi-pass lighting
        /// overlays on to this object.
        /// </summary>
        bool MultiPassLightingEnabled { get; }

        /// <summary>
        /// Gets the max number of affecting lights.
        /// </summary>
        int MaxAffectingLights { get; }

        /// <summary>
        /// Gets whether the drawable casts shadow.
        /// </summary>
        bool CastShadow { get; }

        /// <summary>
        /// Gets whether the drawable receives shadow.
        /// </summary>
        bool ReceiveShadow { get; }

        /// <summary>
        /// Gets whether the lighting system should draw multi-pass shadow
        /// overlays on to this object.
        /// </summary>
        bool MultiPassShadowEnabled { get; }

        /// <summary>
        /// Gets the max number of received shadows.
        /// </summary>
        int MaxReceivedShadows { get; }

        /// <summary>
        /// Gets or sets the data used by the lighting and shadowing system.
        /// </summary>
        object LightingData { get; set; }
    }
}