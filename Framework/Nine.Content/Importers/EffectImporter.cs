namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using System;
    using System.IO;

    public class EffectImporter : EffectProcessor, Nine.Serialization.IContentImporter
    {
        private PipelineImporter importer;

        public EffectImporter()
        {
            importer = new PipelineImporter()
            {
                Importer = new Microsoft.Xna.Framework.Content.Pipeline.EffectImporter(),
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