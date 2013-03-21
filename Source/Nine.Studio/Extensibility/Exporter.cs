namespace Nine.Studio.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Represents an exporter that can save an object to a stream.
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Gets the target type that is supported by this exporter.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Gets the file extensions supported by this serializer.
        /// </summary>
        IEnumerable<string> GetSupportedFileExtensions();

        /// <summary>
        /// Exports the object into a stream.
        /// </summary>
        void Export(Stream output, object value);
    }

    /// <summary>
    /// Generic base class implementing IDocumentSerializer
    /// </summary>
    public abstract class Exporter<T> : IExporter
    {
        public Type TargetType { get { return typeof(T); } }
        
        public abstract IEnumerable<string> GetSupportedFileExtensions();

        void IExporter.Export(Stream output, object value)
        {
            Verify.IsNotNull(output, "output");
            Verify.IsAssignableFrom(value, typeof(T), "value");

            Export(output, (T)value);
        }

        protected abstract void Export(Stream output, T value);        
    }
}
