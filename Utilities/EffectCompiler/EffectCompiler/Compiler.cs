namespace EffectCompiler
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Reflection;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.Diagnostics;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;

    public class CompiledEffect
    {
        public bool HiDef { get; internal set; }
        public List<EffectParameter> EffectParameters { get; internal set; }

        /// Xna
        public byte[] WindowsEffect { get; internal set; }
        public byte[] WindowsHiDefEffect { get; internal set; }
        public byte[] XboxEffect { get; internal set; }

        public string WindowsEffectCode { get; internal set; }
        public string WindowsHiDefEffectCode { get; internal set; }
        public string XboxEffectCode { get; internal set; }

        /// MonoGame
        public byte[] DirectX_11Effect { get; internal set; }
        public byte[] OpenGLEffect { get; internal set; }
        public byte[] PlayStation4Effect { get; internal set; }

        public string DirectX_11EffectCode { get; internal set; }
        public string OpenGLEffectCode { get; internal set; }
        public string PlayStation4EffectCode { get; internal set; }
    }

    public class Compiler
    {
        private GraphicsDevice GraphicsDevice;
        private EffectParameterComparer effectParameterComparer;

        private MGFXBuilder mgfxBuilder;

        public Compiler()
        {
            // Create graphics device
            Form dummy = new Form();

            PresentationParameters parameters = new PresentationParameters();
            parameters.BackBufferWidth = 1;
            parameters.BackBufferHeight = 1;
            parameters.DeviceWindowHandle = dummy.Handle;

            GraphicsAdapter.UseNullDevice = true;
            GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);

            effectParameterComparer = new EffectParameterComparer();
            mgfxBuilder = new MGFXBuilder();
        }

        public CompiledEffect Build(string sourceFile)
        {
            bool HiDef = false;

            CompiledEffectContent windowsCompiledEffect = null;
            CompiledEffectContent windowsHiDefCompiledEffect = null;
            CompiledEffectContent xbox360CompiledEffect = null;

            byte[] windowsEffect = null;
            byte[] windowsHiDefEffect = null;
            byte[] xboxEffect = null;

            try
            {
                HiDef = false;
                windowsCompiledEffect = BuildEffect(sourceFile, TargetPlatform.Windows, GraphicsProfile.Reach);
                windowsHiDefCompiledEffect = BuildEffect(sourceFile, TargetPlatform.Windows, GraphicsProfile.HiDef);
                xbox360CompiledEffect = BuildEffect(sourceFile, TargetPlatform.Xbox360, GraphicsProfile.HiDef);
            }
            catch
            {
                if (!HiDef)
                {
                    HiDef = true;
                    windowsCompiledEffect = BuildEffect(sourceFile, TargetPlatform.Windows, GraphicsProfile.HiDef);
                    xbox360CompiledEffect = BuildEffect(sourceFile, TargetPlatform.Xbox360, GraphicsProfile.HiDef);
                    windowsHiDefCompiledEffect = windowsCompiledEffect;
                }
            }


            windowsEffect = windowsCompiledEffect.GetEffectCode();
            windowsHiDefEffect = windowsHiDefCompiledEffect.GetEffectCode();
            xboxEffect = xbox360CompiledEffect.GetEffectCode();

            string windowsHiDefEffectCode;
            string windowsEffectCode = ByteArrayToString(windowsEffect);
            string xboxEffectCode = ByteArrayToString(xboxEffect);

            // Initialize parameters
            Effect effect = new Effect(GraphicsDevice, windowsEffect);
            Effect effectHiDef = new Effect(GraphicsDevice, windowsHiDefEffect);

            if (effect.Parameters.Concat(effectHiDef.Parameters).Distinct(effectParameterComparer).ToList().Count == effect.Parameters.Count)
            {
                windowsHiDefEffectCode = windowsEffectCode;
            }
            else
            {
                windowsHiDefEffectCode = ByteArrayToString(windowsHiDefEffect);
            }

            var DirectX_11Effect = mgfxBuilder.Run(sourceFile, MGFXTarget.DirectX_11);
            var OpenGLEffect = mgfxBuilder.Run(sourceFile, MGFXTarget.OpenGL);
            var PlayStation4Effect = mgfxBuilder.Run(sourceFile, MGFXTarget.PlayStation4);

            return new CompiledEffect
            {
                HiDef = false,
                EffectParameters = effect.Parameters.Concat(effectHiDef.Parameters).Distinct(effectParameterComparer).Where(p => p.ParameterClass != EffectParameterClass.Struct).ToList(),

                WindowsEffect = windowsEffect,
                WindowsHiDefEffect = windowsHiDefEffect,
                XboxEffect = xboxEffect,

                WindowsEffectCode = windowsEffectCode,
                WindowsHiDefEffectCode = windowsHiDefEffectCode,
                XboxEffectCode = xboxEffectCode,

                DirectX_11Effect = DirectX_11Effect,
                OpenGLEffect = OpenGLEffect,
                PlayStation4Effect = PlayStation4Effect,

                DirectX_11EffectCode = ByteArrayToString(DirectX_11Effect),
                OpenGLEffectCode = ByteArrayToString(OpenGLEffect),
                PlayStation4EffectCode = ByteArrayToString(PlayStation4Effect),
            };
        }

        private CompiledEffectContent BuildEffect(string sourceFile, TargetPlatform targetPlatform, GraphicsProfile targetProfile)
        {
            ContentBuildLogger logger = new CustomLogger();

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

        private static string ByteArrayToString(byte[] effectCode)
        {
            if (effectCode == null)
                return "";

            StringBuilder builder = new StringBuilder(effectCode.Length);

            for (int i = 0; i < effectCode.Length; i++)
            {
                if (i > 0 && i % 24 == 0)
                    builder.AppendLine();

                builder.AppendFormat("0x{0:X2}, ", effectCode[i]);
            }

            return builder.ToString();
        }

        class EffectParameterComparer : IEqualityComparer<EffectParameter>
        {
            public bool Equals(EffectParameter x, EffectParameter y)
            {
                return EffectParameterEquals(x, y);
            }

            bool EffectParameterEquals(EffectParameter a, EffectParameter b)
            {
                return a != null && b != null && a.Name == b.Name;
            }

            public int GetHashCode(EffectParameter obj)
            {
                return 0;
            }
        }
    }

    enum MGFXTarget
    {
        DirectX_11,
        OpenGL,
        PlayStation4,
    }

    class MGFXBuilder
    {
        string temporaryFolderPath;
        DirectoryInfo temporaryFolder;

        public MGFXBuilder()
        {
            temporaryFolderPath = Path.Combine(Path.GetTempPath(), ToString());
            temporaryFolder = Directory.CreateDirectory(temporaryFolderPath);
        }

        ~MGFXBuilder()
        {
            temporaryFolder.Delete(true);
        }

        public byte[] Run(string sourceFile, MGFXTarget target)
        {
            string output = Path.Combine(temporaryFolderPath, Path.GetFileName(sourceFile));

            // 2MGFX <SourceFile> <OutputFile> [/Debug] [/Profile:<DirectX_11,OpenGL,PlayStation4>]
            Process process = new Process();
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
            process.StartInfo.FileName = "2MGFX.exe";
            process.StartInfo.Arguments = string.Format("{0} {1} /Profile:{2}", sourceFile, output, target.ToString());
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Debug.WriteLine(process.StartInfo.FileName);
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 1)
            {
                return File.ReadAllBytes(output);
            }
            else
            {
                // Throw Error
                return new byte[] { };
            }
        }
    }

    class CustomImporterContext : ContentImporterContext
    {
        public CustomImporterContext(ContentBuildLogger logger)
        {
            this.logger = logger;
        }

        public override ContentBuildLogger Logger
        {
            get { return logger; }
        }
        ContentBuildLogger logger;

        public override string IntermediateDirectory
        {
            get { return string.Empty; }
        }

        public override string OutputDirectory
        {
            get { return string.Empty; }
        }

        public override void AddDependency(string filename)
        {

        }
    }

    class CustomLogger : ContentBuildLogger
    {
        public CustomLogger()
        {

        }

        public override void LogMessage(string message, params object[] messageArgs)
        {
            System.Diagnostics.Trace.WriteLine(string.Format(message, messageArgs));
        }

        public override void LogImportantMessage(string message, params object[] messageArgs)
        {
            System.Diagnostics.Trace.WriteLine(string.Format(message, messageArgs));
        }

        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        {

        }
    }

    class CustomProcessorContext : ContentProcessorContext
    {
        public CustomProcessorContext(TargetPlatform targetPlatform, GraphicsProfile targetProfile, ContentBuildLogger logger)
        {
            this.targetPlatform = targetPlatform;
            this.targetProfile = targetProfile;
            this.logger = logger;
        }

        public override TargetPlatform TargetPlatform
        {
            get { return targetPlatform; }
        }
        TargetPlatform targetPlatform;

        public override GraphicsProfile TargetProfile
        {
            get { return targetProfile; }
        }

        GraphicsProfile targetProfile;

        public override ContentBuildLogger Logger
        {
            get { return logger; }
        }

        ContentBuildLogger logger;

        public override string BuildConfiguration
        {
            get { return string.Empty; }
        }

        public override string IntermediateDirectory
        {
            get { return string.Empty; }
        }

        public override string OutputDirectory
        {
            get { return string.Empty; }
        }

        public override string OutputFilename
        {
            get { return string.Empty; }
        }

        public override OpaqueDataDictionary Parameters
        {
            get { return parameters; }
        }
        OpaqueDataDictionary parameters = new OpaqueDataDictionary();

        public override void AddDependency(string filename)
        {

        }

        public override void AddOutputFile(string filename)
        {

        }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            throw new NotImplementedException();
        }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            throw new NotImplementedException();
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
