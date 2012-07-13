#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Nine.Studio.Extensibility
{
    /// <summary>
    /// Represents a importer that can load an object from a stream.
    /// </summary>
    public interface IImporter
    {
        /// <summary>
        /// Gets the file extensions supported by this serializer.
        /// </summary>
        IEnumerable<string> FileExtensions { get; }

        /// <summary>
        /// Gets the target type that can be serialized by this importer.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Checks the stream header to determine if it is supported by this importer.
        /// </summary>
        bool CheckSupported(byte[] header);

        /// <summary>
        /// Imports an object from a stream.
        /// </summary>
        object Import(Stream input, ICollection<string> dependencies);
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
            // Force graphics device service to be initialized upfront to walkaround a bug in Win8.
            graphicsDevice = Nine.Graphics.GraphicsDeviceService.AddRef().GraphicsDevice;
        }

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
        /// Initializes a new instance of the <see cref="Importer&lt;T&gt;"/> class.
        /// </summary>
        public Importer()
        {
            FileExtensions = new HashSet<string>();
        }

        IEnumerable<string> IImporter.FileExtensions
        {
            get { return FileExtensions; }
        }

        /// <summary>
        /// Checks the stream header to determine if it is supported by this importer.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public virtual bool CheckSupported(byte[] header)
        {
            return true;
        }

        object IImporter.Import(Stream input, ICollection<string> dependencies)
        {
            Verify.IsNotNull(input, "input");
            Verify.IsNotNull(dependencies, "dependencies");     

            Dependencies = dependencies;
            
            object value = Import(input);
            Verify.IsAssignableFrom(value, typeof(T), "value");

            return value;
        }

        /// <summary>
        /// Imports from the specified input stream.
        /// </summary>
        protected abstract T Import(Stream input);
    }
}
