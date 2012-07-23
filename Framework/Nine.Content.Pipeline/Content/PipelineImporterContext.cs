namespace Nine.Content.Pipeline
{
    using Microsoft.Xna.Framework.Content.Pipeline;

    class PipelineImporterContext : ContentImporterContext
    {
        public PipelineImporterContext()
        {

        }

        public override void AddDependency(string filename)
        {

        }

        public override string IntermediateDirectory
        {
            get { return PipelineConstants.IntermediateDirectory; }
        }

        public override ContentBuildLogger Logger
        {
            get { return PipelineBuildLogger.Instance; }
        }

        public override string OutputDirectory
        {
            get { return PipelineConstants.OutputDirectory; }
        }
    }
}
