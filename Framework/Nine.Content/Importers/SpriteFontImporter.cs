namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using System;
    using System.IO;

    public class SpriteFontImporter : Nine.Serialization.IContentImporter
    {
        private PipelineImporter importer;

        public SpriteFontImporter()
        {
            importer = new PipelineImporter()
            {
                Importer = new FontDescriptionImporter(),
                Processor = new FontDescriptionProcessor(),
            };
        }

        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
            return importer.Import(stream, serviceProvider);
        }

        public string[] SupportedFileExtensions
        {
            get { return importer.SupportedFileExtensions; }
        }
    }
}