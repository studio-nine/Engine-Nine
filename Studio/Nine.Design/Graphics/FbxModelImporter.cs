namespace Nine.Graphics.Design
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Content.Pipeline.Processors;
    using Nine.Studio.Extensibility;
    using System.Collections.Generic;

    [Export(typeof(IImporter))]
    [LocalizedDisplayName("FbxModel")]
    [LocalizedCategory("Model")]
    [ExportMetadata(Class = "Models", IsDefault = true)]
    public class FbxModelImporter : Nine.Design.PipelineImporter<Model>
    {
        public override IContentImporter ContentImporter
        {
            get { return new FbxImporter(); }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return new ExtendedModelProcessor(); }
        }

        public override IEnumerable<string> GetSupportedFileExtensions()
        {
            yield return ".fbx";
        }
    }
}
