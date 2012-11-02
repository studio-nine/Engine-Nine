namespace Nine.Studio
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Nine.Studio.Extensibility;

    /// <summary>
    /// Represents project that manages multiple documents.
    /// </summary>
    public class Project : ObservableObject, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the fileName of this project.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the absolute fileName of this project with full path.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the absolute directory of this project.
        /// </summary>
        public string Directory { get; private set; }
        
        /// <summary>
        /// Gets whether this project is ready only.
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Gets whether this project is modified since last save.
        /// </summary>
        public bool IsModified
        {
            get { return isModified; }
            set
            {
                if (value != isModified)
                {
                    isModified = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private bool isModified;

        /// <summary>
        /// Gets the containing editor instance.
        /// </summary>
        public Editor Editor { get; private set; }
        
        /// <summary>
        /// Gets a list of documents owned by this project.
        /// </summary>
        public ReadOnlyObservableCollection<ProjectItem> ProjectItems { get; private set; }
        internal ObservableCollection<ProjectItem> InnerProjectItems;

        /// <summary>
        /// Gets a collection of projects that is referenced by this project.
        /// </summary>
        public ReadOnlyObservableCollection<Project> References { get; private set; }
        internal ObservableCollection<Project> InnerReferences;
        #endregion

        #region Methods
        private static readonly string ProjectExtension = ".nine";

        private Project(Editor editor)
        {
            Verify.IsNotNull(editor, "editor");
            this.Editor = editor;
            this.InnerProjectItems = new ObservableCollection<ProjectItem>();
            this.InnerReferences = new ObservableCollection<Project>();
            this.ProjectItems = new ReadOnlyObservableCollection<ProjectItem>(InnerProjectItems);
            this.References = new ReadOnlyObservableCollection<Project>(InnerReferences);
        }

        /// <summary>
        /// Creates an empty project in the editor.
        /// </summary>
        public Project(Editor editor, string directory, string fileName) : this(editor)
        {
            Verify.IsValidPath(directory, "directory");
            Verify.IsValidFileName(fileName, "fileName");
            
            if (!Path.HasExtension(fileName))
                fileName += ProjectExtension;
            FileName = Path.GetFullPath(Path.Combine(directory, fileName));
            Name =  Path.GetFileNameWithoutExtension(FileName);
            Directory = Path.GetDirectoryName(FileName);
        }

        /// <summary>
        /// Creates a project from a existing file in the editor.
        /// </summary>
        public Project(Editor editor, string fileName) : this(editor)
        {
            Verify.FileExists(fileName, "fileName");

            FileName = Path.GetFullPath(fileName);
            Name = Path.GetFileNameWithoutExtension(FileName);
            Directory = Path.GetDirectoryName(FileName);

            Load();
            FileInfo info = new FileInfo(FileName);
            IsReadOnly = info.Exists && info.IsReadOnly;
        }

        /// <summary>
        /// Adds a new reference by this project.
        /// </summary>
        public void AddReference(Project project)
        {
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
            Load(FileName);
        }

        /// <summary>
        /// Loads the project from the input file.
        /// </summary>
        private void Load(string fileName)
        {
            Verify.IsNeitherNullNorEmpty(fileName, "fileName");

            using (FileStream stream = new FileStream(fileName ?? FileName, FileMode.Open))
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
                    var fileName = doc.Attribute("FileName").Value;

                    Editor.NotifyProgressChanged(fileName, 1.0f * (i + 1) / documents.Length);

                    var document = Import(Path.Combine(Directory, fileName));
                    var docReferences = doc.Descendants("References").Where(e => e.Parent == doc).FirstOrDefault();
                    if (docReferences != null)
                    {
                        foreach (var docReference in docReferences.Descendants("Reference"))
                        {
                            var projectName = docReference.Attribute("Project").Value;
                            var referenceProject = projectName == null ? this : Editor.OpenProject(projectName);
                            document.AddReference(referenceProject.Import(docReference.Attribute("FileName").Value));
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
            if (IsModified)
                FileHelper.BackupAndSave(FileName, Save);
            
            Editor.AddRecentProject(FileName);
        }

        /// <summary>
        /// Saves the project to the output stream.
        /// </summary>
        private void Save(Stream stream)
        {
            

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
                    Trace.TraceError("Failed saving {0}", projectItem.FileName);
                    Trace.WriteLine(e.ToString());
                    throw;
                }
            }

            IsModified = false;
        }

        private void SaveProject(Stream stream)
        {
            new XDocument(
                new XElement("Project", new XAttribute("Version", Global.VersionString),
                    new XElement("ProjectItems", from doc in ProjectItems select 
                        new XElement("ProjectItem",
                            new XAttribute("FileName", doc.RelativeFilename),
                            new XElement("References", from reference in doc.References select
                                new XElement("Reference", new XAttribute("FileName", reference.FileName)
                                                        , new XAttribute("Project", reference.Project.FileName))))))).Save(stream);

        }
        #endregion

        #region ProjectItems
        /// <summary>
        /// Creates a new document with the specified factory type name.
        /// </summary>
        public ProjectItem CreateProjectItem(string factoryTypeName)
        {
            
            Verify.IsNeitherNullNorEmpty(factoryTypeName, "factoryTypeName");
            return CreateProjectItem(Editor.Extensions.Factories.Single(x => x.Value.GetType().Name == factoryTypeName).Value);
        }

        /// <summary>
        /// Creates a new document with the specified factory.
        /// </summary>
        public ProjectItem CreateProjectItem(IFactory factory)
        {
            
            Verify.IsNotNull(factory, "factory");
            return CreateProjectItem(factory.Create(Editor, this));
        }

        /// <summary>
        /// Creates a new document with the specified object.
        /// </summary>
        public ProjectItem CreateProjectItem(object objectModel)
        {
            
            Verify.IsNotNull(objectModel, "objectModel");
            
            ProjectItem document = new ProjectItem(this, objectModel, null);
            InnerProjectItems.Add(document);
            IsModified = true;
            Trace.TraceInformation("ProjectItem {0} of type {1} created at {2}", document.Name, document.Metadata.DisplayName, document.FileName);            
            return document;
        }

        /// <summary>
        /// Opens a document from file with the specified serializer
        /// </summary>
        public ProjectItem Import(string fileName, IImporter importer)
        {
            Verify.IsNotNull(importer, "importer");
            Verify.IsValidPath(fileName, "fileName");

            //ImporterParameters = ReflectionHelper.SaveProperties(importer);

            using (FileStream stream = File.OpenRead(fileName))
            {
                List<string> dependencies = new List<string>();
                var objectModel = importer.Import(stream, dependencies);
                Verify.IsNotNull(objectModel, "objectModel");

                ProjectItem document = new ProjectItem(this, objectModel, fileName);
                dependencies.ForEach(d => document.AddReference(Import(d)));
                InnerProjectItems.Add(document);
                IsModified = true;
                return document;
            }
        }

        /// <summary>
        /// Imports a document from file
        /// </summary>
        public ProjectItem Import(string fileName)
        {
            Verify.FileExists(fileName, "fileName");

            List<string> dependencies = new List<string>();
            using (FileStream stream = File.OpenRead(fileName))
            {
                object documentObject = null;
                string ext = Path.GetExtension(fileName).ToLowerInvariant();

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
                            Trace.TraceInformation("Failed loading {0} using {1}", fileName, documentSerializer.GetType());

                            Trace.WriteLine(e);
                        }
                    }
                }

                if (documentObject == null)
                    throw new InvalidOperationException(string.Format("Failed loading {0}", fileName));

                ProjectItem document = new ProjectItem(this, documentObject, fileName);
                dependencies.ForEach(d => document.AddReference(Import(d)));
                InnerProjectItems.Add(document);
                IsModified = true;
                return document;
            }
        }
        #endregion
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }
        #endregion
    }
}