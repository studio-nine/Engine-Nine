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
using Nine.Graphics;

using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using System.Text;
#endregion

namespace Nine
{
    class EmptyServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return null;
        }
    }

    class GraphicsDeviceServiceProvider : IServiceProvider, IGraphicsDeviceService, IGraphicsDeviceManager
    {
        GraphicsDevice graphics;

        public GraphicsDeviceServiceProvider(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this.graphics = graphics;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IGraphicsDeviceService) ||
                serviceType == typeof(IGraphicsDeviceManager))
            {
                return this;
            }
            return null;
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

        public void CreateDevice()
        {

        }

        public void EndDraw()
        {

        }
    }
}