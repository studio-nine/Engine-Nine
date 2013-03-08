namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.IO;

    public class TextureImporter : TextureProcessor, Nine.Serialization.IContentImporter
    {
        private PipelineImporter importer;

        public TextureImporter()
        {
            ColorKeyEnabled = false;
            importer = new PipelineImporter()
            {
                Importer = new Microsoft.Xna.Framework.Content.Pipeline.TextureImporter(),
                Processor = this,
            };
        }
        
        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
            var startPosition = stream.Position;
            var needContentPipeline = ColorKeyEnabled || GenerateMipmaps || ResizeToPowerOfTwo || TextureFormat == TextureProcessorOutputFormat.DxtCompressed;

            try
            {
                if (!needContentPipeline)
                    return Texture2D.FromStream(importer.GraphicsDevice, stream);
            }
            catch
            {
                stream.Position = startPosition;
                return importer.Import(stream, serviceProvider);
            }
            return importer.Import(stream, serviceProvider);
        }

        public string[] SupportedFileExtensions
        {
            get { return importer.SupportedFileExtensions; }
        }
    }
}