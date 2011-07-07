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
using System.Collections;
using System.Xml;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Reflection;
#endregion

namespace Nine
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Extensions
    {
        /// <summary>
        /// Creates a new instance of the target type using the specified template name.
        /// </summary>
        public static T Create<T>(this ITemplateFactory factory, string templateName)
        {
            return (T)factory.Create(typeof(T), templateName);
        }
    }
}