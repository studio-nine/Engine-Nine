namespace Nine.Serialization.Importers
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Nine.Content.Pipeline;
    using System;
    using System.IO;

    public class EffectImporter : EffectProcessor, Nine.Serialization.IContentImporter
    {
        public object Import(Stream stream, IServiceProvider serviceProvider)
        {
            return ContentPipeline.Load<Microsoft.Xna.Framework.Graphics.Effect>(stream, new Microsoft.Xna.Framework.Content.Pipeline.EffectImporter(), this, serviceProvider);
        }

        public string[] SupportedFileExtensions
        {
            get { return ContentPipeline.GetSupportedFileExtensions(typeof(Microsoft.Xna.Framework.Content.Pipeline.EffectImporter)); }
        }
    }
}