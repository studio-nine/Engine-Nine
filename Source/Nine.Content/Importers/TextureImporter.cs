namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Content.Pipeline;
    using System;
    using System.IO;

    public class TextureImporter : TextureProcessor, Nine.Serialization.IContentImporter
    {
        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
            var startPosition = stream.Position;
            var needContentPipeline = ColorKeyEnabled || GenerateMipmaps || ResizeToPowerOfTwo || TextureFormat == TextureProcessorOutputFormat.DxtCompressed;

            try
            {
                if (!needContentPipeline)
                    return Texture2D.FromStream(serviceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice, stream);
            }
            catch
            {
                stream.Position = startPosition;
                return ContentPipeline.Load<Microsoft.Xna.Framework.Graphics.Texture>(stream, new Microsoft.Xna.Framework.Content.Pipeline.TextureImporter(), this, serviceProvider);
            }
            return ContentPipeline.Load<Microsoft.Xna.Framework.Graphics.Texture>(stream, new Microsoft.Xna.Framework.Content.Pipeline.TextureImporter(), this, serviceProvider);
        }

        public string[] SupportedFileExtensions
        {
            get { return ContentPipeline.GetSupportedFileExtensions(typeof(Microsoft.Xna.Framework.Content.Pipeline.TextureImporter)); }
        }

        public TextureImporter()
        {
            ColorKeyEnabled = false;
        }
    }
}