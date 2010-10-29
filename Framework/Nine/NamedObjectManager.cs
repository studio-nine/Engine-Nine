#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Nine
{
    /// <summary>
    /// Manages objects by name.
    /// </summary>
    public interface INamedObjectManager
    {
        /// <summary>
        /// Gets an object by name.
        /// </summary>
        object FindObject(string name);
    }
    
    /// <summary>
    /// Manages objects by name.
    /// </summary>
    public class NamedObjectManager : INamedObjectManager
    {
        Dictionary<string, object> registry = new Dictionary<string,object>(32);

        /// <summary>
        /// Adds an object with the specfied name.
        /// </summary>
        public void Add(string name, object value)
        {
            registry.Add(name, value);
        }

        /// <summary>
        /// Clear all objects managed by this instance.
        /// </summary>
        public void Clear()
        {
            registry.Clear();
        }

        /// <summary>
        /// Gets an object by name.
        /// </summary>
        public object FindObject(string name)
        {
            object result;

            registry.TryGetValue(name, out result);

            return result;
        }
    }
}