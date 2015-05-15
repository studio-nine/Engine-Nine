namespace Nine.Graphics.MaterialsBuilder
{
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            Builder t = new Builder();
            t.Session = new Microsoft.VisualStudio.TextTemplating.TextTemplatingSession();
            t.Session["shaderFolder"] = @"D:\Github\Nine\Source\Nine.Graphics.3D\Shaders";
            t.Initialize();

            string resultText = t.TransformText();

            File.WriteAllText("temp.txt", resultText);
        }
    }
}
