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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Nine.Studio.Extensibility;
using System.ComponentModel;
#endregion

namespace Nine.Studio
{
    /// <summary>
    /// Represents project that manages multiple documents.
    /// </summary>
    public class Project : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Gets the filename of this project.
        /// </summary>
        public string Name { get; private set; }

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
        public bool IsModified
        {
            get { return _IsModified; }
            set
            {
                if (value != _IsModified)
                {
                    _IsModified = value;
                    NotifyPropertyChanged("IsModified");
                }
            }
        }
        private bool _IsModified;
        
        /// <summary>
        /// Gets a list of documents owned by this project.
        /// </summary>
        public ReadOnlyObservableCollection<ProjectItem> ProjectItems { get; private set; }

        /// <summary>
        /// Gets the containing editor instance.
        /// </summary>
        public Editor Editor { get; private set; }

        /// <summary>
        /// Gets a collection of projects that is referenced by this project.
        /// </summary>
        public ReadOnlyObservableCollection<Project> References { get; private set; }

        /// <summary>
        /// Gets or sets any user data that will be saved with the project file.
        /// </summary>
        public object Tag { get; set; }

        internal ObservableCollection<Project> InnerReferences;
        internal ObservableCollection<ProjectItem> InnerProjectItems;

        /// <summary>
        /// Initializes a new project instance.
        /// </summary>
        internal Project(Editor editor, string filename)
        {
            Assert.CheckThread();
            Verify.IsNotNull(editor, "editor");
            Verify.IsNeitherNullNorEmpty(filename, "filename");

            this.Editor = editor;
            this.IsModified = false;
            this.InnerProjectItems = new ObservableCollection<ProjectItem>();
            this.InnerReferences = new ObservableCollection<Project>();
            this.ProjectItems = new ReadOnlyObservableCollection<ProjectItem>(InnerProjectItems);
            this.References = new ReadOnlyObservableCollection<Project>(InnerReferences);

            if (!Path.HasExtension(filename))
                filename = filename + Global.ProjectExtension;
            Filename = Path.GetFullPath(filename);
            Name =  Path.GetFileNameWithoutExtension(Filename);
            if (File.Exists(Filename))
            {
                Load();
                FileInfo info = new FileInfo(Filename);
                IsReadOnly = info.Exists && info.IsReadOnly;
            }
        }

        /// <summary>
        /// Adds a new reference by this project.
        /// </summary>
        public void AddReference(Project project)
        {
            Assert.CheckThread();
            Verify.IsNotNull(project, "project");

            if (HasCircularDependency(project, this))
                throw new InvalidOperationException("Target object has a circular dependency");

            if (!InnerReferences.Contains(project))
                InnerReferences.Add(project);
        }

        /// <summary>
        /// Removes an existing reference by this project
        /// </summary>
        public void RemoveReference(Project project)
        {
            Assert.CheckThread();
            Verify.IsNotNull(project, "project");

            InnerReferences.Remove(project);
        }

        private static bool HasCircularDependency(Project subtree, Project root)
        {
            return !subtree.InnerReferences.All(node => node != root && !HasCircularDependency(node, root));
        }

        /// <summary>
        /// Closes the project.
        /// </summary>
        public void Close()
        {
            Assert.CheckThread();

            // Can't close this when any project references this one
            if (Editor.Projects.Any(project => project.References.Contains(this)))
                throw new InvalidOperationException("Cannot close project because it is referenced by other projects");

            References.ForEach(project => project.Close());
            ProjectItems.ForEach(doc => doc.Close());
            Editor.CloseProject(this);
        }

        #region Serialization
        /// <summary>
        /// Loads the project.
        /// </summary>
        private void Load()
        {
            Load(Filename);
        }

        /// <summary>
        /// Loads the project from the input file.
        /// </summary>
        private void Load(string filename)
        {
            Verify.IsNeitherNullNorEmpty(filename, "filename");

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
            var root = xDoc.Descendants("Project").First();
            
            if (Global.VersionString != root.Attribute("Version").Value)
            {
                throw new InvalidOperationException(
                    string.Format("Invalid version. Expected {0}, got {1}",
                    Global.VersionString, root.Attribute("Version").Value));
            }

            var documentsNode = root.Descendants("ProjectItems").Where(e => e.Parent == root).FirstOrDefault();
            if (documentsNode != null)
            {
                var documents = documentsNode.Descendants("ProjectItem").Where(e => e.Parent == documentsNode).ToArray();
                for (int i = 0; i < documents.Length; i++)
                {
                    var doc = documents[i];
                    var filename = doc.Descendants("Filename").Where(e => e.Parent == doc).First().Value;

                    Editor.NotifyProgressChanged(filename, 1.0f * (i + 1) / documents.Length);

                    var document = Import(Path.Combine(Path.GetDirectoryName(Filename), filename));
                    var docReferences = doc.Descendants("References").Where(e => e.Parent == doc).FirstOrDefault();
                    if (docReferences != null)
                    {
                        foreach (var docReference in docReferences.Descendants("Reference"))
                        {
                            var projectName = docReference.Descendants("Project").FirstOrDefault();
                            var referenceProject = projectName == null ? this : Editor.OpenProject(projectName.Value);
                            document.AddReference(referenceProject.Import(docReference.Descendants("Filename").First().Value));
                        }
                    }
                    document.Tag = LoadTag(doc);
                }
            }

            var referencesNode = root.Descendants("References").Where(e => e.Parent == root).FirstOrDefault();
            if (referencesNode != null)
            {
                foreach (var reference in referencesNode.Descendants("Reference"))
                {
                    AddReference(Editor.OpenProject(reference.Value));
                }
            }
           
            Tag = LoadTag(root);

            IsModified = false;
        }

        private object LoadTag(XElement node)
        {
            var tag = node.Descendants("Tag").Where(e => e.Parent == node).FirstOrDefault();
            if (tag != null)
            {
                Type type = Type.GetType(tag.Attribute("Type").Value);
                return new XmlSerializer(type).Deserialize(tag.Descendants().First().CreateReader());
            }
            return null;
        }

        /// <summary>
        /// Saves the project.
        /// </summary>
        public void Save()
        {
            Assert.CheckThread();
            Assert.IsNeitherNullNorEmpty(Filename);

            if (IsModified)
                FileOperation.BackupAndSave(Filename, Save);
            
            Editor.RecentProject(Filename);
        }

        /// <summary>
        /// Saves the project to the output stream.
        /// </summary>
        private void Save(Stream stream)
        {
            Assert.CheckThread();

            SaveProject(stream);

            foreach (ProjectItem projectItem in ProjectItems)
            {
                try
                {
                    if (projectItem.IsModified && projectItem.CanSave)
                        projectItem.Save();
                }
                catch (Exception e)
                {
                    Trace.TraceError("Failed saving {0}", projectItem.Filename);
                    Trace.WriteLine(e.ToString());
                    throw;
                }
            }

            IsModified = false;
        }

        private void SaveProject(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(stream, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("Project");
            writer.WriteAttributeString("Version", Global.VersionString);
            WriteProjectItems(writer);
            WriteReferences(writer);
            WriteTag(writer, Tag);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
        }

        private void WriteProjectItems(XmlWriter writer)
        {
            if (ProjectItems.Count > 0)
            {
                writer.WriteStartElement("ProjectItems");
                foreach (var doc in ProjectItems)
                {
                    Assert.IsNeitherNullNorEmpty(doc.Filename);

                    writer.WriteStartElement("ProjectItem");
                    writer.WriteElementString("Filename", doc.RelativeFilename);
                    if (doc.References.Count > 0)
                    {
                        writer.WriteStartElement("References");
                        foreach (var reference in doc.References)
                        {
                            writer.WriteStartElement("Reference");
                            writer.WriteElementString("Filename", reference.Filename);
                            if (reference.Project != this)
                                writer.WriteElementString("Project", reference.Project.Filename);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
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
                    writer.WriteElementString("Reference", reference.Filename);
                writer.WriteEndElement();
            }
        }

        private void WriteTag(XmlWriter writer, object tag)
        {
            if (tag != null && tag.GetType().IsSerializable)
            {
                writer.WriteStartElement("Tag");
                writer.WriteAttributeString("Type", tag.GetType().AssemblyQualifiedName);
                new XmlSerializer(tag.GetType()).Serialize(writer, tag);
                writer.WriteEndElement();
                writer.Flush();
            }
        }
        #endregion

        #region ProjectItems
        /// <summary>
        /// Creates a new document with the specified factory type name.
        /// </summary>
        public ProjectItem CreateProjectItem(string factoryTypeName)
        {
            Assert.CheckThread();
            Verify.IsNeitherNullNorEmpty(factoryTypeName, "factoryTypeName");

            return CreateProjectItem(Editor.Extensions.Factories.Single(x => x.Value.GetType().Name == factoryTypeName).Value);
        }

        /// <summary>
        /// Creates a new document with the specified factory.
        /// </summary>
        public ProjectItem CreateProjectItem(IFactory factory)
        {
            Assert.CheckThread();
            Verify.IsNotNull(factory, "factory");
            return CreateProjectItem(factory.Create(Editor, this));
        }

        /// <summary>
        /// Creates a new document with the specified object.
        /// </summary>
        public ProjectItem CreateProjectItem(object objectModel)
        {
            Assert.CheckThread();
            Verify.IsNotNull(objectModel, "objectModel");
            
            ProjectItem document = new ProjectItem(this, objectModel, null);
            InnerProjectItems.Add(document);
            IsModified = true;
            Trace.TraceInformation("ProjectItem {0} of type {1} created at {2}", document.Name, document.Metadata.DisplayName, document.Filename);            
            return document;
        }

        /// <summary>
        /// Opens a document from file with the specified serializer
        /// </summary>
        public ProjectItem Import(string filename, IImporter importer)
        {
            Assert.CheckThread();
            Verify.IsNotNull(importer, "importer");
            Verify.IsValidPath(filename, "filename");

            //ImporterParameters = ReflectionHelper.SaveProperties(importer);

            using (FileStream stream = File.OpenRead(filename))
            {
                List<string> dependencies = new List<string>();
                var objectModel = importer.Import(stream, dependencies);
                Verify.IsNotNull(objectModel, "objectModel");

                ProjectItem document = new ProjectItem(this, objectModel, filename);
                dependencies.ForEach(d => document.AddReference(Import(d)));
                InnerProjectItems.Add(document);
                IsModified = true;
                return document;
            }
        }

        /// <summary>
        /// Imports a document from file
        /// </summary>
        public ProjectItem Import(string filename)
        {
            Assert.CheckThread();
            Verify.FileExists(filename, "filename");

            List<string> dependencies = new List<string>();
            using (FileStream stream = File.OpenRead(filename))
            {
                object documentObject = null;
                string ext = Path.GetExtension(filename).ToLowerInvariant();

                byte[] tempHeader = new byte[Constants.MaxHeaderBytes];                
                int count = stream.Read(tempHeader, 0, tempHeader.Length);
                stream.Seek(0, SeekOrigin.Begin);
                byte[] header = new byte[count];
                Array.Copy(tempHeader, header, count);

                var importers = Editor.Extensions.Importers.Where(i => i.Value.MatchFileExtension(ext)).Concat(
                                Editor.Extensions.Importers.Where(i => (i.Value.FileExtensions == null || i.Value.FileExtensions.Count() == 0)))
                                                 .Select(i => i.Value).ToList();

                foreach (IImporter documentSerializer in importers)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    if (documentSerializer.CheckSupported(tempHeader))
                    {
                        try
                        {
                            dependencies.Clear();
                            documentObject = documentSerializer.Import(stream, dependencies);
                            break;
                        }
                        catch (Exception e)
                        {
                            Trace.TraceInformation("Failed loading {0} using {1}", filename, documentSerializer.GetType());
                            Trace.WriteLine(e);
                        }
                    }
                }

                if (documentObject == null)
                    throw new InvalidOperationException(string.Format("Failed loading {0}", filename));

                ProjectItem document = new ProjectItem(this, documentObject, filename);
                dependencies.ForEach(d => document.AddReference(Import(d)));
                InnerProjectItems.Add(document);
                IsModified = true;
                return document;
            }
        }
        #endregion
        
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            Assert.CheckThread();

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}
