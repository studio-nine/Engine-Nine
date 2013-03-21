namespace Nine.Serialization
{
    using System;
    using System.IO;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Content.Pipeline;

    /// <summary>
    /// Defines a generic binary object writer that writes xna content.
    /// </summary>
    public class PipelineObjectWriter<T> : IBinaryObjectWriter
    {
        public Type ReaderType
        {
            get { return typeof(PipelineObjectReader); }
        }

        public Type TargetType
        {
            get { return typeof(T); }
        }
        
        public void Write(BinaryWriter output, object value, IServiceProvider serviceProvider)
        {
            if (value != null)
            {
                object overrideObject;
                var serializationOverride = serviceProvider.TryGetService<ISerializationOverride>();
                if (serializationOverride != null && serializationOverride.TryGetOverride(value, out overrideObject))
                    value = overrideObject;
            }
            var builder = Nine.Graphics.GraphicsResources<PipelineBuilder>.GetInstance(serviceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice);
            builder.Compile(output.BaseStream, value);
        }
    }

    class SpriteFontWriter : PipelineObjectWriter<SpriteFont> { }
    class EffectWriter : PipelineObjectWriter<Effect> { }
}
