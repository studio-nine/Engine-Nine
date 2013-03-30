namespace Nine.Content.Pipeline
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Graphics;

    class PipelineProcessorContext : ContentProcessorContext
    {
        private OpaqueDataDictionary parameters;
        private PipelineBuilder builder;

        public PipelineProcessorContext(PipelineBuilder pipelineBuilder)
        {
            this.builder = pipelineBuilder;
        }

        public override void AddDependency(string filename) { builder.ResolveExternalReference(filename); }
        public override void AddOutputFile(string filename) { }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            return builder.BuildAndLoad<TOutput>(builder.ResolveExternalReference(sourceAsset.Filename), processorName, processorParameters, importerName);
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string fileName)
        {
            return new ExternalReference<TOutput>(builder.ResolveExternalReference(sourceAsset.Filename) + ".xnb");
        }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            return builder.Convert<TInput, TOutput>(input, processorName, processorParameters);
        }
        
        public override string BuildConfiguration
        {
            get { return "Release"; }
        }

        public override string IntermediateDirectory
        {
            get { return ContentPipeline.IntermediateDirectory; }
        }

        public override ContentBuildLogger Logger
        {
            get { return PipelineBuildLogger.Instance; }
        }

        public override string OutputDirectory
        {
            get { return ContentPipeline.OutputDirectory; }
        }

        public override string OutputFilename
        {
            get { return builder.OutputFilename; }
        }

        public override OpaqueDataDictionary Parameters
        {
            get { return parameters ?? (parameters = new OpaqueDataDictionary()); }
        }

        public override TargetPlatform TargetPlatform
        {
            get { return TargetPlatform.Windows; }
        }

        public override GraphicsProfile TargetProfile
        {
            get { return GraphicsProfile.Reach; }
        }
    }
}
