namespace Nine.Studio.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Represents a importer that can load an object from a stream.
    /// </summary>
    public interface IImporter
    {
        /// <summary>
        /// Gets the target type that can be serialized by this importer.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Gets the file extensions supported by this importer.
        /// </summary>
        IEnumerable<string> GetSupportedFileExtensions();

        /// <summary>
        /// Imports an object from a stream.
        /// </summary>
        object Import(string fileName, ICollection<string> dependencies);
    }

    /// <summary>
    /// Generic base class implementing IImporter
    /// </summary>
    public abstract class Importer<T> : IImporter
    {
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }
        private static GraphicsDevice graphicsDevice;

        static Importer()
        {
            // Force graphics device service to be initialized upfront to walk around a bug in Win8.
            graphicsDevice = Nine.Graphics.GraphicsDeviceService.AddRef().GraphicsDevice;
        }

        protected ICollection<string> Dependencies { get; private set; }

        public Type TargetType { get { return typeof(T); } }

        public abstract IEnumerable<string> GetSupportedFileExtensions();

        object IImporter.Import(string fileName, ICollection<string> dependencies)
        {
            Verify.FileExists(fileName, "fileName");
            Verify.IsNotNull(dependencies, "dependencies");     

            Dependencies = dependencies;

            object value = Import(fileName);
            Verify.IsAssignableFrom(value, typeof(T), "value");

            return value;
        }

        /// <summary>
        /// Imports from the specified input stream.
        /// </summary>
        protected abstract T Import(string fileName);
    }
}
