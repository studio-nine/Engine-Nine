namespace Nine.Content.Pipeline
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Graphics;

    class PipelineProcessorContext : ContentProcessorContext
    {
        private OpaqueDataDictionary parameters;
        private PipelineBuilder pipelineBuilder;

        public PipelineProcessorContext(PipelineBuilder pipelineBuilder)
        {
            this.pipelineBuilder = pipelineBuilder;
        }

        public override void AddDependency(string filename) { pipelineBuilder.ResolveExternalReference(filename); }
        public override void AddOutputFile(string filename) { }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            var builder = pipelineBuilder.Clone();
            return builder.BuildAndLoad<TOutput>(pipelineBuilder.ResolveExternalReference(sourceAsset.Filename), processorName, processorParameters, importerName);
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        {
            var builder = pipelineBuilder.Clone();
            return new ExternalReference<TOutput>(builder.Build(pipelineBuilder.ResolveExternalReference(sourceAsset.Filename), processorName, processorParameters, importerName, assetName));
        }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            var builder = pipelineBuilder.Clone();
            return builder.Convert<TInput, TOutput>(input, processorName, processorParameters);
        }
        
        public override string BuildConfiguration
        {
            get { return pipelineBuilder.Constants.BuildConfiguration; }
        }

        public override string IntermediateDirectory
        {
            get { return pipelineBuilder.Constants.IntermediateDirectory; }
        }

        public override ContentBuildLogger Logger
        {
            get { return PipelineBuildLogger.Instance; }
        }

        public override string OutputDirectory
        {
            get { return pipelineBuilder.Constants.OutputDirectory; }
        }

        public override string OutputFilename
        {
            get { return pipelineBuilder.OutputFilename; }
        }

        public override OpaqueDataDictionary Parameters
        {
            get { return parameters ?? (parameters = new OpaqueDataDictionary()); }
        }

        public override TargetPlatform TargetPlatform
        {
            get { return pipelineBuilder.Constants.TargetPlatform; }
        }

        public override GraphicsProfile TargetProfile
        {
            get { return pipelineBuilder.Constants.TargetProfile; }
        }
    }
}
