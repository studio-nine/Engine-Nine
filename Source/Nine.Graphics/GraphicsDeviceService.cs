// The IGraphicsDeviceService interface requires a DeviceCreated event, but we
// always just create the device inside our constructor, so we have no place to
// raise that event. The C# compiler warns us that the event is never used, but
// we don't care so we just disable this warning.
#pragma warning disable 67

namespace Nine.Graphics
{
    using System;
    using System.Threading;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework;

    #region GraphicsDeviceService
    /// <summary>
    /// Helper class responsible for creating and managing the GraphicsDevice.
    /// All GraphicsDeviceControl instances share the same GraphicsDeviceService,
    /// so even though there can be many controls, there will only ever be a single
    /// underlying GraphicsDevice. This implements the standard IGraphicsDeviceService
    /// interface, which provides notification events for when the device is reset
    /// or disposed.
    /// </summary>
    internal class GraphicsDeviceService : IGraphicsDeviceService, IServiceProvider
    {
        #region Fields


        // Singleton device service instance.
        static GraphicsDeviceService singletonInstance;


        // Keep track of how many controls are sharing the singletonInstance.
        static int referenceCount;
        #endregion


        /// <summary>
        /// Constructor is private, because this is a singleton class:
        /// client controls should use the public AddRef method instead.
        /// </summary>
        GraphicsDeviceService(IntPtr windowHandle, int width, int height, GraphicsProfile profile)
        {
            parameters = new PresentationParameters();

            parameters.BackBufferWidth = Math.Max(width, 1);
            parameters.BackBufferHeight = Math.Max(height, 1);
            parameters.BackBufferFormat = SurfaceFormat.Color;
            parameters.DepthStencilFormat = DepthFormat.Depth24;
            parameters.DeviceWindowHandle = windowHandle;
            parameters.PresentationInterval = PresentInterval.Immediate;
            parameters.IsFullScreen = false;

            graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, profile, parameters);
        }

        /// <summary>
        /// Gets a reference to the singleton instance.
        /// </summary>
        public static GraphicsDeviceService AddRef()
        {
            return AddRef(IntPtr.Zero, 0, 0, GraphicsProfile.HiDef);
        }

        /// <summary>
        /// Gets a reference to the singleton instance.
        /// </summary>
        public static GraphicsDeviceService AddRef(int width, int height)
        {
            return AddRef(IntPtr.Zero, width, height, GraphicsProfile.HiDef);
        }

        /// <summary>
        /// Gets a reference to the singleton instance.
        /// </summary>
        public static GraphicsDeviceService AddRef(IntPtr windowHandle, int width, int height, GraphicsProfile profile)
        {
            // Increment the "how many controls sharing the device" reference count.
            if (Interlocked.Increment(ref referenceCount) == 1)
            {
                if (windowHandle == IntPtr.Zero)
                    windowHandle = new System.Windows.Forms.Form().Handle;

                if (width <= 0 || height <= 0)
                {
                    width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                }

                // If this is the first control to start using the

                // device, we must create the singleton instance.
                singletonInstance = new GraphicsDeviceService(windowHandle, width, height, profile);                
            }
            return singletonInstance;
        }


        /// <summary>
        /// Releases a reference to the singleton instance.
        /// </summary>
        public void Release(bool disposing)
        {
            // Decrement the "how many controls sharing the device" reference count.
            if (Interlocked.Decrement(ref referenceCount) == 0)
            {
                // If this is the last control to finish using the

                // device, we should dispose the singleton instance.
                if (disposing)
                {
                    if (DeviceDisposing != null)
                        DeviceDisposing(this, EventArgs.Empty);

                    graphicsDevice.Dispose();
                }

                graphicsDevice = null;
            }
        }

        
        /// <summary>
        /// Resets the graphics device to whichever is bigger out of the specified
        /// resolution or its current size. This behavior means the device will
        /// demand-grow to the largest of all its GraphicsDeviceControl clients.
        /// </summary>
        public void ResetDevice(int width, int height)
        {
            if (DeviceResetting != null)
                DeviceResetting(this, EventArgs.Empty);

            parameters.BackBufferWidth = Math.Max(parameters.BackBufferWidth, width);
            parameters.BackBufferHeight = Math.Max(parameters.BackBufferHeight, height);

#if PCL
            // TODO: Apply graphicsdevice parameters
#else
            graphicsDevice.Reset(parameters);
#endif

            if (DeviceReset != null)
                DeviceReset(this, EventArgs.Empty);
        }

        
        /// <summary>
        /// Gets the current graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }

        GraphicsDevice graphicsDevice;


        // Store the current device settings.
        PresentationParameters parameters;


        // IGraphicsDeviceService events.
        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IGraphicsDeviceService))
                return this;
            return null;
        }
    }
    #endregion

    #region GraphicsDeviceServiceProvider
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
    #endregion
}
