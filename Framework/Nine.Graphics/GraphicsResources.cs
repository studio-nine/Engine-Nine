#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    class GraphicsResources<T>
    {
        static Dictionary<GraphicsDevice, T> resourceDictionary;

        public static T GetInstance(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            T value;

            if (resourceDictionary == null)
                resourceDictionary = new Dictionary<GraphicsDevice, T>();

            if (resourceDictionary.TryGetValue(graphics, out value))
                return value;

            value = (T)Activator.CreateInstance(typeof(T), graphics);
            resourceDictionary.Add(graphics, value);

            return value;
        }
    }
}
