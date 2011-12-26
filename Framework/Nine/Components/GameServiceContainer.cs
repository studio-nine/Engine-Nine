#region File Description
//-----------------------------------------------------------------------------
// DrawableGameComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public class GameServiceContainer : IServiceProvider
    {
        Dictionary<Type, object> services = new Dictionary<Type, object>();

        public void AddService(Type type, object provider)
        {
            services.Add(type, provider);
        }

        public void RemoveService(Type type)
        {
            services.Remove(type);
        }

        public object GetService(Type serviceType)
        {
            object result;
            if (services.TryGetValue(serviceType, out result))
                return result;
            return null;
        }
    }
}
