namespace Nine.Serialization
{
    using Microsoft.Xna.Framework.Content;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    class XnbLoader : ContentManager, IContentImporter
    {
        Stream currentStream;

        public XnbLoader(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Stream OpenStream(string assetName)
        {
            if (currentStream != null)
            {
                var result = currentStream;
                currentStream = null;
                return result;
            }
            return base.OpenStream(assetName);
        }

        object IContentImporter.Import(Stream stream, IServiceProvider serviceProvider)
        {
            try
            {
                currentStream = stream;
                return Load<object>(Guid.NewGuid().ToString("N").ToUpper());
            }
            finally
            {
                currentStream = null;
            }
        }

        string[] IContentImporter.SupportedFileExtensions
        {
            get { return SupportedFileExtensions; }
        }
        static readonly string[] SupportedFileExtensions = new[] { ".xnb" };
    }
}