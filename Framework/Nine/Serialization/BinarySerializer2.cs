namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;

    abstract class BinaryObjectWriter<T, TReader> : IBinaryObjectWriter
    {
        public Type TargetType
        {
            get { return typeof(T); }
        }

        public Type ReaderType
        {
            get { return typeof(TReader); }
        }

        public abstract void Write(BinaryWriter output, T value, IServiceProvider serviceProvider);

        void IBinaryObjectWriter.Write(BinaryWriter output, object value, IServiceProvider serviceProvider)
        {
            Write(output, (T)value, serviceProvider);
        }
    }

    public partial class BinarySerializer : IContentImporter
    {
        object IContentImporter.Import(Stream stream, IServiceProvider serviceProvider)
        {
            return Load(stream, serviceProvider);
        }

        string[] IContentImporter.SupportedFileExtensions
        {
            get { return SupportedFileExtensions; }
        }
        static readonly string[] SupportedFileExtensions = new[] { ".n" };
    }
}