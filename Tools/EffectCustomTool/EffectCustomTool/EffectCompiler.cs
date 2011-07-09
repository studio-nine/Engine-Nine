//=============================================================================
//
//  Copyright 2009 - 2010 (c) Yufei Huang.
//
//=============================================================================

#region Using Directives
using System;
using System.IO;
using System.CodeDom;
using System.Linq;
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
    public class EffectCompiler : IEqualityComparer<EffectParameter>
    {
        public string Designer;
        public string Default;

        bool hiDef = false;
        List<string> structNames = new List<string>();
        StringBuilder structures = new StringBuilder();
        private GenerationEventArgs e;

        public EffectCompiler(string className, string nameSpace, string sourceFile, GenerationEventArgs e)
        {
            this.e = e;
            Generate(className, nameSpace, sourceFile);
        }

        private string Generate(string className, string nameSpace, string sourceFile)
        {
            // Create graphics device
            Form dummy = new Form();

            PresentationParameters parameters = new PresentationParameters();
            parameters.BackBufferWidth = 1;
            parameters.BackBufferHeight = 1;
            parameters.DeviceWindowHandle = dummy.Handle;

            GraphicsAdapter.UseNullDevice = true;
            GraphicsDevice device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);

            CompiledEffectContent windowsCompiledEffect = null;
            CompiledEffectContent windowsHiDefCompiledEffect = null;
            CompiledEffectContent xbox360CompiledEffect = null;

            byte[] windowsEffectCode = null;
            byte[] windowsHiDefEffectCode = null;
            byte[] xbox360EffectCode = null;

            try
            {
                hiDef = false;
                windowsCompiledEffect = BuildEffect(sourceFile, TargetPlatform.Windows, GraphicsProfile.Reach);
                windowsHiDefCompiledEffect = BuildEffect(sourceFile, TargetPlatform.Windows, GraphicsProfile.HiDef);
                xbox360CompiledEffect = BuildEffect(sourceFile, TargetPlatform.Xbox360, GraphicsProfile.Reach);
            }
            catch (Exception ex)
            {
                if (e != null)
                    e.GenerateWarning(ex.ToString());
                if (!hiDef)
                {
                    hiDef = true;
                    windowsCompiledEffect = BuildEffect(sourceFile, TargetPlatform.Windows, GraphicsProfile.HiDef);
                    xbox360CompiledEffect = BuildEffect(sourceFile, TargetPlatform.Xbox360, GraphicsProfile.HiDef);
                    windowsHiDefCompiledEffect = windowsCompiledEffect;
                }
            }

            string indent = "        ";

            StringBuilder check = new StringBuilder();
            if (hiDef)
            {
                check.AppendLine();
                check.AppendLine(indent + "    if (GraphicsDevice.GraphicsProfile != GraphicsProfile.HiDef)");
                check.AppendFormat(indent + "        throw new InvalidOperationException(\"{{$CLASS}} requires GraphicsProfile.HiDef.\");");
                check.AppendLine();
                check.AppendLine();
            }

            windowsEffectCode = windowsCompiledEffect.GetEffectCode();
            windowsHiDefEffectCode = windowsHiDefCompiledEffect.GetEffectCode();
            xbox360EffectCode = xbox360CompiledEffect.GetEffectCode();

            string WindowsEffectCode = ByteArrayToString(windowsEffectCode);
            string XboxEffectCode = ByteArrayToString(xbox360EffectCode);

            // Initialize parameters
            Effect effect = new Effect(device, windowsEffectCode);
            Effect effectHiDef = new Effect(device, windowsHiDefEffectCode);

            string WindowsHiDefEffectCode = (effect.Parameters.Concat(effectHiDef.Parameters).Distinct(this).ToList().Count == effect.Parameters.Count) ?
                                            "ReachEffectCode" : "new byte[]\r\n{" + ByteArrayToString(windowsHiDefEffectCode) + "\r\n}";

            string body = GetProperties(className, indent, effect.Parameters, effectHiDef.Parameters);

            string content = Strings.Designer;

            content = content.Replace(@"{$CHECK}", check.ToString());
            content = content.Replace(@"{$BODY}", body.ToString());
            content = content.Replace(@"{$STRUCTS}", structures.ToString());
            content = content.Replace(@"{$CLASS}", className);
            content = content.Replace(@"{$NAMESPACE}", nameSpace);
            content = content.Replace(@"{$XBOXBYTECODE}", XboxEffectCode);
            content = content.Replace(@"{$WINDOWSBYTECODE}", WindowsEffectCode);
            content = content.Replace(@"{$WINDOWSHIDEFBYTECODE}", WindowsHiDefEffectCode);
            content = content.Replace(@"{$VERSION}", GetType().Assembly.GetName().Version.ToString(4));
            content = content.Replace(@"{$RUNTIMEVERSION}", GetType().Assembly.ImageRuntimeVersion);

            return Designer = content;
        }

        private bool ByteEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }

        private string GetProperties(string className, string indent, EffectParameterCollection effectParametersReach,
                                                                      EffectParameterCollection effectParametersHiDef)
        {
            StringBuilder Apply = new StringBuilder();
            StringBuilder Clone = new StringBuilder();
            StringBuilder DirtyFlags = new StringBuilder();
            StringBuilder Initialize = new StringBuilder();
            StringBuilder Properties = new StringBuilder();

            List<string> parameterNames = new List<string>();

            uint dirtyFlag = 0;

            var effectParameters = effectParametersReach.Concat(effectParametersHiDef).Distinct(this).ToList();

            // Create a field and property for each effect parameter
            for (int i = 0; i < effectParameters.Count; i++)
            {
                EffectParameter parameter = effectParameters[i];
                EffectParameter parameterReach = FindCorrespondingParameter(effectParametersReach, parameter);
                EffectParameter parameterHiDef = FindCorrespondingParameter(effectParametersHiDef, parameter);

                string parameterType;
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
                string propertyTypeReference = isArray ? parameterType + "[]" : parameterType;
                string fieldDirtyFlagName = parameter.Name + "DirtyFlag";

                // Initialize
                UseProfileWrapper(parameter, parameterReach, parameterHiDef, Initialize, indent, () =>
                {
                    Initialize.Append(indent);
                    Initialize.AppendFormat("    this.{0} = cloneSource[\"{1}\"];", parameterFieldName, parameter.Name);
                    if (IsStruct(parameter))
                    {
                        Initialize.AppendLine();
                        Initialize.Append(indent);
                        if (isArray)
                        {
                            Initialize.AppendFormat("    this.{0} = new {1}[{2}];", fieldName, parameterType, parameter.Elements.Count);
                            Initialize.AppendLine();
                            Initialize.Append(indent);
                            Initialize.AppendFormat("    for (int i = 0; i < this.{0}.Length; i++)", fieldName);
                            Initialize.AppendLine();
                            Initialize.Append(indent);
                            Initialize.AppendFormat("        this.{0}[i] = new {1}({2}.Elements[i].StructureMembers);", fieldName, parameterType, parameterFieldName);
                        }
                        else
                        {
                            Initialize.AppendFormat("    this.{0} = new {1}({2});", fieldName, parameterType, parameterFieldName);
                        }
                    }
                });
                Initialize.AppendLine();

                UseProfileWrapper(parameter, parameterReach, parameterHiDef, Apply, indent, () =>
                {
                    if (IsStruct(parameter))
                    {
                        if (isArray)
                        {
                            Apply.Append(indent);
                            Apply.AppendFormat("    for (int i = 0; i < {0}; i++)", parameter.Elements.Count);
                            Apply.AppendLine();
                            Apply.Append(indent);
                            Apply.AppendFormat("        this.{0}[i].Apply();", fieldName);
                        }
                        else
                        {
                            Apply.AppendFormat(indent + "    this.{0}.Apply();", fieldName);
                        }
                        Apply.AppendLine();
                    }
                    else
                    {
                        Apply.AppendFormat(indent + "    if ((this.dirtyFlag & {0}) != 0)", fieldDirtyFlagName);
                        Apply.AppendLine();
                        Apply.AppendLine(indent + "    {");
                        Apply.AppendFormat(indent + "        this.{0}.SetValue({1});", parameterFieldName, fieldName);
                        Apply.AppendLine();
                        Apply.AppendFormat(indent + "        this.dirtyFlag &= ~{0};", fieldDirtyFlagName);
                        Apply.AppendLine();
                        Apply.AppendLine(indent + "    }");
                    }
                });

                // Field
                if (!parameterNames.Contains(parameter.Name))
                {
                    DirtyFlags.AppendFormat(indent + "const uint {0} = 1 << {1};", fieldDirtyFlagName, dirtyFlag);
                    DirtyFlags.AppendLine();

                    Clone.AppendFormat(indent + "    this.{0} = cloneSource.{0};", fieldName);
                    Clone.AppendLine();

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
                    if (HasSetMethod(parameter))
                    {
                        Properties.Append(indent);
                        if (ShouldCompareValue(parameter))
                        {
                            Properties.AppendFormat(@"    set {{ if ({0} != value) {{ {0} = value; dirtyFlag |= {1}; }} }}", fieldName, fieldDirtyFlagName);
                        }
                        else
                        {
                            Properties.AppendFormat(@"    set {{ {0} = value; dirtyFlag |= {1}; }}", fieldName, fieldDirtyFlagName);
                        }
                        Properties.AppendLine();
                    }
                    Properties.Append(indent + "}");
                    Properties.AppendLine();
                    Properties.AppendLine();

                    dirtyFlag++;
                }

                if (!parameterNames.Contains(parameter.Name))
                    parameterNames.Add(parameter.Name);
            }

            string content = Strings.Body.Replace("        ", indent);
            content = content.Replace(@"{$PROPERTIES}", Properties.ToString());
            content = content.Replace(@"{$INITIALIZE}", Initialize.ToString());
            content = content.Replace(@"{$APPLY}", Apply.ToString());
            content = content.Replace(@"{$CLONE}", Clone.ToString());
            content = content.Replace(@"{$CLASS}", className);
            content = content.Replace(@"{$DIRTYFLAGS}", DirtyFlags.ToString());
            return content.ToString();
        }

        private void UseProfileWrapper(EffectParameter parameter, EffectParameter parameterReach, 
                                       EffectParameter parameterHiDef, StringBuilder builder, string indent, Action action)
        {
            bool hasReach = EffectParameterEquals(parameter, parameterReach);
            bool hasHiDef = EffectParameterEquals(parameter, parameterHiDef);

            if (!(hasHiDef && hasReach))
            {
                if (hasReach)
                {
                    builder.Append(indent);
                    builder.AppendLine(@"    if (GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)");
                    builder.Append(indent);
                    builder.AppendLine("    {");
                    action();
                    builder.AppendLine();
                    builder.Append(indent);
                    builder.AppendLine("    }");
                }
                if (hasHiDef)
                {
                    builder.Append(indent);
                    builder.AppendLine(@"    if (GraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)");
                    builder.Append(indent);
                    builder.AppendLine("    {");
                    action();
                    builder.AppendLine();
                    builder.Append(indent);
                    builder.AppendLine("    }");
                }
            }
            else
            {
                action();
            }
        }

        private EffectParameter FindCorrespondingParameter(EffectParameterCollection effectParametersHiDef, EffectParameter parameter)
        {
            return effectParametersHiDef.FirstOrDefault(p => p.Name == parameter.Name && p.ParameterType == parameter.ParameterType &&
                                                             p.ParameterClass == parameter.ParameterClass);
        }

        bool EffectParameterEquals(EffectParameter a, EffectParameter b)
        {
            return a != null && b != null && a.Name == b.Name && a.ParameterType == b.ParameterType &&
                   a.ParameterClass == b.ParameterClass && a.RowCount == b.RowCount && a.ColumnCount == b.ColumnCount &&
                 ((a.Elements != null && b.Elements != null && a.Elements.Count == b.Elements.Count) ||
                  (a.Elements == null && b.Elements == null));
        }

        private bool IsStruct(EffectParameter parameter)
        {
            return parameter.ParameterClass == EffectParameterClass.Struct;
        }

        private bool HasSetMethod(EffectParameter parameter)
        {
            return parameter.ParameterClass != EffectParameterClass.Struct;
        }

        private bool ShouldCompareValue(EffectParameter parameter)
        {
            return !IsArray(parameter) && 
                   (parameter.ParameterClass == EffectParameterClass.Scalar ||
                    parameter.ParameterClass == EffectParameterClass.Object);
        }

        private CompiledEffectContent BuildEffect(string sourceFile, TargetPlatform targetPlatform, GraphicsProfile targetProfile)
        {
            // Compile effect
            ContentBuildLogger logger = new CustomLogger(e);

            // Import the effect source code.
            EffectImporter importer = new EffectImporter();
            ContentImporterContext importerContext = new CustomImporterContext(logger);
            EffectContent sourceEffect = importer.Import(sourceFile, importerContext);

            // Compile the effect.
            EffectProcessor processor = new EffectProcessor();
            processor.DebugMode = EffectProcessorDebugMode.Optimize;
            processor.Defines = targetProfile == GraphicsProfile.Reach ? "Reach;REACH" : "HiDef;HIDEF";
            processor.Defines += ";";
            processor.Defines += targetPlatform == TargetPlatform.Xbox360 ? "Xbox;XBOX;XBOX360" : "Windows;WIN;WINDOWS";
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

        private string GetParameterType(EffectParameter effectParameter)
        {
            bool isArray = IsArray(effectParameter);

            switch (effectParameter.ParameterClass)
            {
                case EffectParameterClass.Matrix:
                    if (effectParameter.ParameterType == EffectParameterType.Single && effectParameter.RowCount == 4 && effectParameter.ColumnCount == 4)
                        return GetCSharpTypeName(typeof(Matrix));
                    break;

                case EffectParameterClass.Object:

                    switch (effectParameter.ParameterType)
                    {
                        case EffectParameterType.String:
                            return GetCSharpTypeName(typeof(String));

                        case EffectParameterType.Single:
                            return GetCSharpTypeName(typeof(float));

                        case EffectParameterType.Int32:
                            return GetCSharpTypeName(typeof(Int32));

                        case EffectParameterType.Bool:
                            return GetCSharpTypeName(typeof(bool));

                        case EffectParameterType.Texture:                     
                            return GetCSharpTypeName(typeof(Texture2D));

                        case EffectParameterType.Texture2D:
                            return GetCSharpTypeName(typeof(Texture2D));

                        case EffectParameterType.Texture3D:
                            return GetCSharpTypeName(typeof(Texture3D));

                        case EffectParameterType.TextureCube:
                            return GetCSharpTypeName(typeof(TextureCube));
                    }
                    break;

                case EffectParameterClass.Scalar:
                    switch (effectParameter.ParameterType)
                    {
                        case EffectParameterType.Single:
                            return GetCSharpTypeName(typeof(float));
                        case EffectParameterType.Int32:
                            return GetCSharpTypeName(typeof(int));
                        case EffectParameterType.Bool:
                            return GetCSharpTypeName(typeof(bool));
                    }
                    break;

                case EffectParameterClass.Struct:
                    if (isArray)
                        return AddStruct(effectParameter.Elements[0]);
                    return AddStruct(effectParameter);
                    break;

                case EffectParameterClass.Vector:

                    switch (effectParameter.ParameterType)
                    {
                        case EffectParameterType.Single:
                            switch (effectParameter.ColumnCount)
                            {
                                case 2:
                                    return GetCSharpTypeName(typeof(Vector2));
                                case 3:
                                    return GetCSharpTypeName(typeof(Vector3));
                                case 4:
                                    return GetCSharpTypeName(typeof(Vector4));
                            }
                            break;
                    }
                    break;
            }

            throw new NotSupportedException("");
        }

        private string AddStruct(EffectParameter effectParameter)
        {
            string className = "Class_" + effectParameter.Name;
            if (!structNames.Contains(className))
            {
                var body = GetProperties(className, "            ", effectParameter.StructureMembers,
                                                                    effectParameter.StructureMembers);
                string content = Strings.Struct;
                content = content.Replace(@"{$BODY}", body.ToString());
                content = content.Replace(@"{$CLASS}", className);
                structures.AppendLine();
                structures.Append(content);
                structNames.Add(className);
            }
            return className;
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

        public bool Equals(EffectParameter x, EffectParameter y)
        {
            return EffectParameterEquals(x, y);
        }

        public int GetHashCode(EffectParameter obj)
        {
            return 0;
        }

        static void Main(string[] args)
        {
            EffectCompiler compiler = new EffectCompiler("DirectionalLightEffect", "Nine.Graphics.Effects", @"D:\BasicEffect.fx", null);

            File.WriteAllText(@"D:\BasicEffect.Designer.cs", compiler.Designer);
        }
    }
}
