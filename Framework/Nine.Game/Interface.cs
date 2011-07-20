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
using Nine.Graphics;
using System.Xml.Serialization;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a world object that has a template.
    /// </summary>
    public interface IWorldObject
    {
        /// <summary>
        /// Gets the transform of this object.
        /// </summary>
        Matrix Transform { get; }

        /// <summary>
        /// Gets the name of the visual template of this object.
        /// </summary>
        Template Template { get; }
    }
}