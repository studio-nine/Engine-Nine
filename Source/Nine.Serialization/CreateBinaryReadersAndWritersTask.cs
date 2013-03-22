namespace Nine.Serialization.CodeGeneration
{
    using Microsoft.Build.Framework;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;

    public class CreateBinaryReadersAndWritersTask : MarshalByRefObject, ITask
    {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        public bool Merge { get; set; }
        public bool DelaySign { get; set; }
        public string KeyFile { get; set; }
        public string Target { get; set; }
        public string[] References { get; set; }
        public string WorkingDirectory { get; set; }
        
        public bool Execute()
        {
            //System.Diagnostics.Debugger.Launch();

            Embedded.EnsureAssembliesInitialized();

            AppDomain workerDomain = null;
            string serializationAssembly = null;

            try
            {
                workerDomain = AppDomain.CreateDomain("BinarySerialization");

                // http://www.west-wind.com/weblog/posts/2009/Jan/19/Assembly-Loading-across-AppDomains
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;                
                var generator = (CreateBinaryReadersAndWritersTask)workerDomain.CreateInstanceFromAndUnwrap(GetType().Assembly.Location, GetType().FullName);
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                
                serializationAssembly = generator.Generate(Target, WorkingDirectory, DelaySign, KeyFile, References);
            }
            catch (AggregateException e)
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(CompilerError));
                foreach (var error in e.InnerExceptions.OfType<InvalidOperationException>().Select(x => FromText(serializer, x.Message)))
                {
                    if (error.IsWarning)
                        LogWarning(error.ErrorText, error.ErrorNumber, error.FileName, error.Line, error.Column);
                    else
                        LogError(error.ErrorText, error.ErrorNumber, error.FileName, error.Line, error.Column);
                }
                return false;
            }
            catch (Exception e)
            {
                LogError(e.Message, null, null, 0, 0);
                return false;
            }
            finally
            {
                if (workerDomain != null)
                    AppDomain.Unload(workerDomain);
            }

            if (serializationAssembly != null)
            {
                LogMessage("Serialization assembly generated", MessageImportance.Normal);

                if (Merge)
                {
                    MergeAssemblies(DelaySign, KeyFile, Target, serializationAssembly);
                    LogMessage("Serialization assemblies merged", MessageImportance.Normal);
                }
            }
            return true;
        }
        
        public string Generate(string target, string workingDirectory, bool delaySign, string keyFile, string[] references)
        {
            XmlSerializer serializer = null;
            var errors = new List<Exception>();
            var result = CodeGenerator.Generate(target, workingDirectory, delaySign, keyFile, references, e =>
            {
                serializer = serializer ?? new System.Xml.Serialization.XmlSerializer(typeof(CompilerError));
                errors.Add(new InvalidOperationException(GetText(serializer, e)));
            });
            if (errors.Count > 0)
                throw new AggregateException(errors);
            return result;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                return Assembly.Load(args.Name);
            }
            catch { return null; }
        }

        private CompilerError FromText(System.Xml.Serialization.XmlSerializer serializer, string text)
        {
            return (CompilerError)serializer.Deserialize(new System.IO.StringReader(text));
        }

        private string GetText(System.Xml.Serialization.XmlSerializer serializer,CompilerError e)
        {
            var result = new System.IO.StringWriter();
            serializer.Serialize(result, e);
            return result.ToString();
        }

        private static void MergeAssemblies(bool delaySign, string keyFile, params string[] inputAssemblies)
        {
            var repack = new ILRepacking.ILRepack();
            repack.DebugInfo = true;
            repack.DelaySign = delaySign;
            repack.KeyFile = keyFile;
            repack.NoRepackRes = true;
            repack.InputAssemblies = inputAssemblies;
            repack.OutputFile = inputAssemblies[0];
            repack.Repack();
        }

        private void LogMessage(string message, MessageImportance importance)
        {
            if (BuildEngine != null)
            {
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message, null, null, importance));
            }
        }

        private void LogError(string message, string code, string file,  int lineNumber, int columnNumber)
        {
            if (BuildEngine != null)
            {
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("BinarySerialization", code, file, lineNumber, columnNumber, lineNumber, columnNumber, message, null, null));
            }
        }

        private void LogWarning(string message, string code, string file, int lineNumber, int columnNumber)
        {
            if (BuildEngine != null)
            {
                BuildEngine.LogWarningEvent(new BuildWarningEventArgs("BinarySerialization", code, file, lineNumber, columnNumber, lineNumber, columnNumber, message, null, null));
            }
        }
    }
}
