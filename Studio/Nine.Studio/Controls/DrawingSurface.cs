namespace Nine.Studio.Controls
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;
    using System.Windows.Media;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;

    /// <summary>
    /// Provides data for the Draw event.
    /// </summary>
    public sealed class DrawEventArgs : EventArgs
    {
        internal DrawingSurface DrawingSurface;

        public TimeSpan DeltaTime { get; internal set; }
        public TimeSpan TotalTime { get; internal set; }
        public GraphicsDevice GraphicsDevice { get; internal set; }

        public void InvalidateSurface()
        {
            DrawingSurface.Invalidate();
        }
    }

    /// <summary>
    /// Defines an area within which 3-D content can be composed and rendered. 
    /// </summary>
    /// <remarks>
    /// Thanks to:
    ///     maoren      (http://forums.create.msdn.com/forums/p/53048/321984.aspx#321984)
    ///     bozalina    (http://blog.bozalina.com/2010/11/xna-40-and-wpf.html)
    /// </remarks>
    public class DrawingSurface : ContentControl, IDisposable
    {
        // TODO: Cannot change this profile after the surface is loaded
        public GraphicsProfile GraphicsProfile { get; set; }

        public event EventHandler<DrawEventArgs> Draw;
        
        internal GraphicsDeviceService GraphicsDeviceService;
        internal D3DImage D3dImage;
        internal Image Image;

        private ContentManager content;
        private IntPtr backbuffer = IntPtr.Zero;
        private Stopwatch stopWatch;
        private TimeSpan lastDrawTimestamp;
        private DrawEventArgs eventArgs = new DrawEventArgs();
        private bool contentNeedsRefresh = false;

        public ContentManager ContentManager
        {
            get
            {
                if (content == null)
                {
                    ServiceContainer services = new ServiceContainer();
                    services.AddService<IGraphicsDeviceService>(GraphicsDeviceService);
                    content = new ContentManager(services);
                }
                return content;
            }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return GraphicsDeviceService.GraphicsDevice; }
        }

        public DrawingSurface()
        {
            GraphicsProfile = GraphicsProfile.HiDef;
            AddChild(Image = new Image() { Source = D3dImage = new D3DImage() });
            Image.Stretch = Stretch.None;
            Image.Source = D3dImage = new D3DImage();
            D3dImage.IsFrontBufferAvailableChanged += new DependencyPropertyChangedEventHandler(D3dImage_IsFrontBufferAvailableChanged);

            SizeChanged += new SizeChangedEventHandler(DrawingSurface_SizeChanged);
            Loaded += new RoutedEventHandler(DrawingSurface_Loaded);
            Unloaded += new RoutedEventHandler(DrawingSurface_Unloaded);
        }

        void DrawingSurface_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            contentNeedsRefresh = true;
        }
                
        void DrawingSurface_Loaded(object sender, RoutedEventArgs e)
        {
            if (GraphicsDeviceService == null)
            {
                GraphicsDeviceService = GraphicsDeviceService.AddRef(IntPtr.Zero, (int)ActualWidth, (int)ActualHeight, GraphicsProfile);
                GraphicsDeviceService.DeviceResetting += GraphicsDeviceService_DeviceResetting;

                D3dImage.Lock();
                D3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, backbuffer = Interop.GetBackBuffer(GraphicsDeviceService.GraphicsDevice));
                D3dImage.Unlock();
                Marshal.Release(backbuffer);

                CompositionTarget.Rendering += CompositionTarget_Rendering;

                stopWatch = Stopwatch.StartNew();
                lastDrawTimestamp = TimeSpan.Zero;
                contentNeedsRefresh = true;
            }
        }

        void DrawingSurface_Unloaded(object sender, RoutedEventArgs e)
        {
            if (GraphicsDeviceService != null)
            {
                // D3DImage will keep an reference to the backbuffer that causes the device reset below to fail.
                D3dImage.Lock();
                D3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                D3dImage.Unlock();
                backbuffer = IntPtr.Zero;

                CompositionTarget.Rendering -= CompositionTarget_Rendering;

                GraphicsDeviceService.DeviceResetting -= GraphicsDeviceService_DeviceResetting;
                GraphicsDeviceService = null;
            }
        }

        void GraphicsDeviceService_DeviceResetting(object sender, EventArgs e)
        {
            // D3DImage will keep an reference to the backbuffer that causes the device reset below to fail.
            D3dImage.Lock();
            D3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
            D3dImage.Unlock();
            backbuffer = IntPtr.Zero;
        }

        void D3dImage_IsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (D3dImage.IsFrontBufferAvailable)
                contentNeedsRefresh = true;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (Draw != null && contentNeedsRefresh && BeginDraw())
            {
                contentNeedsRefresh = false;

                D3dImage.Lock();
                D3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, backbuffer = Interop.GetBackBuffer(GraphicsDeviceService.GraphicsDevice));

                ResetGraphicsDeviceState();
                SetViewport();
                
                eventArgs.GraphicsDevice = GraphicsDeviceService.GraphicsDevice;
                eventArgs.DrawingSurface = this;
                eventArgs.TotalTime = stopWatch.Elapsed;
                eventArgs.DeltaTime = eventArgs.TotalTime - lastDrawTimestamp;
                lastDrawTimestamp = eventArgs.TotalTime;

                Draw(this, eventArgs);
                EndDraw();

                D3dImage.AddDirtyRect(new Int32Rect(0, 0, (int)ActualWidth, (int)ActualHeight));
                D3dImage.Unlock();

                Marshal.Release(backbuffer);
            }
        }

        bool BeginDraw()
        {
            // If we have no graphics device, we must be running in the designer.
            if (GraphicsDeviceService == null)
                return false;

            if (!D3dImage.IsFrontBufferAvailable)
                return false;

            // Make sure the graphics device is big enough, and is not lost.
            if (!HandleDeviceReset())
                return false;

            return true;
        }

        private void ResetGraphicsDeviceState()
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        }

        void SetViewport()
        {
            // Many GraphicsDeviceControl instances can be sharing the same
            // GraphicsDevice. The device backbuffer will be resized to fit the
            // largest of these controls. But what if we are currently drawing
            // a smaller control? To avoid unwanted stretching, we set the
            // viewport to only use the top left portion of the full backbuffer.
            Viewport viewport = new Viewport();

            viewport.X = 0;
            viewport.Y = 0;
            viewport.Width = Math.Max(1, (int)ActualWidth);
            viewport.Height = Math.Max(1, (int)ActualHeight);
            viewport.MinDepth = 0;
            viewport.MaxDepth = 1;

            GraphicsDeviceService.GraphicsDevice.Viewport = viewport;
        }

        void EndDraw()
        {
            try
            {
                //Rectangle sourceRectangle = new Rectangle(0, 0, (int)ActualWidth, (int)ActualHeight);
                //graphicsDeviceService.GraphicsDevice.Present(sourceRectangle, null, handle);
                //graphicsDeviceService.GraphicsDevice.Present();
                //Marshal.Release(GetBackBuffer(graphicsDeviceService.GraphicsDevice));
            }
            catch
            {
                // Present might throw if the device became lost while we were
                // drawing. The lost device will be handled by the next BeginDraw,
                // so we just swallow the exception.
            }
        }

        bool HandleDeviceReset()
        {
            bool deviceNeedsReset = false;

            switch (GraphicsDeviceService.GraphicsDevice.GraphicsDeviceStatus)
            {  
                case GraphicsDeviceStatus.Lost:
                    // If the graphics device is lost, we cannot use it at all.
                    return false;

                case GraphicsDeviceStatus.NotReset:
                    // If device is in the not-reset state, we should try to reset it.
                    deviceNeedsReset = true;
                    break;

                default:
                    // If the device state is ok, check whether it is big enough.
                    PresentationParameters pp = GraphicsDeviceService.GraphicsDevice.PresentationParameters;
                    deviceNeedsReset = ((int)ActualWidth > pp.BackBufferWidth) || ((int)ActualHeight > pp.BackBufferHeight);
                    break;
            }

            if (deviceNeedsReset)
            {
                Debug.WriteLine("Resetting Device");
                GraphicsDeviceService.ResetDevice((int)ActualWidth, (int)ActualHeight);
                deviceNeedsReset = false;
                return false;
            }

            return true;
        }

        public void Invalidate()
        {
            contentNeedsRefresh = true;
        }

        #region IDisposable
        private bool isDisposed = false;

        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (GraphicsDeviceService != null)
                    GraphicsDeviceService.Release(disposing);
                isDisposed = true;
            }
        }

        ~DrawingSurface()
        {
            Dispose(false);
        }
        #endregion
    }
}
