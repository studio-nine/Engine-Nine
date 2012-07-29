namespace Nine.Content.Pipeline.Graphics.Materials
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
    using Nine.Graphics.Materials;


    [ContentTypeWriter]
    class CustomMaterialWriter : ContentTypeWriter<CustomMaterial>
    {
        protected override void Write(ContentWriter output, CustomMaterial value)
        {
            WriteObject(output, value, "AttachedProperties", value.AttachedProperties);
            output.Write(value.IsTransparent);
            WriteObject(output, value, "Source", value.Source);
            WriteObject(output, value, "Texture", value.texture);
            output.Write(value.IsTransparent);
            output.Write(value.TwoSided);
            WriteObject(output, value, "SamplerState", value.SamplerState);
            
            if (value.Parameters == null)
                output.WriteRawObject<Dictionary<string, object>>(null);
            else
                output.WriteRawObject(new Dictionary<string, object>(value.Parameters));
        }

        private void WriteObject(ContentWriter output, System.Object parent, string member, System.Object value)
        {
            var propertyInstance = new Nine.Content.Pipeline.Xaml.PropertyInstance(parent, member);
            var serializationData = Nine.Content.Pipeline.Xaml.XamlSerializer.SerializationData;
            if (serializationData.ContainsKey(propertyInstance))
                output.WriteObject(serializationData[propertyInstance]);
            else
                output.WriteObject(value);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(CustomMaterialReader).AssemblyQualifiedName;
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(CustomMaterial).AssemblyQualifiedName;
        }
    }
}
