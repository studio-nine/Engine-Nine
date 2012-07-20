namespace Nine.Graphics
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    class GraphicsDeviceServiceProvider : IServiceProvider, IGraphicsDeviceService, IGraphicsDeviceManager
    {
        GraphicsDevice graphics;
        IServiceProvider serviceProvider;
        
        public GraphicsDeviceServiceProvider(GraphicsDevice graphics)
            : this(graphics, null)
        {

        }

        public GraphicsDeviceServiceProvider(GraphicsDevice graphics, IServiceProvider serviceProvider)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this.graphics = graphics;
            this.serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            var service = serviceProvider.GetService(serviceType);
            if (service != null)
                return service;

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