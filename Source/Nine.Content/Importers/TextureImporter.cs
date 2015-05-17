namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Content.Pipeline;
    using System;
    using System.IO;

    public class TextureImporter : TextureProcessor, Nine.Serialization.IContentImporter
    {
        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
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

    class Texture2DWriter : PipelineObjectWriter<Texture2D> { }
    class Texture3DWriter : PipelineObjectWriter<Texture3D> { }
    class TextureCubeWriter : PipelineObjectWriter<TextureCube> { }
}