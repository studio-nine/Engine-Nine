namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Nine.Content.Pipeline;
    using System;
    using System.IO;

    public class XModelImporter : Nine.Content.Pipeline.Processors.ExtendedModelProcessor, Nine.Serialization.IContentImporter
    {
        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
            return ContentPipeline.Load<Microsoft.Xna.Framework.Graphics.Model>(stream, new XImporter(), this, serviceProvider);
        }

        public string[] SupportedFileExtensions
        {
            get { return ContentPipeline.GetSupportedFileExtensions(typeof(XImporter)); }
        }
    }
}