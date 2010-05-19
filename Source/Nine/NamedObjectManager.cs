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
    public interface INamedObjectManager
    {
        object GetObjectByName(string name);
    }


    public class NamedObjectManager : INamedObjectManager
    {
        Dictionary<string, object> registry = new Dictionary<string,object>(32);

        public void Add(string name, object value)
        {
            registry.Add(name, value);
        }

        public void Clear()
        {
            registry.Clear();
        }

        public object GetObjectByName(string name)
        {
            object result;

            registry.TryGetValue(name, out result);

            return result;
        }
    }
}