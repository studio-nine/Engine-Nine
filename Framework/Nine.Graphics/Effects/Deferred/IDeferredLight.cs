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
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ScreenEffects;
#endregion

namespace Nine.Graphics.Effects.Deferred
{
    /// <summary>
    /// Defines a light used by deferred rendering.
    /// </summary>
    public interface IDeferredLight
    {
        /// <summary>
        /// Gets the effect used to draw the light geometry.
        /// </summary>
        Effect Effect { get; }

        /// <summary>
        /// Gets the vertex buffer of the light geometry.
        /// </summary>
        VertexBuffer VertexBuffer { get; }

        /// <summary>
        /// Gets the index buffer of the light geometry.
        /// </summary>
        IndexBuffer IndexBuffer { get; }
    }
}
