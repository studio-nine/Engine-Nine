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
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Effects
{
    /// <summary>
    /// Defines a list of effect parameter semantics supported by the rendering system.
    /// </summary>
    /// <remarks>
    /// Prefix the enum value with "Sas", "SasEffect" or "SasUi" is also supported.
    /// </remarks>
    enum EffectAnnotations
    {
        BindAddress,
        ResourceName,
        Function,
        Dimensions,
    }
}