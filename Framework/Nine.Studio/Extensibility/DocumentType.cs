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
    /// Represents a document type that can be created, opened, saved or edited.
    /// </summary>
    public interface IDocumentType
    {
        /// <summary>
        /// Gets a friendly name of the document type.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets a short description of the document type.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the icon for this document type.
        /// </summary>
        /// <remarks>
        /// The icon can be a <c>System.Drawing.Bitmap</c> or <c>System.Windows.Media.ImageSource</c>.
        /// </remarks>
        object Icon { get; }

        /// <summary>
        /// Gets the default serializer for this document type.
        /// </summary>
        Type DefaultSerializer { get; }

        /// <summary>
        /// Creates a new object of this document type.
        /// </summary>
        object CreateDocumentObject();
    }

    /// <summary>
    /// Generic base class implementing IDocumentType
    /// </summary>
    public abstract class DocumentType<T> : IDocumentType
    {
        public string DisplayName { get; protected set; }
        public string Description { get; protected set; }
        public object Icon { get; protected set; }

        public Type DefaultSerializer { get; protected set; }

        public virtual object CreateDocumentObject()
        {
            return Activator.CreateInstance<T>();
        }
    }

    class EmptyDocumentType : IDocumentType
    {
        public string DisplayName
        {
            get { return Strings.Misc; }
        }

        public string Description
        {
            get { return ""; }
        }

        public object Icon
        {
            get { return null; }
        }

        public Type DefaultSerializer
        {
            get { return null; }
        }

        public object CreateDocumentObject()
        {
            return null;
        }
    }
}
