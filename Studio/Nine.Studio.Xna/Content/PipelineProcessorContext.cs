#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Nine.Studio.Content
{
    class PipelineProcessorContext : ContentProcessorContext
    {
        OpaqueDataDictionary parameters;

        public override void AddDependency(string filename) { }
        public override void AddOutputFile(string filename) { }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            return PipelineBuilder.BuildAndLoad<TOutput>(sourceAsset.Filename, processorName, processorParameters, importerName);
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        {
            return new ExternalReference<TOutput>(PipelineBuilder.Build(sourceAsset.Filename, processorName, processorParameters, importerName, assetName));
        }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            return PipelineBuilder.Convert<TInput, TOutput>(input, processorName, processorParameters);
        }
        
        public override string BuildConfiguration
        {
            get { return "Release"; }
        }

        public override string IntermediateDirectory
        {
            get { return Global.IntermediateDirectory; }
        }

        public override ContentBuildLogger Logger
        {
            get { return PipelineBuildLogger.Instance; }
        }

        public override string OutputDirectory
        {
            get { return Global.OutputDirectory; }
        }

        public override string OutputFilename
        {
            get { throw new NotImplementedException(); }
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
