namespace Nine.Studio
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Nine.Studio.Extensibility;
    using System.Diagnostics;

    /// <summary>
    /// Represents a single project managed by a project instance.
    /// </summary>
    public class ProjectItem : ObservableObject, IDisposable
    {
        #region Properties
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
        /// Gets the underlying object model.
        /// </summary>
        public object ObjectModel
        {
            get 
            {
                if (Interlocked.CompareExchange(ref objectModelNeedsReload, 0, 1) == 1)
                {
                    NotifyPropertyChanged();
                }
                return objectModel;
            }
            private set { objectModel = value; }
        }
        private object objectModel;
        
        /// <summary>
        /// Gets or sets the current selection.
        /// </summary>
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
        public Extension<IExporter> Exporter { get; private set; }

        /// <summary>
        /// Gets the default importer of this project item.
        /// </summary>
        public Extension<IImporter> Importer { get; private set; }

        /// <summary>
        /// Gets the default visualizer of this project item.
        /// </summary>
        public Extension<IVisualizer> Visualizer { get; private set; }

        internal List<ProjectItem> InnerReferences;

        private List<string> dependentFiles = new List<string>();
        private int objectModelNeedsReload;
        private IDisposable sourceFileWatcher;
        #endregion

        #region Method
        private ProjectItem(Project project)
        {
            Verify.IsNotNull(project, "project");

            this.Project = project;
            this.InnerReferences = new List<ProjectItem>();
            this.References = new ReadOnlyCollection<ProjectItem>(InnerReferences);
        }

        /// <summary>
        /// Creates a new project item with the specified object model inside the target project.
        /// </summary>
        internal ProjectItem(Project project, object objectModel) : this(project)
        {
            Verify.IsNotNull(objectModel, "objectModel");

            this.ObjectModel = objectModel;
            this.Exporter = Editor.Extensions.FindExporter(objectModel.GetType());
            this.Importer = Editor.Extensions.FindImporter(objectModel.GetType());
            this.Visualizer = Editor.Extensions.FindVisualizer(objectModel.GetType());

            Verify.IsNotNull(Importer, "Importer");
            Verify.IsNotNull(Exporter, "Exporter");

            this.FileName = Path.GetFullPath(FileHelper.FindNextValidFileName(
                            Path.Combine(Project.Directory, Importer.Class, Importer.DisplayName),
                            Importer.Value.GetSupportedFileExtensions().First()));
            
            this.Name = Path.GetFileNameWithoutExtension(FileName);
            this.RelativeFilename = FileHelper.GetRelativeFileName(FileName, Project.Directory);

            var info = new FileInfo(FileName);
            this.IsReadOnly = info.Exists && info.IsReadOnly && CanSave;
        }

        /// <summary>
        /// Imports a project item from a source file to the specified project.
        /// </summary>
        internal ProjectItem(Project project, string fileName, IImporter importer) : this(project)
        {
            Verify.IsNotNull(importer, "importer");

            fileName = Path.IsPathRooted(fileName) ? fileName : FileHelper.NormalizeFileName(Path.Combine(Project.Directory, fileName)); 

            this.ObjectModel = importer.Import(fileName, dependentFiles);
            this.FileName = CopyToProjectDirectory(fileName, Editor.Extensions.GetClass(importer));
            
            CopyToProjectDirectory(dependentFiles, Path.GetDirectoryName(Path.GetFullPath(fileName)), Path.GetDirectoryName(FileName));

            this.Importer = new Extension<IImporter>(Editor.Extensions, importer);
            this.Exporter = Editor.Extensions.FindExporter(objectModel.GetType());
            this.Visualizer = Editor.Extensions.FindVisualizer(objectModel.GetType());

            Verify.IsNotNull(Importer, "Importer");
            
            this.Name = Path.GetFileNameWithoutExtension(FileName);
            this.RelativeFilename = FileHelper.GetRelativeFileName(FileName, Project.Directory);

            var info = new FileInfo(FileName);
            this.IsReadOnly = info.Exists && info.IsReadOnly && CanSave;
        }

        private string CopyToProjectDirectory(string fileName, string folder)
        {
            if (fileName.StartsWith(Project.Directory, StringComparison.OrdinalIgnoreCase))
                return fileName;

            var destFile = Path.GetFullPath(FileHelper.FindNextValidFileName(
                           Path.Combine(Project.Directory, folder,
                           Path.GetFileNameWithoutExtension(fileName)),
                           Path.GetExtension(fileName), false));

            var destPath = Path.GetDirectoryName(destFile);
            if (!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);

            File.Copy(fileName, destFile);
            return destFile;
        }

        private void CopyToProjectDirectory(IEnumerable<string> fileNames, string sourceDirectory, string targetDirectory)
        {
            if (sourceDirectory.StartsWith(targetDirectory, StringComparison.OrdinalIgnoreCase))
                return;

            foreach (var fileName in fileNames)
            {
                var relativePath = Path.IsPathRooted(fileName) ? FileHelper.GetRelativeFileName(fileName, sourceDirectory) : fileName;
                var absolutePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(sourceDirectory, fileName);

                if (Path.IsPathRooted(relativePath))
                {
                    Trace.TraceWarning("Absolute references found {0}", relativePath);
                    continue;
                }

                if (!File.Exists(absolutePath))
                {
                    Trace.TraceWarning("Cannot find reference to {0}", absolutePath);
                    continue;
                }

                var destFile = Path.Combine(targetDirectory, relativePath);
                if (File.Exists(destFile))
                {
                    Trace.TraceWarning("A file named {0} already exist at {0}", relativePath, targetDirectory);
                    continue;
                }

                var destPath = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);

                File.Copy(fileName, destFile);
            }
        }

        /// <summary>
        /// Adds a new reference by this projectItem
        /// </summary>
        public void AddReference(ProjectItem projectItem)
        {
            Verify.IsNotNull(projectItem, "projectItem");

            if (HasCircularDependency(projectItem, this))
                throw new InvalidOperationException("Target object has a circular dependency");
            
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
            // Can't close this when any document references this one
            if (Editor.Projects.SelectMany(proj => proj.ProjectItems).Any(doc => doc.References.Contains(this)))
                throw new InvalidOperationException("Cannot close this project item because it is referenced by other project items");

            Project.ProjectItems.Remove(this);
            References.ForEach(doc => doc.Close());

            var disposable = objectModel as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
        
        internal void Save()
        {
            Verify.IsNeitherNullNorEmpty(FileName, "FileName");

            if (IsModified && CanSave)
                Export(FileName, Exporter.Value);
        }

        /// <summary>
        /// Exports this project item.
        /// </summary>
        public void Export(string fileName, IExporter exporter)
        {
            Verify.IsNeitherNullNorEmpty(fileName, "fileName");
            Verify.IsNotNull(exporter, "exporter");

            FileHelper.BackupAndSave(fileName, stream =>
            {
                exporter.Export(stream, ObjectModel);
                stream.Flush();
            });
        }

        protected override void NotifyPropertyChanged(string propertyName = null)
        {
            isModified = true;
            base.NotifyPropertyChanged(propertyName);
        }

        private void BeginWatchFileChanges()
        {
            if (sourceFileWatcher == null && File.Exists(FileName))
            {
                sourceFileWatcher = FileHelper.WatchFileChanged(FileName, fileName =>
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

        void IDisposable.Dispose()
        {
            Close();
        }
        #endregion
    }
}
