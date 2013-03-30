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
            return loader.Create<object>(input.BaseStream);
        }

        class PipelineContentManager : ContentManager
        {
            Stream currentStream;

            public PipelineContentManager(IServiceProvider serviceProvider)
                : base(serviceProvider)
            { }

            public T Create<T>(Stream stream)
            {
                try
                {
                    currentStream = stream;
                    return ReadAsset<T>("?", null);
                }
                finally
                {
                    currentStream = null;
                }
            }

            public override T Load<T>(string assetName)
            {
                // This should only be called during ContentReader.ReadExternalReference
                return ServiceProvider.GetService<ContentLoader>().Load<T>(assetName);
            }

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
        }
    }
}