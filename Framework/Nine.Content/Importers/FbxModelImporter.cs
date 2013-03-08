namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using System;
    using System.IO;

    public class FbxModelImporter : Nine.Serialization.Processors.ExtendedModelProcessor, Nine.Serialization.IContentImporter
    {
        private PipelineImporter importer;

        public FbxModelImporter()
        {
            importer = new PipelineImporter()
            {
                Importer = new FbxImporter(),
                Processor = this,
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