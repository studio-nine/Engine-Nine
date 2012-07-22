using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Nine.Studio.Extensibility;


namespace Nine.Studio
{
using Exporter = Lazy<IExporter, IMetadata>;
using Importer = Lazy<IImporter, IMetadata>;
using Visualizer = Lazy<IVisualizer, IMetadata>;


    /// <summary>
    /// Represents a single project managed by a project instance.
    /// </summary>
    public class ProjectItem : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Gets the fileName of this document.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the absolute fileName of this document with full path.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the relative fileName as to it's parent project.
        /// </summary>
        public string RelativeFilename { get; private set; }

        /// <summary>
        /// Gets the metadata of this document.
        /// </summary>
        public IMetadata Metadata { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance can be saved.
        /// </summary>
        public bool CanSave { get { return Exporter != null; } }

        /// <summary>
        /// Gets whether this document is ready only.
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Gets whether this document is modified since last save.
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
        /// Gets the underlying object model.
        /// </summary>
        public object ObjectModel
        {
            get 
            {
                if (Interlocked.CompareExchange(ref objectModelNeedsReload, 0, 1) == 1)
                {
                    // TODO: Reload object model                    
                    NotifyPropertyChanged("ObjectModel");
                }
                return objectModel;
            }
            private set { objectModel = value; }
        }
        private object objectModel;
        
        /// <summary>
        /// Gets or sets the current selection.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Selection { get; set; }

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
        public ReadOnlyCollection<ProjectItem> References { get; private set; }

        /// <summary>
        /// Gets the default exporter of this project item.
        /// </summary>
        public Exporter Exporter { get; private set; }

        /// <summary>
        /// Gets the default importer of this project item.
        /// </summary>
        public Importer Importer { get; private set; }

        /// <summary>
        /// Gets the default visualizer of this project item.
        /// </summary>
        public Visualizer Visualizer { get; private set; }

        /// <summary>
        /// Gets a dictionary of the parameters used by the importer.
        /// </summary>
        public IDictionary<string, object> ImporterParameters { get; private set; }
        
        /// <summary>
        /// Gets or sets any user data that will be saved with the project file.
        /// </summary>
        public object Tag { get; set; }

        internal List<ProjectItem> InnerReferences;

        private int objectModelNeedsReload;
        private IDisposable sourceFileWatcher;

        /// <summary>
        /// Initializes a new document instance.
        /// </summary>
        internal ProjectItem(Project project, object objectModel, string fileName)
        {
            Verify.IsNotNull(project, "project");
            Verify.IsNotNull(objectModel, "objectModel");

            this.Project = project;
            this.InnerReferences = new List<ProjectItem>();
            this.References = new ReadOnlyCollection<ProjectItem>(InnerReferences);
            this.ObjectModel = objectModel;
            this.IsModified = string.IsNullOrEmpty(fileName);
            this.Exporter = Editor.Extensions.FindExporter(objectModel.GetType());
            this.Importer = Editor.Extensions.FindImporter(objectModel.GetType());
            this.Visualizer = Editor.Extensions.FindVisualizer(objectModel.GetType());

            this.Metadata = new Metadata
            {
                DisplayName = Editor.GetDisplayName(objectModel),
                Category = Editor.GetCategory(objectModel),
                FolderName = Editor.GetFolderName(ObjectModel),
            };

            var extension = Exporter != null ? Exporter.Value.FileExtensions.FirstOrDefault() : null;
            if (string.IsNullOrEmpty(fileName))
                fileName = Global.NextName(Path.Combine(Metadata.FolderName, Metadata.DisplayName), extension);

            if (Path.IsPathRooted(fileName))
                FileName = fileName;
            else
                FileName = Path.Combine(Project.Directory, fileName);

            Name = Path.GetFileNameWithoutExtension(FileName);
            RelativeFilename = Global.GetRelativeFilename(FileName, Project.Directory);
            Verify.IsFalse(Path.IsPathRooted(RelativeFilename), "RelativeFilename");

            FileInfo info = new FileInfo(FileName);
            IsReadOnly = info.Exists && info.IsReadOnly;

            BeginWatchFileChanges();
        }

        /// <summary>
        /// Adds a new reference by this projectItem
        /// </summary>
        public void AddReference(ProjectItem projectItem)
        {
            Verify.IsNotNull(projectItem, "projectItem");
            Verify.IsFalse(HasCircularDependency(projectItem, this), "Target object has a circular dependency");
            
            if (!InnerReferences.Contains(projectItem))
            {
                InnerReferences.Add(projectItem);
                Project.AddReference(projectItem.Project);
            }
        }

        /// <summary>
        /// Removes an existing reference by this document
        /// </summary>
        public void RemoveReference(ProjectItem projectItem)
        {
            Verify.IsNotNull(projectItem, "projectItem");

            InnerReferences.Remove(projectItem);
        }

        private static bool HasCircularDependency(ProjectItem subtree, ProjectItem root)
        {
            return !subtree.InnerReferences.All(node => node != root && !HasCircularDependency(node, root));
        }

        /// <summary>
        /// Closes this document
        /// </summary>
        public void Close()
        {
            Assert.CheckThread();

            // Can't close this when any document references this one
            if (Editor.Projects.SelectMany(proj => proj.ProjectItems).Any(doc => doc.References.Contains(this)))
                throw new InvalidOperationException("Cannot close this project item because it is referenced by other project items");

            Project.InnerProjectItems.Remove(this);
            References.ForEach(doc => doc.Close());
        }
        
        /// <summary>
        /// Saves this document
        /// </summary>
        public void Save()
        {
            Assert.CheckThread();
            Verify.IsNeitherNullNorEmpty(FileName, "FileName");

            if (IsModified && CanSave)
                Save(FileName);
        }

        /// <summary>
        /// Saves this document
        /// </summary>
        public void Save(string fileName)
        {
            Assert.CheckThread();
            Verify.IsNeitherNullNorEmpty(fileName, "fileName");

            Save(fileName, null);
        }

        /// <summary>
        /// Saves this document
        /// </summary>
        public void Save(string fileName, IExporter exporter)
        {
            Assert.CheckThread();
            Assert.IsNotNull(Exporter);
            Assert.IsNotNull(Exporter.Value);
            Verify.IsNeitherNullNorEmpty(fileName, "fileName");

            FileOperation.BackupAndSave(fileName, stream => Save(stream, exporter));
        }

        /// <summary>
        /// Saves this document
        /// </summary>
        public void Save(Stream stream, IExporter exporter)
        {
            Assert.CheckThread();
            Verify.IsNotNull(stream, "stream");

            if (exporter != null)
                exporter.Export(stream, ObjectModel);
            else if (Exporter.Value != null)
                Exporter.Value.Export(stream, ObjectModel);
            else
                throw new InvalidOperationException("No valid exporter found.");

            stream.Flush();
        }

        private void BeginWatchFileChanges()
        {
            if (sourceFileWatcher == null && File.Exists(FileName))
            {
                sourceFileWatcher = FileOperation.WatchFileContentChange(FileName, fileName =>
                {
                    Interlocked.Exchange(ref objectModelNeedsReload, 1);
                });
            }
        }

        private void EndWatchFileChanges()
        {
            if (sourceFileWatcher != null)
            {
                sourceFileWatcher.Dispose();
                sourceFileWatcher = null;
            }
        }

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
            EndWatchFileChanges();
        }
    }
}
