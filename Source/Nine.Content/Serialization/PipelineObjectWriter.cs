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
            string cachedFileName;
            if (!ContentPipeline.ObjectCache.TryGetValue(new WeakReference(value), out cachedFileName))
                throw new InvalidOperationException();

            var bytes = File.ReadAllBytes(cachedFileName);
            output.Write(bytes, 0, bytes.Length);
        }
    }
}
