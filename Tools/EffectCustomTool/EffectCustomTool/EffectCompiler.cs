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
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace Nine.Tools.EffectCustomTool
{
    public class EffectCompiler
    {
        public string Designer;
        public string Default;

        StringBuilder Apply = new StringBuilder();
        StringBuilder Clone = new StringBuilder();
        StringBuilder DirtyFlags = new StringBuilder();
        StringBuilder Initialize = new StringBuilder();
        StringBuilder Properties = new StringBuilder();
        string WindowsEffectCode;
        string XboxEffectCode;
        bool hiDef = false;

        public EffectCompiler(string className, string nameSpace, string sourceFile)
        {
            // Create graphics device
            Form dummy = new Form();

            PresentationParameters parameters = new PresentationParameters();
            parameters.BackBufferWidth = 1;
            parameters.BackBufferHeight = 1;
            parameters.DeviceWindowHandle = dummy.Handle;

            GraphicsAdapter.UseNullDevice = true;
            GraphicsDevice device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);

            CompiledEffectContent windowsCompiledEffect;
            CompiledEffectContent xbox360CompiledEffect;

            byte[] windowsEffectCode = null;
            byte[] xbox360EffectCode = null;

            try
            {
                windowsCompiledEffect = BuildEffect(sourceFile, TargetPlatform.Windows, GraphicsProfile.Reach);
                xbox360CompiledEffect = BuildEffect(sourceFile, TargetPlatform.Xbox360, GraphicsProfile.Reach);
            }
            catch (Exception ex) 
            {
                hiDef = true;

                windowsCompiledEffect = BuildEffect(sourceFile, TargetPlatform.Windows, GraphicsProfile.HiDef);
                xbox360CompiledEffect = BuildEffect(sourceFile, TargetPlatform.Xbox360, GraphicsProfile.HiDef);
            }

            string indent = "        ";

            if (hiDef)
            {
                Initialize.AppendLine(indent + "    if (GraphicsDevice.GraphicsProfile != GraphicsProfile.HiDef)");
                Initialize.AppendFormat(indent + "        throw new InvalidOperationException(\"{{$CLASS}} requires GraphicsProfile.HiDef.\");");
                Initialize.AppendLine();
                Initialize.AppendLine();
            }

            windowsEffectCode = windowsCompiledEffect.GetEffectCode();
            xbox360EffectCode = xbox360CompiledEffect.GetEffectCode();

            WindowsEffectCode = ByteArrayToString(windowsEffectCode);
            XboxEffectCode = ByteArrayToString(xbox360EffectCode);

            // Initialize parameters
            Effect effect = new Effect(device, windowsEffectCode);

            uint dirtyFlag = 0;

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

                string fieldName = "_" + parameter.Name;
                string parameterFieldName = "_" + parameter.Name + "Parameter";
                string propertyName = parameter.Name;
                string propertyTypeReference = isArray ? GetCSharpTypeName(parameterType) + "[]" : 
                                                         GetCSharpTypeName(parameterType);
                string fieldDirtyFlagName = parameter.Name + "DirtyFlag";

                // Initialize
                Initialize.Append(indent);
                Initialize.AppendFormat("    this.{0} = cloneSource.Parameters[\"{1}\"];", parameterFieldName, parameter.Name);
                Initialize.AppendLine();


                DirtyFlags.AppendFormat(indent + "const uint {0} = 1 << {1};", fieldDirtyFlagName, dirtyFlag);
                DirtyFlags.AppendLine();

                Clone.AppendFormat(indent + "    this.{0} = cloneSource.{0};", fieldName);
                Clone.AppendLine();

                Apply.AppendFormat(indent + "    if ((this.dirtyFlag & {0}) != 0)", fieldDirtyFlagName);
                Apply.AppendLine();
                Apply.AppendLine(indent + "    {");
                Apply.AppendFormat(indent + "        this.{0}.SetValue({1});", parameterFieldName, fieldName);
                Apply.AppendLine();
                Apply.AppendFormat(indent + "        this.dirtyFlag &= ~{0};", fieldDirtyFlagName);
                Apply.AppendLine();
                Apply.AppendLine(indent + "    }");


                // Field
                Properties.Append(indent);
                Properties.AppendFormat("private {0} {1};", propertyTypeReference, fieldName);
                Properties.AppendLine();

                Properties.Append(indent);
                Properties.Append("private EffectParameter ");
                Properties.Append(parameterFieldName);
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
                Properties.Append(indent);
                Properties.AppendFormat("    get {{ return {0}; }}", fieldName);
                Properties.AppendLine();

                // Set method
                Properties.Append(indent);
                Properties.AppendFormat(@"    set {{ {0} = value; dirtyFlag |= {1}; }}", fieldName, fieldDirtyFlagName);
                Properties.AppendLine();
                Properties.Append(indent + "}");
                Properties.AppendLine();
                Properties.AppendLine();

                dirtyFlag++;
            }


            string content = Strings.Designer;

            content = content.Replace(@"{$PROPERTIES}", Properties.ToString());
            content = content.Replace(@"{$INITIALIZE}", Initialize.ToString());
            content = content.Replace(@"{$APPLY}", Apply.ToString());
            content = content.Replace(@"{$CLONE}", Clone.ToString());
            content = content.Replace(@"{$DIRTYFLAGS}", DirtyFlags.ToString());
            content = content.Replace(@"{$CLASS}", className);
            content = content.Replace(@"{$NAMESPACE}", nameSpace);
            content = content.Replace(@"{$XBOXBYTECODE}", XboxEffectCode);
            content = content.Replace(@"{$WINDOWSBYTECODE}", WindowsEffectCode);
            content = content.Replace(@"{$VERSION}", GetType().Assembly.GetName().Version.ToString(4));
            content = content.Replace(@"{$RUNTIMEVERSION}", GetType().Assembly.ImageRuntimeVersion);

            Designer = content;
        }

        private static CompiledEffectContent BuildEffect(string sourceFile, TargetPlatform targetPlatform, GraphicsProfile targetProfile)
        {
            // Compile effect
            ContentBuildLogger logger = new CustomLogger();

            // Import the effect source code.
            EffectImporter importer = new EffectImporter();
            ContentImporterContext importerContext = new CustomImporterContext(logger);
            EffectContent sourceEffect = importer.Import(sourceFile, importerContext);

            // Compile the effect.
            EffectProcessor processor = new EffectProcessor();
            ContentProcessorContext processorContext = new CustomProcessorContext(targetPlatform, targetProfile, logger);
            return processor.Process(sourceEffect, processorContext);
        }

        public string Process(string content, string className, string namespaceName)
        {
            return null;
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

        static void Main(string[] args)
        {
            EffectCompiler compiler = new EffectCompiler("Hi", "NS", @"D:\BasicEffect.fx");

            File.WriteAllText(@"D:\BasicEffect.Designer.cs", compiler.Designer);
        }
    }
}
