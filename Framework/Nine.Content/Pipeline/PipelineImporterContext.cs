namespace Nine.Content.Pipeline
{
    using Microsoft.Xna.Framework.Content.Pipeline;

    class PipelineImporterContext : ContentImporterContext
    {
        private PipelineBuilder pipelineBuilder;

        public PipelineImporterContext(PipelineBuilder builder)
        {
            this.pipelineBuilder = builder;
        }

        public override void AddDependency(string filename)
        {
            pipelineBuilder.ResolveExternalReference(filename);
        }

        public override string IntermediateDirectory
        {
            get { return pipelineBuilder.IntermediateDirectory; }
        }

        public override ContentBuildLogger Logger
        {
            get { return PipelineBuildLogger.Instance; }
        }

        public override string OutputDirectory
        {
            get { return pipelineBuilder.OutputDirectory; }
        }
    }
}
