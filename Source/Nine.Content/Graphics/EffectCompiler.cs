namespace Nine.Graphics.Materials
{
    using System.IO;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Nine.Content.Pipeline;
    using System;

    public static class EffectCompiler
    {
        public static byte[] Compile(string effectCode)
        {
            using (var memoryStream = new MemoryStream())
            {
                var writer = new StreamWriter(memoryStream);
                writer.Write(effectCode);
                writer.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);

                return ContentPipeline.LoadContent<CompiledEffectContent>(memoryStream
                     , new Microsoft.Xna.Framework.Content.Pipeline.EffectImporter()
                     , new EffectProcessor()).GetEffectCode();
            }
        }

        public static void Disassemble(string effectFile, string asmFile)
        {
            string dxSdkDir = Environment.GetEnvironmentVariable("DXSDK_DIR");

            if (!string.IsNullOrEmpty(dxSdkDir))
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = Path.Combine(dxSdkDir, @"Utilities\bin\x86\fxc.exe");
                process.StartInfo.Arguments = "/nologo /Tfx_2_0 /O3 /Fc \"" + asmFile + "\" \"" + effectFile + "\"";
                process.Start();
                process.WaitForExit();
            }
        }
    }
}
