namespace Nine.Content.Pipeline.Processors
{
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Nine.Content.Pipeline.Graphics.Materials;
    using Nine.Content.Pipeline.Xaml;
    using Nine.Graphics.Materials;

    [DefaultContentProcessor]    
    public class CustomMaterialProcessor : ContentProcessor<CustomMaterial, CustomMaterial>
    {
        public override CustomMaterial Process(CustomMaterial input, ContentProcessorContext context)
        {
            if (!string.IsNullOrEmpty(input.ShaderCode))
            {
                if (input.Source != null)
                {
                    context.Logger.LogWarning(null, null, "Replacing custom material shaders");
                    input.Source = null;
                }
                
                var hashString = new StringBuilder();
                var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input.ShaderCode));
                for (int i = 0; i < hash.Length; ++i)
                {
                    hashString.Append(hash[i].ToString("X2"));
                }

                var name = hashString.ToString().ToUpperInvariant();
                var assetName = Path.Combine(ContentProcessorContextExtensions.DefaultOutputDirectory, name);
                var sourceFile = Path.Combine(context.IntermediateDirectory, input.GetType().Name + "-" + name + ".fx");

                File.WriteAllText(sourceFile, input.ShaderCode);

                var source = context.BuildAsset<EffectContent, CustomEffectContent>(
                    new ExternalReference<EffectContent>(sourceFile), "CustomEffectProcessor", null, null, assetName);
                
                XamlSerializer.SerializationData[new PropertyInstance(input, "Source")] =
                    new ContentReference<CompiledEffectContent>(source.Filename);
                input.ShaderCode = null;
            }
            return input;
        }
    }
}
