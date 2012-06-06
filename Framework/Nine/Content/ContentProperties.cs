#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework;
using System.Linq;
using System;
using System.ComponentModel;
#endregion

namespace Nine.Content
{
    /// <summary>
    /// Contains properties for content build.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ContentProperties
    {
        /// <summary>
        /// Determines whether we are currently building the content.
        /// </summary>
        public static bool IsContentBuild { get; internal set; }
    }
}