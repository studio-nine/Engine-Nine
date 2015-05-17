namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Nine.Content.Pipeline;
    using System;
    using System.IO;

    public class SpriteFontImporter : Nine.Serialization.IContentImporter
    {
        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
            return ContentPipeline.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>(stream, new FontDescriptionImporter(), new FontDescriptionProcessor(), serviceProvider);
        }

        public string[] SupportedFileExtensions
        {
            get { return ContentPipeline.GetSupportedFileExtensions(typeof(FontDescriptionImporter)); }
        }
    }

    class SpriteFontWriter : PipelineObjectWriter<Microsoft.Xna.Framework.Graphics.SpriteFont> { }
}