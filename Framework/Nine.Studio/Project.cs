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
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;
using Nine.Studio.Extensibility;
using System.Xml.Linq;
#endregion

namespace Nine.Studio
{
    /// <summary>
    /// Represents project that manages multiple documents.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Gets the filename of this project.
        /// </summary>
        public string Name { get { return Filename != null ? Path.GetFileName(Filename) : null; } }

        /// <summary>
        /// Gets the absolute filename of this project with full path.
        /// </summary>
        public string Filename { get; private set; }
        
        /// <summary>
        /// Gets whether this project is ready only.
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Gets whether this project is modified since last save.
        /// </summary>
        public bool IsModified { get; set; }
        
        /// <summary>
        /// Gets a list of documents owned by this project.
        /// </summary>
        public ICollection<Document> Documents { get; private set; }

        /// <summary>
        /// Gets the containing editor instance.
        /// </summary>
        public Editor Editor { get; private set; }

        /// <summary>
        /// Gets a collection of projects that is referenced by this project.
        /// </summary>
        public ICollection<Project> References { get; private set; }
        
        /// <summary>
        /// Initializes a new project instance.
        /// </summary>
        internal Project(Editor editor, string filename)
        {
            this.Editor = editor;
            this.IsModified = false;
            this.Documents = new HashSet<Document>();
            this.References = new HashSet<Project>();
            if (!string.IsNullOrEmpty(filename))
            {
                Filename = Path.GetFullPath(filename);
                Load();
            }
        }

        /// <summary>
        /// Closes the project.
        /// </summary>
        public void Close()
        {
            Editor.Project = null;
        }

        #region Serialization
        /// <summary>
        /// Loads the project.
        /// </summary>
        private void Load()
        {
            if (string.IsNullOrEmpty(Filename))
                throw new InvalidOperationException("Filename");

            Load(Filename);
        }

        /// <summary>
        /// Loads the project from the input file.
        /// </summary>
        private void Load(string filename)
        {
            using (FileStream stream = new FileStream(filename ?? Filename, FileMode.Open))
            {
                Load(stream);
            }
        }

        /// <summary>
        /// Loads the project from the input stream.
        /// </summary>
        private void Load(Stream stream)
        {
            var xDoc = XDocument.Load(stream);
            var root = xDoc.Descendants("Manifest").First();
            
            if (Global.VersionString != root.Attribute("Version").Value)
            {
                throw new InvalidOperationException(
                    string.Format("Invalid version. Expected {0}, got {1}",
                    Global.VersionString, root.Attribute("Version").Value));
            }

            var documents = root.Descendants("Documents").FirstOrDefault();
            if (documents != null)
            {
                foreach (var doc in documents.Descendants("Document"))
                {
                    OpenDocument(doc.Descendants("Filename").First().Value);
                }
            }

            IsModified = false;
        }

        /// <summary>
        /// Saves the project.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(Filename))
                throw new InvalidOperationException("Filename");
            if (IsModified)
                Save(Filename);
        }

        /// <summary>
        /// Saves the project to the output file.
        /// </summary>
        public void Save(string filename)
        {
            Global.SafeSave(filename ?? Filename, Save);

            if (!string.IsNullOrEmpty(filename))
                Filename = Path.GetFullPath(filename);
        }

        /// <summary>
        /// Saves the project to the output stream.
        /// </summary>
        public void Save(Stream stream)
        {
            IsModified = false;

            SaveProject(stream);

            foreach (Document document in Documents)
            {
                try
                {
                    if (document.CanSave && document.IsModified)
                        document.Save();
                }
                catch (Exception e)
                {
                    Trace.TraceError("Failed saving document {0}", document.Filename);
                    Trace.WriteLine(e.ToString());
                    throw;
                }
            }
        }

        private void SaveProject(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(stream, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("Manifest");
            writer.WriteAttributeString("Version", Global.VersionString);
            WriteDocuments(writer);
            WriteReferences(writer);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
        }

        private void WriteDocuments(XmlWriter writer)
        {
            if (Documents.Count > 0)
            {
                writer.WriteStartElement("Documents");
                foreach (var doc in Documents)
                {
                    if (string.IsNullOrEmpty(doc.Filename))
                    {
                        if (!doc.CanSave)
                            continue;
                        throw new InvalidOperationException(
                            string.Format("One of the {0} document not saved", doc.DocumentType.DisplayName));
                    }
                    writer.WriteStartElement("Document");
                    writer.WriteElementString("Filename", doc.Filename);
                    if (doc.References.Count > 0)
                    {
                        writer.WriteStartElement("References");
                        foreach (var reference in doc.References)
                            writer.WriteElementString("Reference", reference.Filename);
                    }
                    WriteTag(writer, doc.Tag);
                    writer.WriteEndElement();
                    writer.Flush();
                }
                writer.WriteEndElement();
            }
        }

        private void WriteReferences(XmlWriter writer)
        {
            if (References.Count > 0)
            {
                writer.WriteStartElement("References");
                foreach (var reference in References)
                {
                    if (string.IsNullOrEmpty(reference.Filename))
                    {
                        throw new InvalidOperationException("One of the referenced project not saved");
                    }
                    writer.WriteElementString("Reference", reference.Filename);
                }
                writer.WriteEndElement();
            }
        }

        private void WriteTag(XmlWriter writer, object tag)
        {
            if (tag != null && tag.GetType().IsSerializable)
            {
                new XmlSerializer(tag.GetType()).Serialize(writer, tag);
                writer.Flush();
            }
        }
        #endregion

        #region Documents
        /// <summary>
        /// Creates a new document with the specified document object.
        /// </summary>
        public Document CreateDocument(object documentObject)
        {
            if (documentObject == null)
                throw new ArgumentNullException("documentObject");
            if (documentObject is IDocumentType)
                throw new ArgumentException("documentObject cannot be an instance of IDocumentType");

            Document document = new Document(this, documentObject, null, null);
            Documents.Add(document);
            IsModified = true;
            Trace.TraceInformation(string.Format("Document {0} created at {1}", document.DocumentType.DisplayName,
                                                                                document.Filename));
            return document;
        }

        /// <summary>
        /// Creates a new document with the specified document type.
        /// </summary>
        public Document CreateDocument(IDocumentType documentType)
        {
            if (documentType == null)
                throw new ArgumentNullException("documentType");

            Document document = new Document(this, null, documentType, null);
            Documents.Add(document);
            IsModified = true;
            Trace.TraceInformation(string.Format("Document {0} created at {1}", document.DocumentType.DisplayName,
                                                                                document.Filename));
            return document;
        }

        /// <summary>
        /// Opens a document from file with the specified serializer
        /// </summary>
        public Document OpenDocument(string filename, IDocumentSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            using (FileStream stream = File.OpenRead(filename))
            {
                var documentObject = serializer.Deserialize(stream);
                Document document = new Document(this, documentObject, null, filename);
                Documents.Add(document);
                IsModified = true;
                return document;
            }
        }

        /// <summary>
        /// Opens a document from file
        /// </summary>
        public Document OpenDocument(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename");

            using (FileStream stream = File.OpenRead(filename))
            {
                object documentObject = null;

                string ext = Path.GetExtension(filename).ToLowerInvariant();
                foreach (IDocumentSerializer documentSerializer in Editor.Extensions.DocumentSerializers)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    if (documentSerializer.MatchFileExtension(ext) && documentSerializer.CheckSupported(stream))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        try
                        {
                            documentObject = documentSerializer.Deserialize(stream);
                            break;
                        }
                        catch
                        {
                            Trace.TraceInformation(string.Format("Failed loading {0} using {1}", filename, documentSerializer.DisplayName));
                        }
                    }
                }

                if (documentObject == null)
                {
                    Trace.TraceError(string.Format("Failed loading {0}", filename));
                    return null;
                }
                Document document = new Document(this, documentObject, null, filename);
                Documents.Add(document);
                IsModified = true;
                return document;
            }
        }
        #endregion
    }
}
