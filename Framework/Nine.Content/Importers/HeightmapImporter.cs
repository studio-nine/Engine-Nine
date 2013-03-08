namespace Nine.Serialization.Importers
{
    using Nine.Serialization.Processors;
    using System;
    using System.IO;

    public class HeightmapImporter : HeightmapProcessor, Nine.Serialization.IContentImporter
    {
        private PipelineImporter importer;

        public HeightmapImporter()
        {
            importer = new PipelineImporter()
            {
                Importer = new Microsoft.Xna.Framework.Content.Pipeline.TextureImporter(),
                Processor = this,
                Convert = x => x,
            };
        }

        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
            return importer.Import(stream, serviceProvider);
        }

        public string[] SupportedFileExtensions
        {
            get { return null; }
        }
    }
}