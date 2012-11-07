namespace Nine.Studio
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents an instance of editor object.
    /// </summary>
    public class Editor : ObservableObject, IDependencyProvider<Project>, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the title of the editor.
        /// </summary>
        public string Title { get { return Strings.Title; } }

        /// <summary>
        /// Gets the version of the editor.
        /// </summary>
        public Version Version
        {
            get { return version ?? (version = Assembly.GetExecutingAssembly().GetName().Version); }
        }
        private Version version;

        /// <summary>
        /// Gets the string representation of the editor version.
        /// </summary>
        public string VersionString
        {
            get { return versionString ?? (versionString = string.Concat(Version.Major, ".", Version.Minor)); }
        }
        private string versionString;

        /// <summary>
        /// Gets or sets the current active project.
        /// </summary>
        public Project ActiveProject
        {
            get { return activeProject; }
            set
            {
                if (!Projects.Contains(value))
                    throw new InvalidOperationException();
                activeProject = value;
                NotifyPropertyChanged();
            }
        }
        private Project activeProject;

        /// <summary>
        /// Gets all the projects currently opened by the editor.
        /// </summary>
        public ReadOnlyObservableCollection<Project> Projects { get; private set; }
        internal ObservableCollection<Project> projects;

        /// <summary>
        /// Gets a list of recent files.
        /// </summary>
        public ReadOnlyObservableCollection<string> RecentFiles { get; private set; }
        private ObservableCollection<string> recentFiles;

        /// <summary>
        /// Gets the extensions of this editor.
        /// </summary>
        public EditorExtensions Extensions { get; private set; }

        internal readonly string AppDataDirectory;        
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new instance of the editor.
        /// </summary>
        public Editor()
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                Trace.TraceError("Unhandled Exception Occurred:");
                Trace.WriteLine(e.ExceptionObject.ToString());
                if (e.ExceptionObject is Exception && ((Exception)e.ExceptionObject).InnerException != null)
                {
                    Trace.WriteLine("Inner Exception:");
                    Trace.WriteLine(((Exception)e.ExceptionObject).InnerException.ToString());
                }
                Trace.Flush();
            };

            AppDataDirectory = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), Strings.Title, VersionString);

            if (!Directory.Exists(AppDataDirectory))
                Directory.CreateDirectory(AppDataDirectory);

            InitializeTrace();

            Trace.TraceInformation("Launching Editor {0}", DateTime.Now);
            Trace.TraceInformation("Working Directory {0}", Directory.GetCurrentDirectory());
            Trace.TraceInformation("Application Data Directory {0}", AppDataDirectory);

            Extensions = new EditorExtensions();
            Projects = new ReadOnlyObservableCollection<Project>(projects = new ObservableCollection<Project>());
            RecentFiles = new ReadOnlyObservableCollection<string>(recentFiles = new ObservableCollection<string>());

            LoadRecentFiles();
        }

        private void InitializeTrace()
        {
            try
            {
                Trace.AutoFlush = true;
                Trace.Listeners.Add(new TextWriterTraceListener(new FileStream(Path.Combine(AppDataDirectory, "Editor.log"), FileMode.Create)));
            }
            catch { }
        }

        const string RecentFilesName = "RecentFiles";
        const int MaxRecentFilesCount = 10;

        private void LoadRecentFiles()
        {
            recentFiles.Clear();
            var filename = Path.Combine(AppDataDirectory, RecentFilesName);
            if (File.Exists(filename))
                recentFiles.AddRange(File.ReadAllLines(filename).Take(MaxRecentFilesCount));
        }

        private void SaveRecentFiles()
        {
            File.WriteAllLines(Path.Combine(AppDataDirectory, RecentFilesName), 
                recentFiles.Take(MaxRecentFilesCount));
        }

        internal void AddRecentProject(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName) &&
                !recentFiles.Any(file => FileHelper.FileNameEquals(file, fileName)))
            {
                recentFiles.Insert(0, fileName);
                if (recentFiles.Count > MaxRecentFilesCount)
                    recentFiles.RemoveAt(recentFiles.Count - 1);
                System.Windows.Shell.JumpList.AddToRecentCategory(fileName);
                SaveRecentFiles();
            }
        }

        /// <summary>
        /// Loads the editor extensions from the target path.
        /// </summary>
        public void LoadExtensions(string path = ".")
        {
            Extensions.Load(path);
        }
        
        /// <summary>
        /// Creates an empty project in the editor.
        /// </summary>
        public Project CreateProject(string name, string directory)
        {
            var result = new Project(this, name, directory);
            projects.Add(result);
            return result;
        }
        
        /// <summary>
        /// Creates a project from a existing file in the editor.
        /// </summary>
        public Project OpenProject(string fileName)
        {
            var result = new Project(this, fileName);
            projects.Add(result);
            return result;
        }

        /// <summary>
        /// Closes the current project.
        /// </summary>
        public void Close()
        {
            var order = new int[Projects.Count];
            DependencyGraph.Sort(Projects, order, this);
            order.Select(i => Projects[i]).ToArray().ForEach(proj => proj.Close());
            Trace.TraceInformation("Editor closed.");
        }
        
        int IDependencyProvider<Project>.GetDependencies(IList<Project> elements, int index, int[] dependencies)
        {
            int i = 0;
            foreach (var proj in Projects)
                if (proj.References.Contains(elements[index]))
                    dependencies[i++] = ((IList)elements).IndexOf(proj);
            return i;
        }

        /// <summary>
        /// Occurs when the progress changes during a loading operation.
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;

        internal void NotifyProgressChanged(string text, float percentage)
        {
            if (ProgressChanged != null)
                ProgressChanged(this, new ProgressChangedEventArgs((int)(100 * percentage), text));
        }
                
        void IDisposable.Dispose()
        {
            Close();
        }
        #endregion
    }
}
