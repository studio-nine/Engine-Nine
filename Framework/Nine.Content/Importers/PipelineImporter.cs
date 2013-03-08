namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Represents an importer that is compatible the XNA content pipeline.
    /// </summary>
    public class PipelineImporter : Nine.Serialization.IContentImporter
    {
        private PipelineBuilder builder;
        private string[] supportedFileExtensions;

        public GraphicsDevice GraphicsDevice { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }

        public Microsoft.Xna.Framework.Content.Pipeline.IContentImporter Importer { get; set; }
        public Microsoft.Xna.Framework.Content.Pipeline.IContentProcessor Processor { get; set; }

        public Func<object, object> Convert;

        public virtual object Import(Stream stream, IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            this.GraphicsDevice = serviceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice;
            this.builder = Nine.Graphics.GraphicsResources<PipelineBuilder>.GetInstance(GraphicsDevice);

            if (Importer == null)
                return null;

            var file = stream as FileStream;
            if (file == null)
                throw new NotSupportedException();
            
            file.Close();

            var content = builder.BuildAndLoad<object>(file.Name, Importer, Processor, null);
            if (content == null)
                return null;

            if (Convert != null)
            {
                var result = Convert(content);
                if (result != null)
                    return result;
            }
            builder.Compile(builder.OutputFilename, content);
            return builder.Content.Load<object>(builder.OutputFilename);
        }

        public string[] SupportedFileExtensions
        {
            get { return supportedFileExtensions ?? (supportedFileExtensions = GetSupportedFileExtensions()); }
        }
        
        private string[] GetSupportedFileExtensions()
        {
            if (Importer == null)
                return null;

            return Importer.GetType().GetCustomAttributes(true)
                           .OfType<ContentImporterAttribute>()
                           .SelectMany(x => x.FileExtensions).ToArray();
        }
    }
}