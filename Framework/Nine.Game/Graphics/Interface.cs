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
using Nine.Graphics.Passes;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines a drawable that can be rendered by the render system.
    /// </summary>
    public interface IDrawableView
    {
        /// <summary>
        /// Draws this object using the provided context.
        /// </summary>
        void Draw(GraphicsContext context);

        /// <summary>
        /// Draws this object using the designated effect.
        /// </summary>
        void Draw(GraphicsContext context, Effect effect);
    }

    /// <summary>
    /// Defines an interface for a light used by the render system.
    /// </summary>
    public interface ILight
    {
        /// <summary>
        /// Gets whether the light is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Gets whether the light should cast a shadow.
        /// </summary>
        bool CastShadow { get; }

        /// <summary>
        /// Gets the order of this light when it's been process by the renderer.
        /// Light might be discarded when the max affecting lights are reached.
        /// </summary>
        float Order { get; }

        /// <summary>
        /// Gets the multi-pass lighting effect used to draw object.
        /// </summary>
        Effect Effect { get; }
        
        /// <summary>
        /// Lights the specified objects.
        /// </summary>
        void Light(ISpatialQuery<IDrawableView> drawables);

        /// <summary>
        /// Draws the depth map of the specified drawables.
        /// </summary>
        void DrawDepthMap(ISpatialQuery<IDrawableView> drawables);
    }

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
        /// Gets whether the drawable casts shadow.
        /// </summary>
        bool CastShadow { get; }

        /// <summary>
        /// Gets whether the drawable receives shadow.
        /// </summary>
        bool ReceiveShadow { get; }
        
        /// <summary>
        /// Gets the effect used by this material.
        /// </summary>
        Effect Effect { get; }
    }
}