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
#endregion

namespace Nine
{
    public class ServiceProvider : Collection<object>, IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            for (int i = 0; i < Count; i++)
            {
                if (serviceType.IsAssignableFrom(this[i].GetType()))
                    return this[i];
            }
            return null;
        }
    }

    class GraphicsDeviceServiceProvider : IGraphicsDeviceService, IGraphicsDeviceManager
    {
        GraphicsDevice graphics;

        public GraphicsDeviceServiceProvider(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this.graphics = graphics;
        }

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        public GraphicsDevice GraphicsDevice
        {
            get { return graphics; }
        }

        public bool BeginDraw()
        {
            return true;
        }

        public void CreateDevice() { }
        public void EndDraw() { }
    }
}
