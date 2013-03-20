namespace Nine.Content.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    class PipelineContentManager : ContentManager
    {
        public Dictionary<string, Stream> MemoryStreams { get; private set; }

        public PipelineContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider ?? new PipelineGraphicsDeviceService())
        {
            MemoryStreams = new Dictionary<string, Stream>(StringComparer.OrdinalIgnoreCase);
        }

        public override T Load<T>(string fileName)
        {
            return base.Load<T>(fileName);
        }

        protected override Stream OpenStream(string fileName)
        {
            Stream memoryStream;
            if (MemoryStreams.TryGetValue(fileName, out memoryStream))
                return memoryStream;

            if (!fileName.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase))
                fileName = fileName + ".xnb";
            return File.OpenRead(fileName);
        }
    }

    class PipelineGraphicsDeviceService : IGraphicsDeviceService, IServiceProvider
    {
#pragma warning disable 0067
        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
#pragma warning restore 0067
        
        public GraphicsDevice GraphicsDevice
        {
            get { return PipelineGraphics.GraphicsDevice; }
        }

        public object GetService(Type serviceType)
        {
            return serviceType == typeof(IGraphicsDeviceService) ? this : null;
        }
    }
}
