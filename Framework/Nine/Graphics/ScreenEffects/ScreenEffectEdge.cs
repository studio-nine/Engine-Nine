#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ScreenEffects
{
    /// <summary>
    /// Defines an edge that connects two passes in a post processing effect graph.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ScreenEffectEdge
    {
        /// <summary>
        /// Gets or sets whether this edge is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the texture name used to connect to the next pass.
        /// </summary>
        public string TextureName { get; set; }

        /// <summary>
        /// Gets or sets the endpoint of this edge.
        /// </summary>
        public ScreenEffectPass Effect { get; set; }

        /// <summary>
        /// Creates a new instance of <c>ScreenEffectEdge</c>.
        /// </summary>
        public ScreenEffectEdge() { Enabled = true; }
    }

    /// <summary>
    /// Represents a collection of <c>ScreenEffectEdge</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ScreenEffectEdgeCollection : Collection<ScreenEffectEdge>
    {
        public void Add(Effect effect)
        {
            Add(new ScreenEffectEdge() { Effect = new ScreenEffectPass(effect.GraphicsDevice, effect) });
        }

        public void Add(Effect effect, string textureName)
        {
            Add(new ScreenEffectEdge() { Effect = new ScreenEffectPass(effect.GraphicsDevice, effect), TextureName = textureName });
        }
    }
}

