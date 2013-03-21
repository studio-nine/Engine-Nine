namespace Nine.Serialization.CodeGeneration
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;

    public class CodeGenerator
    {
        public static string Generate(string target, string workingDirectory, bool delaySign, string keyFile, string[] references, Action<CompilerError> onError)
        {
            foreach (var reference in references)
            {
                Assembly.LoadFrom(reference);
            }

            //System.Diagnostics.Debugger.Launch();

            var g = new AssemblyData(Assembly.LoadFrom(target));
            var reader = new BinaryObjectReader(g);
            var writer = new BinaryObjectWriter(g);

            var readerCode = reader.TransformText();
            var writerCode = writer.TransformText();

            return Compile(g.Assembly, workingDirectory, delaySign, keyFile, references, onError, BuildAssemblyInfo(g.Assembly), readerCode, writerCode);
        }
        
        private static string BuildAssemblyInfo(Assembly assembly)
        {
            var name = assembly.GetName();
            return string.Format(
                "[assembly: System.Reflection.AssemblyVersion(\"{0}\")]" + Environment.NewLine +
                "[assembly: System.Reflection.AssemblyFileVersion(\"{0}\")]" + Environment.NewLine +
                "[assembly: System.Reflection.AssemblyTitle(\"{1}\")]" + Environment.NewLine +
                "[assembly: System.Reflection.AssemblyDescription(\"{1}.dll\")]" + Environment.NewLine,
                name.Version, name.Name + ".Serialization");
        }

        private static string Compile(Assembly assembly, string tempDir, bool delaySign, string keyFile, string[] references, Action<CompilerError> onError, params string[] sources)
        {
            if (string.IsNullOrEmpty(tempDir))
                tempDir = Path.GetTempPath();

            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            var options = new CompilerParameters();
            options.WarningLevel = 3;
            options.CompilerOptions = "/optimize /nostdlib+ ";
            if (delaySign)
                options.CompilerOptions += "/delaysign ";
            if (!string.IsNullOrWhiteSpace(keyFile))
                options.CompilerOptions += "/keyFile:\"" + keyFile + "\" ";
            options.GenerateInMemory = false;
            options.IncludeDebugInformation = false;
            options.OutputAssembly = Path.Combine(tempDir, assembly.GetName().Name + ".Serialization.dll");
            options.ReferencedAssemblies.Add(assembly.Location);
            options.ReferencedAssemblies.AddRange(references);

            for (int i = 0; i < sources.Length; i++)
            {
                var code = sources[i];
                sources[i] = Path.Combine(tempDir, FileNames[i]);
                File.WriteAllText(sources[i], code);
            }

            var result = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromFile(options, sources);
            if (result.Errors.HasErrors)
            {
                if (onError != null)
                {
                    foreach (CompilerError error in result.Errors)
                        onError(error);
                }
                return null;
            }

            return result.CompiledAssembly.Location;
        }

        private static readonly string[] FileNames = new[] { "AssemblyInfo.cs", "BinaryReaders.cs", "BinaryWriters.cs" };
    }
}