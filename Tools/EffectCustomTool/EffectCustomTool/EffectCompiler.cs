//=============================================================================
//
//  Copyright 2009 - 2010 (c) Yufei Huang.
//
//=============================================================================

#region Using Directives
using System;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using CustomToolTemplate;
#endregion

namespace Microsoft.VisualStudio.Shell.Interop
{
    internal class ProcessorContext : ContentProcessorContext
    {
        public override void AddDependency(string filename) { }
        public override void AddOutputFile(string filename) { }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            return default(TOutput);
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        {
            return null;
        }

        public override string BuildConfiguration
        {
            get { return ""; }
        }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            return default(TOutput);
        }

        public override string IntermediateDirectory
        {
            get { return Path.GetTempPath(); }
        }

        public override ContentBuildLogger Logger
        {
            get { return null; }
        }

        public override string OutputDirectory
        {
            get { return Path.GetTempPath(); }
        }

        public override string OutputFilename
        {
            get { return "CompiledEffect.xnb"; }
        }

        public override OpaqueDataDictionary Parameters
        {
            get { return new OpaqueDataDictionary(); }
        }

        internal TargetPlatform platform = TargetPlatform.Windows;

        public override TargetPlatform TargetPlatform
        {
            get { return platform; }
        }

        public override GraphicsProfile TargetProfile
        {
            get { return GraphicsProfile.Reach; }
        }
    }


    public class EffectCompiler
    {
        public string WindowsEffectCode { get; private set; }
        public string XBoxEffectCode { get; private set; }
        public StringBuilder Properties { get; private set; }
        public StringBuilder Initialize { get; private set; }
        

        public EffectCompiler(string sourceFile, string shaderContent)
        {
            Properties = new StringBuilder();
            Initialize = new StringBuilder();

            // Create graphics device
            Form dummy = new Form();

            PresentationParameters parameters = new PresentationParameters();
            parameters.BackBufferWidth = 1;
            parameters.BackBufferHeight = 1;
            parameters.DeviceWindowHandle = dummy.Handle;

            GraphicsAdapter.UseNullDevice = true;
            GraphicsDevice device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.Reach, parameters);
            

            // Compile effect
            EffectContent content = new EffectContent() { EffectCode = shaderContent };

            content.Name = Path.GetFileNameWithoutExtension(sourceFile);
            content.Identity = new ContentIdentity(sourceFile);
            

            EffectProcessor compiler = new EffectProcessor();
            ProcessorContext context = new ProcessorContext();

            compiler.Defines = "WINDOWS";
            compiler.DebugMode = EffectProcessorDebugMode.Optimize;            
            context.platform = TargetPlatform.Windows;
            CompiledEffectContent windowsCompiledEffect = compiler.Process(content, context);

            compiler.Defines = "XBOX;XBOX360";
            context.platform = TargetPlatform.Xbox360;
            CompiledEffectContent xbox360CompiledEffect = compiler.Process(content, context);

            byte[] windowsEffectCode = windowsCompiledEffect.GetEffectCode();
            byte[] xbox360EffectCode = xbox360CompiledEffect.GetEffectCode();

            WindowsEffectCode = ByteArrayToString(windowsEffectCode);
            XBoxEffectCode = ByteArrayToString(xbox360EffectCode);


            // Initialize parameters
            Effect effect = new Effect(device, windowsEffectCode);

            // Create a field and property for each effect parameter
            for (int i = 0; i < effect.Parameters.Count; i++)
            {
                EffectParameter parameter = effect.Parameters[i];
                Type parameterType;
                try
                {
                    parameterType = GetParameterType(parameter);
                }
                catch (NotSupportedException notSupportedException)
                {
                    // Unable to create a property for this paramter
                    continue;
                }


                bool isArray = IsArray(parameter);
                bool isPublic = parameter.Name[0] >= 'A' && parameter.Name[0] <= 'Z';

                string indent = "        ";
                string fieldName = "_" + parameter.Name;
                string propertyName = parameter.Name;
                string propertyTypeReference = isArray ? GetCSharpTypeName(parameterType) + "[]" : 
                                                         GetCSharpTypeName(parameterType);
                

                // Initialize
                Initialize.Append(indent);
                Initialize.Append("    ");
                Initialize.Append("this.");
                Initialize.Append(fieldName);
                Initialize.Append(" = Parameters[\"");
                Initialize.Append(parameter.Name);
                Initialize.AppendLine("\"];");


                // Field
                Properties.Append(indent);
                Properties.Append("private EffectParameter ");
                Properties.Append(fieldName);
                Properties.AppendLine(";");
                Properties.AppendLine();

                // Summary
                EffectAnnotation sasUiDescriptionAnnotation = parameter.Annotations["SasUiDescription"];
                if (sasUiDescriptionAnnotation != null)
                {
                    Properties.Append(indent);
                    Properties.AppendLine("/// <summary>");
                    Properties.Append(indent);
                    Properties.Append("/// ");
                    Properties.AppendLine(sasUiDescriptionAnnotation.GetValueString());
                    Properties.Append(indent);
                    Properties.AppendLine("/// </summary>");
                }
                
                // Property
                Properties.Append(indent);
                Properties.Append(isPublic ? "public " : "internal ");
                Properties.Append(propertyTypeReference);
                Properties.Append(" ");
                Properties.AppendLine(propertyName);
                Properties.Append(indent);
                Properties.AppendLine("{");
                
                // Get method
                if (!isArray)
                {
                    Properties.Append(indent);
                    Properties.Append("    ");
                    Properties.Append("get { return ");
                    Properties.Append(fieldName);
                    Properties.Append(".GetValue");
                    Properties.Append(parameterType.Name);
                    Properties.AppendLine("(); }");
                }

                // Set method
                Properties.Append(indent);
                Properties.Append("    ");
                Properties.Append("set { ");
                Properties.Append(fieldName);
                Properties.AppendLine(".SetValue(value); }");

                Properties.Append(indent);
                Properties.AppendLine("}");
                Properties.AppendLine();
            }            
        }

        private static bool IsArray(EffectParameter parameter)
        {
            return parameter.Elements.Count != 0;
        }

        private static string GetCSharpTypeName(Type type)
        {
            if (type == typeof(int))
                return "int";
            if (type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(string))
                return "string";
            
            return type.Name;
        }

        private static Type GetParameterType(EffectParameter effectParameter)
        {
            bool isArray = IsArray(effectParameter);

            switch (effectParameter.ParameterClass)
            {
                case EffectParameterClass.Matrix:
                    if (effectParameter.ParameterType == EffectParameterType.Single && effectParameter.RowCount == 4 && effectParameter.ColumnCount == 4)
                        return typeof(Matrix);
                    break;

                case EffectParameterClass.Object:

                    switch (effectParameter.ParameterType)
                    {
                        case EffectParameterType.String:
                            return typeof(String);

                        case EffectParameterType.Single:
                            return typeof(float);

                        case EffectParameterType.Int32:
                            if (isArray)
                                throw new NotSupportedException("Array not supported");
                            return typeof(Int32);

                        case EffectParameterType.Bool:
                            if (isArray)
                                throw new NotSupportedException("Array not supported");
                            return typeof(bool);

                        case EffectParameterType.Texture:
                            if (isArray)
                                throw new NotSupportedException("Array not supported");                         
                            return typeof(Texture2D);

                        case EffectParameterType.Texture2D:
                            if (isArray)
                                throw new NotSupportedException("Array not supported");
                            return typeof(Texture2D);

                        case EffectParameterType.Texture3D:
                            if (isArray)
                                throw new NotSupportedException("Array not supported");
                            return typeof(Texture3D);

                        case EffectParameterType.TextureCube:
                            if (isArray)
                                throw new NotSupportedException("Array not supported");
                            return typeof(TextureCube);
                    }
                    break;

                case EffectParameterClass.Scalar:
                    switch (effectParameter.ParameterType)
                    {
                        case EffectParameterType.Single:
                            return typeof(float);
                    }
                    break;

                case EffectParameterClass.Struct:
                    throw new NotSupportedException(""); //TODO: support structs

                case EffectParameterClass.Vector:

                    switch (effectParameter.ParameterType)
                    {
                        case EffectParameterType.Single:
                            switch (effectParameter.ColumnCount)
                            {
                                case 2:
                                    return typeof(Vector2);
                                case 3:
                                    return typeof(Vector3);
                                case 4:
                                    return typeof(Vector4);
                            }
                            break;
                    }
                    break;
            }

            throw new NotSupportedException("");
        }


        private static string ByteArrayToString(byte[] effectCode)
        {
            StringBuilder builder = new StringBuilder(effectCode.Length);

            for (int i = 0; i < effectCode.Length; i++)
            {
                if (i > 0 && i % 24 == 0)
                    builder.AppendLine();

                builder.AppendFormat("0x{0:X2}, ", effectCode[i]);
            }

            return builder.ToString();
        }

        public static void Main(string[] args)
        {
            using (StreamReader reader = new StreamReader(new FileStream(@"D:/BasicEffect.fx", FileMode.Open)))
            {
                EffectCompiler compiler = new EffectCompiler(@"D:/BasicEffect.fx", reader.ReadToEnd());
            }
        }
    }
}
