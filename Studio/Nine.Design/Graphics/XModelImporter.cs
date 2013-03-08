namespace Nine.Graphics.Design
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Content.Processors;
    using Nine.Studio.Extensibility;
    using System.Collections.Generic;

    [Export(typeof(IImporter))]
    [LocalizedDisplayName("XModel")]
    [LocalizedCategory("Model")]
    [ExportMetadata(Class = "Models", IsDefault = true)]
    public class XModelImporter : Nine.Design.PipelineImporter<Model>
    {
        public override IContentImporter ContentImporter
        {
            get { return new XImporter(); }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return new ExtendedModelProcessor(); }
        }

        public override IEnumerable<string> GetSupportedFileExtensions()
        {
            yield return ".x";
        }
    }
}
