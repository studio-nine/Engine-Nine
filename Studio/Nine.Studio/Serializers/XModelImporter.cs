namespace Nine.Studio.Serializers
{
    using System.ComponentModel.Composition;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Studio.Extensibility;

    [Export(typeof(IImporter))]
    [LocalizedDisplayName("XModel")]
    [LocalizedCategory("Model")]
    public class XModelImporter : PipelineImporter<Model>
    {
        public XModelImporter()
        {
            FileExtensions.Add(".x");
        }

        public override IContentImporter ContentImporter
        {
            get { return new XImporter(); }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return new ModelProcessor(); }
        }
    }
}
