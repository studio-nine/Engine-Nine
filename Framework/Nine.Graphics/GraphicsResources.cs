namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Graphics;

    class GraphicsResources<T> where T : class
    {
        static T defaultInstance;
        static GraphicsDevice defaultGraphics;
        static Dictionary<GraphicsDevice, WeakReference<T>> resourceDictionary = new Dictionary<GraphicsDevice, WeakReference<T>>();

        public static T GetInstance(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (defaultGraphics == null)
                return defaultInstance = (T)Activator.CreateInstance(typeof(T), defaultGraphics = graphics);
            if (defaultGraphics == graphics)
                return defaultInstance;

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
