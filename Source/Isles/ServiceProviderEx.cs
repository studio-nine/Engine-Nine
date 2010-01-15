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


namespace Isles
{
    public interface IServiceProviderEx
    {
        T GetService<T>(string name);

        void AddService<T>(string name, T service);
    }


    public class ServiceProviderEx : IServiceProviderEx
    {
        struct Entry
        {
            public string Name;
            public Type Type;
            public object Value;
        }

        List<Entry> services = new List<Entry>();
        

        public T GetService<T>(string name)
        {
            if (name == null)
                name = "";

            foreach (Entry entry in services)
                if (entry.Name == name && entry.Type == typeof(T))
                    return (T)entry.Value;

            return default(T);
        }

        public void AddService<T>(string name, T service)
        {
            if (name == null)
                name = "";

            // Ignore duplicated services
            foreach (Entry entry in services)
                if (entry.Name == name && entry.Type == typeof(T))
                    return;

            Entry e;

            e.Name = name;
            e.Type = typeof(T);
            e.Value = service;

            services.Add(e);
        }
    }
}