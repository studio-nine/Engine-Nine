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
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Nine.Graphics
{
    class GraphicsResources<T> where T : class
    {
        static Dictionary<GraphicsDevice, WeakReference<T>> resourceDictionary = new Dictionary<GraphicsDevice, WeakReference<T>>();

        public static T GetInstance(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            T result;
            WeakReference<T> value;

            if (resourceDictionary.TryGetValue(graphics, out value))
            {
                result = value.Target;
                if (result != null)
                    return result;
                
                // Need to remove it from the dictionary
                resourceDictionary.Remove(graphics);
            }
            
            result = (T)Activator.CreateInstance(typeof(T), graphics);
            resourceDictionary.Add(graphics, new WeakReference<T>(result));
            return result;
        }
    }
}
