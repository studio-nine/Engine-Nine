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
            using (var stream = new FileStream(Path.Combine(Path.GetTempPath(), Path.GetTempFileName()), FileMode.Create))
            {
                var writer = new StreamWriter(stream);
                writer.Write(effectCode);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                return ContentPipeline.LoadContent<CompiledEffectContent>(stream
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
