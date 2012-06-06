#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio
{
    /// <summary>
    /// Represents a single document managed by an editor instance.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Gets the filename of this document.
        /// </summary>
        public string Name { get { return Filename != null ? Path.GetFileName(Filename) : null; } }

        /// <summary>
        /// Gets the absolute filename of this document with full path.
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Gets whether this document is ready only.
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Gets whether this document is modified since last save.
        /// </summary>
        public bool IsModified { get; set; }

        /// <summary>
        /// Gets whether this document can be saved to the output stream.
        /// </summary>
        public bool CanSave { get { return serializer != null; } }

        /// <summary>
        /// Gets the document type.
        /// </summary>
        public IDocumentType DocumentType { get; private set; }

        /// <summary>
        /// Gets the underlying document object.
        /// </summary>
        public object DocumentObject { get; private set; }

        /// <summary>
        /// Gets the handle to the host window.
        /// </summary>
        public IntPtr Window { get; private set; }

        /// <summary>
        /// Gets the containing project instance.
        /// </summary>
        public Project Project { get; private set; }
         
        /// <summary>
        /// Gets the containing editor instance.
        /// </summary>
        public Editor Editor { get { return Project.Editor; } }

        /// <summary>
        /// Gets a list of documents referenced by this document.
        /// </summary>
        public ICollection<Document> References { get; private set; }

        /// <summary>
        /// Gets or sets any user data that will be saved with the project file.
        /// </summary>
        public object Tag { get; set; }

        private IDocumentSerializer serializer;

        /// <summary>
        /// Initializes a new document instance.
        /// </summary>
        internal Document(Project project, object documentObject, IDocumentType documentType, string filename)
        {
            this.Project = project;
            this.References = new HashSet<Document>();
            this.DocumentObject = documentObject ?? documentType.CreateDocumentObject();
            this.DocumentType = documentType ?? new EmptyDocumentType();

            if (DocumentObject == null)
            {
                throw new InvalidOperationException(
                    string.Format("{0} CreateDocumentObject cannot return null", documentType.DisplayName));
            }

            if (!string.IsNullOrEmpty(filename))
            {
                Filename = Path.GetFullPath(filename);
            }

            this.IsModified = string.IsNullOrEmpty(filename);
            this.serializer = FindSerializer();
        }

        /// <summary>
        /// Closes this document
        /// </summary>
        public void Close() 
        {
        }
        
        /// <summary>
        /// Saves this document
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(Filename))
                throw new InvalidOperationException("Filename");

            if (IsModified)
                Save(Filename);
        }

        /// <summary>
        /// Saves this document
        /// </summary>
        public void Save(string filename)
        {
            Global.SafeSave(filename ?? Filename, Save);

            if (!string.IsNullOrEmpty(filename))
                Filename = Path.GetFullPath(filename);
        }

        /// <summary>
        /// Saves this document
        /// </summary>
        public void Save(Stream stream)
        {
            if (serializer == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Cannot find document serializer for document type {0}", DocumentType.DisplayName));
            }
            serializer.Serialize(stream, DocumentObject);
            stream.Flush();
        }

        private IDocumentSerializer FindSerializer()
        {
            IDocumentSerializer serializer = null;
            if (DocumentType.DefaultSerializer != null)
            {
                serializer = Editor.Extensions.DocumentSerializers.FirstOrDefault(s =>
                    s.GetType() == DocumentType.DefaultSerializer && s.CanDeserialize);
            }
            if (serializer == null)
            {
                serializer = Editor.Extensions.DocumentSerializers.FirstOrDefault(s =>
                    s.TargetType.IsAssignableFrom(DocumentObject.GetType()) && s.CanDeserialize);
            }
            return serializer;
        }
    }
}
