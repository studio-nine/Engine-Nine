namespace Nine.Graphics
{
    using System;
    using System.IO;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Serialization;

    class PipelineWriter<T> : BinaryObjectWriter<T, XnbReader>
    {
        private PipelineBuilder builder;

        public override void Write(BinaryWriter output, T value, IServiceProvider serviceProvider)
        {
            if (builder == null)
                builder = new PipelineBuilder(serviceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice);
            builder.Compile(output.BaseStream, value);
        }
    }

    class ModelWriter : PipelineWriter<Microsoft.Xna.Framework.Graphics.Model> { }
}
