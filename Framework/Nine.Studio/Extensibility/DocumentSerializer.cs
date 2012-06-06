#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Nine.Studio.Extensibility
{
    /// <summary>
    /// Represents a document serializer that can load and save an object from a stream.
    /// </summary>
    public interface IDocumentSerializer
    {
        /// <summary>
        /// Gets the display name of this serializer.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the file extensions supported by this serializer.
        /// </summary>
        IEnumerable<string> FileExtensions { get; }

        /// <summary>
        /// Gets the target type that can be serialized by this IDocumentSerializer.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Gets whether this IDocumentSerializer can serialize an object to an output stream.
        /// </summary>
        bool CanSerialize { get; }

        /// <summary>
        /// Gets whether this IDocumentSerializer can deserialize an object from a input stream.
        /// </summary>
        bool CanDeserialize { get; }

        /// <summary>
        /// Checks the stream header to determine if it is supported by this document type.
        /// </summary>
        bool CheckSupported(Stream stream);

        /// <summary>
        /// Serializes the object into a stream.
        /// </summary>
        void Serialize(Stream output, object value);

        /// <summary>
        /// Deserializes an object from a stream.
        /// </summary>
        object Deserialize(Stream input);
    }

    /// <summary>
    /// Generic base class implementing IDocumentSerializer
    /// </summary>
    public abstract class DocumentSerializer<T> : IDocumentSerializer
    {
        public string DisplayName { get; protected set; }
        public ICollection<string> FileExtensions { get; private set; }
        public object Icon { get; protected set; }
        public bool CanSerialize { get; protected set; }
        public bool CanDeserialize { get; protected set; }

        public Type TargetType { get { return typeof(T); } }

        public DocumentSerializer()
        {
            CanSerialize = true;
            CanDeserialize = true;
            DisplayName = GetType().ToString();
            FileExtensions = new HashSet<string>();
        }

        IEnumerable<string> IDocumentSerializer.FileExtensions
        {
            get { return FileExtensions; }
        }

        public virtual bool CheckSupported(Stream stream)
        {
            return true;
        }

        void IDocumentSerializer.Serialize(Stream output, object value)
        {
            if (!(value is T))
            {
                throw new InvalidCastException(
                    string.Format("Cannot cast from {0} to {1}", value.GetType(), typeof(T)));
            }
            Serialize(output, (T)value);
        }

        object IDocumentSerializer.Deserialize(Stream input)
        {
            object value = Deserialize(input);
            if (!(value is T))
            {
                throw new InvalidCastException(
                    string.Format("Cannot cast from {0} to {1}", value.GetType(), typeof(T)));
            }
            return value;
        }

        protected abstract void Serialize(Stream output, T value);
        protected abstract T Deserialize(Stream input);
    }
}
