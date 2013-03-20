namespace Nine.Serialization
{
    using Microsoft.Xna.Framework.Content;
    using System;
    using System.IO;

    class PipelineObjectReader : IBinaryObjectReader
    {
        PipelineContentManager loader;

        public object Read(BinaryReader input, object existingInstance, IServiceProvider serviceProvider)
        {
            if (loader == null)
                loader = new PipelineContentManager(serviceProvider);
            return loader.LoadUnique(input.BaseStream);
        }
    }

    class PipelineContentManager : ContentManager
    {
        Stream currentStream;

        public PipelineContentManager(IServiceProvider serviceProvider) : base(serviceProvider) 
        { }

        protected override Stream OpenStream(string fileName)
        {
            if (currentStream != null)
            {
                var result = currentStream;
                currentStream = null;
                return result;
            }
            return base.OpenStream(fileName);
        }

        public object LoadUnique(Stream stream)
        {
            try
            {
                currentStream = stream;
                return Load<object>(Guid.NewGuid().ToString("N"));
            }
            finally
            {
                currentStream = null;
            }
        }
    }
}