namespace Nine.Studio.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Generic base class implementing IImporter and IExporter
    /// </summary>
    public abstract class Serializer<T> : IImporter, IExporter
    {
        /// <summary>
        /// Gets the dependencies.
        /// </summary>
        public ICollection<string> Dependencies { get; private set; }

        /// <summary>
        /// Gets the file extensions supported by this serializer.
        /// </summary>
        public ICollection<string> FileExtensions { get; private set; }

        /// <summary>
        /// Gets the target type that can be serialized by this importer.
        /// </summary>
        public Type TargetType { get { return typeof(T); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Serializer&lt;T&gt;"/> class.
        /// </summary>
        public Serializer()
        {
            FileExtensions = new HashSet<string>();
        }

        IEnumerable<string> IImporter.FileExtensions
        {
            get { return FileExtensions; }
        }

        IEnumerable<string> IExporter.FileExtensions
        {
            get { return FileExtensions; }
        }

        /// <summary>
        /// Checks the stream header to determine if it is supported by this importer.
        /// </summary>
        public virtual bool CheckSupported(byte[] header)
        {
            return true;
        }

        object IImporter.Import(Stream input, ICollection<string> dependencies)
        {
            Verify.IsNotNull(input, "input");
            Verify.IsNotNull(dependencies, "dependencies");

            Dependencies = dependencies;

            object value = Deserialize(input);
            Verify.IsAssignableFrom(value, typeof(T), "value");

            return value;
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        protected abstract T Deserialize(Stream input);

        void IExporter.Export(Stream output, object value)
        {
            Verify.IsNotNull(output, "output");
            Verify.IsAssignableFrom(value, typeof(T), "value");

            Serialize(output, (T)value);
        }

        /// <summary>
        /// Serializes the specified output.
        /// </summary>
        protected abstract void Serialize(Stream output, T value);
    }
}
