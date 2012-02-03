#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics
{
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

#pragma warning disable 0067

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

#pragma warning restore 0067

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