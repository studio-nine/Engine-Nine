namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Nine.Content.Pipeline;
    using System;
    using System.IO;

    public class FbxModelImporter : Nine.Content.Pipeline.Processors.ExtendedModelProcessor, Nine.Serialization.IContentImporter
    {
        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
            return ContentPipeline.Load<Microsoft.Xna.Framework.Graphics.Model>(stream, new FbxImporter(), this, serviceProvider);
        }

        public string[] SupportedFileExtensions
        {
            get { return ContentPipeline.GetSupportedFileExtensions(typeof(FbxImporter)); }
        }
    }
}