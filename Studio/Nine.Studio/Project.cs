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
        public static readonly string SourceFileExtension = ".nine";
        public static readonly string OutputFileExtension = ".n";

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
        private bool isModified = true;

        /// <summary>
        /// Gets the containing editor instance.
        /// </summary>
        public Editor Editor { get; private set; }

        /// <summary>
        /// Gets or sets the active project item.
        /// </summary>
        public ProjectItem ActiveProjectItem
        {
            get { return activeProjectItem; }
            set 
            {
                if (activeProjectItem != value)
                {
                    if (value != null && (value.Project != this || !ProjectItems.Contains(value)))
                        throw new InvalidOperationException();                 
                    activeProjectItem = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private ProjectItem activeProjectItem;
        
        /// <summary>
        /// Gets a list of documents owned by this project.
        /// </summary>
        public ObservableCollection<ProjectItem> ProjectItems { get; private set; }

        /// <summary>
        /// Gets a collection of projects that is referenced by this project.
        /// </summary>
        public ObservableCollection<Project> References { get; private set; }
        #endregion

        #region Methods
        private Project(Editor editor)
        {
            Verify.IsNotNull(editor, "editor");

            this.Editor = editor;
            this.ProjectItems = new ObservableCollection<ProjectItem>();
            this.References = new ObservableCollection<Project>();
            this.ProjectItems.CollectionChanged += (sender, e) => { isModified = true; };
            this.References.CollectionChanged += (sender, e) => { isModified = true; };
        }

        internal Project(Editor editor, string name, string directory) : this(editor)
        {
            Verify.IsValidPath(directory, "directory");
            Verify.IsValidFileName(name, "name");

            Name = name;
            Directory = Path.GetFullPath(Path.Combine(directory, name));
            FileName = Path.Combine(Directory, name + SourceFileExtension);

            if (System.IO.Directory.Exists(Directory) &&
               (System.IO.Directory.GetFiles(Directory).Length > 0 || 
                System.IO.Directory.GetDirectories(Directory).Length > 0))
            {
                throw new ArgumentException(string.Format(Strings.DirectoryNotEmpty, name));
            }
        }

        internal Project(Editor editor, string fileName) : this(editor)
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
        /// Creates a new project item from a factory.
        /// </summary>
        public ProjectItem Create(IFactory factory)
        {
            Verify.IsNotNull(factory, "factory");

            var result = new ProjectItem(this, factory.Create(this, null));
            ProjectItems.Add(result);
            return result;
        }
        
        /// <summary>
        /// Creates a new project item with the specified object model inside the target project.
        /// </summary>
        public ProjectItem Create(object objectModel)
        {
            var result = new ProjectItem(this, objectModel);
            ProjectItems.Add(result);
            return result;
        }

        /// <summary>
        /// Imports a project item from a source file to this project.
        /// </summary>
        /// <param name="fileName">
        /// The full path to the file to be imported, or a relative path relative to the project directory.
        /// </param>
        public ProjectItem Import(string fileName)
        {
            var fileExtension  = FileHelper.NormalizeExtension(Path.GetExtension(fileName));
            var importer = Editor.Extensions.Importers.FirstOrDefault(
                i => i.Value.GetSupportedFileExtensions().Any(
                    ext => FileHelper.NormalizeExtension(ext) == fileExtension));

            return importer != null ? Import(fileName, importer.Value) : null;
        }

        /// <summary>
        /// Imports a project item from a source file to this project.
        /// </summary>
        public ProjectItem Import(string fileName, IImporter importer)
        {
            var result = new ProjectItem(this, fileName, importer);
            ProjectItems.Add(result);
            return result;
        }

        /// <summary>
        /// Adds a new reference by this project.
        /// </summary>
        public void AddReference(Project project)
        {
            Verify.IsNotNull(project, "project");

            if (HasCircularDependency(project, this))
                throw new InvalidOperationException("Target object has a circular dependency");

            if (!References.Contains(project))
                References.Add(project);
        }

        /// <summary>
        /// Removes an existing reference by this project
        /// </summary>
        public void RemoveReference(Project project)
        {
            Verify.IsNotNull(project, "project");

            References.Remove(project);
        }

        private static bool HasCircularDependency(Project subtree, Project root)
        {
            return !subtree.References.All(node => node != root && !HasCircularDependency(node, root));
        }

        /// <summary>
        /// Closes the project.
        /// </summary>
        public void Close()
        {
            // Can't close this when any project references this one
            if (Editor.Projects.Any(project => project.References.Contains(this)))
                throw new InvalidOperationException("Cannot close project because it is referenced by other projects");

            ProjectItems.ForEach(doc => doc.Close());
            Editor.projects.Remove(this);
            References.ForEach(project => project.Close());
            if (Editor.ActiveProject == this)
                Editor.ActiveProject = null;

            Trace.TraceInformation("Project {0} closed.", Name);
        }

        private void Load()
        {
            var project = (Nine.Studio.Serialization.Project)System.Xaml.XamlServices.Load(FileName);
            
            if (Version.Parse(project.Version) > Version.Parse(Editor.VersionString))
                throw new InvalidOperationException(Strings.VersionNotSupported);

            project.References.ForEach(x => Editor.OpenProject(x.Source));
            project.ProjectItems.ForEach(x => Import(x.Source, x.Importer));
        }
        
        /// <summary>
        /// Saves the project.
        /// </summary>
        public void Save()
        {
            if (IsModified)
                FileHelper.BackupAndSave(FileName, SaveProject);

            foreach (ProjectItem projectItem in ProjectItems)
            {
                if (projectItem.IsModified && projectItem.CanSave)
                    projectItem.Save();
            }

            IsModified = false;
            Editor.AddRecentProject(FileName);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void SaveProject(Stream stream)
        {
            var project = new Nine.Studio.Serialization.Project() { Version = Editor.VersionString };
            project.References.AddRange(References.Select(x => new Nine.Studio.Serialization.ProjectReference() { Source = x.FileName }));
            project.ProjectItems.AddRange(ProjectItems.Select(x => new Nine.Studio.Serialization.ProjectItem() { Source = x.RelativeFilename, Importer = x.Importer != null ? x.Importer.Value : null }));
            System.Xaml.XamlServices.Save(stream, project);
        }

        protected override void NotifyPropertyChanged(string propertyName = null)
        {
            isModified = true;
            base.NotifyPropertyChanged(propertyName);
        }
        
        void IDisposable.Dispose()
        {
            Close();
        }
        #endregion
    }
}