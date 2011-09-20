#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Nine.Graphics.Effects;
using Nine.Content.Pipeline.Graphics;
using Nine.Content.Pipeline.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// Processes the input CustomEffectContent.
    /// </summary>
    [ContentProcessor(DisplayName = "Custom Effect - Engine Nine")]
    public class CustomEffectProcessor : ContentProcessor<EffectContent, CustomEffectContent>
    {
        public const string PrecedualTextureDirectory = "Textures\\Precedual";

        public virtual EffectProcessorDebugMode DebugMode { get; set; }
        public virtual string Defines { get; set; }
        public virtual string ResourceDirectory { get; set; }

        public override CustomEffectContent Process(EffectContent input, ContentProcessorContext context)
        {
            if (context.TargetPlatform == TargetPlatform.WindowsPhone)
            {
                context.Logger.LogWarning(null, null, Strings.CustomEffectNotSupported);
                return new CustomEffectContent
                {
                     EffectCode = new byte[0],
                     Parameters = new Dictionary<string,object>(),
                };
            }

            var effectProcessor = new EffectProcessor
            {
                 DebugMode = DebugMode,
                 Defines = Defines,
            };

            var compiledEffect = effectProcessor.Process(input, context);

            return new CustomEffectContent() 
            {
                EffectCode = compiledEffect.GetEffectCode(),
                Parameters = ProcessParameters(input.Identity, input, context),
            };
        }

        private Dictionary<string, object> ProcessParameters(ContentIdentity identity, EffectContent effectContent, ContentProcessorContext context)
        {
            var opaqueData = new Dictionary<string, object>();
            var effectProcessor = new EffectProcessor { DebugMode = DebugMode, Defines = Defines };
            var compiledEffect = effectProcessor.Process(effectContent, new CustomEffectContentProcessorContext(context));
            var effect = new Effect(ContentGraphics.GraphicsDevice, compiledEffect.GetEffectCode());
            foreach (var parameter in effect.Parameters)
            {
                // Semantic
                if (!string.IsNullOrEmpty(parameter.Semantic))
                {
                    EffectSemantics sementic;
                    if (!Enum.TryParse<EffectSemantics>(parameter.Semantic, true, out sementic))
                    {
                        context.Logger.LogWarning(null, identity, "Effect parameter semantic {0} is not supported", parameter.Semantic);
                    }
                }

                // Annotation
                EffectAnnotations annotation;
                var annotations = new Dictionary<EffectAnnotations, EffectAnnotation>();
                foreach (var a in parameter.Annotations)
                {
                    if (Enum.TryParse<EffectAnnotations>(a.Name, true, out annotation))
                        annotations.Add(annotation, a);
                }

                foreach (var a in annotations)
                {
                    if (a.Key == EffectAnnotations.ResourceName)
                    {
                        if (a.Value.ParameterType != EffectParameterType.String)
                        {
                            throw new InvalidContentException("ResourceName must be a string");
                        }

                        string filename = a.Value.GetValueString();
                        if (!File.Exists(filename))
                        {
                            if (!string.IsNullOrEmpty(ResourceDirectory))
                                filename = Path.Combine(ResourceDirectory, filename);
                            if (!File.Exists(filename))
                            {
                                context.Logger.LogWarning(null, identity, "Missing asset: {0}", filename);
                                continue;
                            }
                        }

                        if (!string.IsNullOrEmpty(filename))
                        {
                            if (parameter.ParameterType == EffectParameterType.Texture ||
                                parameter.ParameterType == EffectParameterType.Texture1D ||
                                parameter.ParameterType == EffectParameterType.Texture2D ||
                                parameter.ParameterType == EffectParameterType.Texture3D ||
                                parameter.ParameterType == EffectParameterType.TextureCube)
                            {
                                opaqueData.Add(parameter.Name, context.BuildAsset<TextureContent, TextureContent>(
                                    new ExternalReference<TextureContent>(filename), "TextureProcessor"));
                                continue;
                            }
                        }
                    }
#if MDX
                    else if (a.Key == EffectAnnotations.Function)
                    {
                        if (a.Value.ParameterType != EffectParameterType.String)
                            throw new InvalidContentException("Function must be a string");
                        if (parameter.ParameterType != EffectParameterType.Texture && parameter.ParameterType != EffectParameterType.Texture2D)
                            throw new NotSupportedException("Only 2D procedural textures are supported.");
                        
                        EffectAnnotation dimension;
                        if (!annotations.TryGetValue(EffectAnnotations.Dimensions, out dimension))
                            throw new InvalidContentException("Dimensions attribute must be specified when the texture is generated from a shader function.");
                        if (dimension.ParameterType != EffectParameterType.Single && dimension.RowCount != 2)
                            throw new InvalidContentException("Dimensions attribute must be a float2.");

                        var functionName = a.Value.GetValueString();
                        var size = dimension.GetValueVector2();

                        try
                        {
                            var textureGenerator = new ProceduralTextureGenerator();
                            var outputFilename = textureGenerator.CreateTextureContentFromShader(identity.SourceFilename, a.Value.GetValueString(), (int)size.X, (int)size.Y, context.IntermediateDirectory);
                            var sourceTexture = new ExternalReference<TextureContent>(outputFilename);
                            var assetName = Path.Combine(PrecedualTextureDirectory, Path.GetFileNameWithoutExtension(outputFilename));

                            context.Logger.LogImportantMessage("Precedual texture generated: {0} -> {1}", sourceTexture.Filename, assetName);
                            sourceTexture = context.BuildAsset<TextureContent, TextureContent>(sourceTexture, "TextureProcessor", null, null, assetName);
                            opaqueData.Add(parameter.Name, sourceTexture);
                        }
                        catch (FileNotFoundException e)
                        {
                            throw new InvalidOperationException("Managed DirectX is needed to enable precedual texture generation.", e);
                        }
                        catch (Exception e)
                        {
                            throw new InvalidContentException("Error generating precedual texture from shader " + functionName + "\n" + e);
                        }
                    }
#endif
                }
            }
            return opaqueData;
        }
    }
    
    class CustomEffectContentProcessorContext : ContentProcessorContext
    {
        ContentProcessorContext context;

        public CustomEffectContentProcessorContext(ContentProcessorContext context)
        {
            this.context = context;
        }

        public override void AddDependency(string filename)
        {
            context.AddDependency(filename);
        }

        public override string IntermediateDirectory
        {
            get { return context.IntermediateDirectory; }
        }

        public override ContentBuildLogger Logger
        {
            get { return context.Logger; }
        }

        public override string OutputDirectory
        {
            get { return context.OutputDirectory; }
        }

        public override void AddOutputFile(string filename)
        {
            context.AddOutputFile(filename);
        }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            return context.BuildAndLoadAsset<TInput, TOutput>(sourceAsset, processorName, processorParameters, importerName);
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        {
            return context.BuildAsset<TInput, TOutput>(sourceAsset, processorName, processorParameters, importerName, assetName);
        }

        public override string BuildConfiguration
        {
            get { return context.BuildConfiguration; }
        }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            return context.Convert<TInput, TOutput>(input, processorName, processorParameters);
        }

        public override string OutputFilename
        {
            get { return context.OutputFilename; }
        }

        public override OpaqueDataDictionary Parameters
        {
            get { return context.Parameters; }
        }

        public override TargetPlatform TargetPlatform
        {
            get { return TargetPlatform.Windows; }
        }

        public override GraphicsProfile TargetProfile
        {
            get { return GraphicsProfile.HiDef; }
        }
    }
}
