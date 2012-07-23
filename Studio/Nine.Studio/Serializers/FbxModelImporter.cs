namespace Nine.Studio.Serializers
{
    using System.ComponentModel.Composition;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Studio.Extensibility;

    [Export(typeof(IImporter))]
    [LocalizedDisplayName("FbxModel")]
    [LocalizedCategory("Model")]
    public class FbxModelImporter : PipelineImporter<Model>
    {
        public FbxModelImporter()
        {
            FileExtensions.Add(".fbx");
        }

        public override IContentImporter ContentImporter
        {
            get { return new FbxImporter(); }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return new ModelProcessor(); }
        }
    }
}
