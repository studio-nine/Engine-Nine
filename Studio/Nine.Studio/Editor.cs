namespace Nine.Studio
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents an instance of editor object.
    /// </summary>
    public class Editor : ObservableObject, IDisposable
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
        /// Gets or sets the current active project.
        /// </summary>
        public Project ActiveProject
        {
            get { return activeProject; }
            set
            {
                if (!projects.Contains(value))
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
        private ObservableCollection<Project> projects;

        /// <summary>
        /// Gets a list of recent files.
        /// </summary>
        public ReadOnlyObservableCollection<string> RecentProjects { get; private set; }
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
                Environment.SpecialFolder.ApplicationData), Strings.Title,
                string.Format("v{0}.{1}", Version.Major, Version.Minor));

            if (!Directory.Exists(AppDataDirectory))
                Directory.CreateDirectory(AppDataDirectory);

            InitializeTrace();

            Trace.TraceInformation("Launching Editor {0}", DateTime.Now);
            Trace.TraceInformation("Working Directory {0}", Directory.GetCurrentDirectory());
            Trace.TraceInformation("Application Data Directory {0}", AppDataDirectory);

            Extensions = new EditorExtensions();
            projects = new ObservableCollection<Project>();
            Projects = new ReadOnlyObservableCollection<Project>(projects);
            recentFiles = new ObservableCollection<string>();
            RecentProjects = new ReadOnlyObservableCollection<string>(recentFiles);

            LoadRecentFiles();
        }

        private void InitializeTrace()
        {
            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(
                new FileStream(Path.Combine(AppDataDirectory,
                string.Concat("Nine.", DateTime.Now.Ticks, ".log")), FileMode.Create)));
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
        /// Closes the current project.
        /// </summary>
        public void Close()
        {
            projects.ForEach(proj => proj.Close());
        }

        /// <summary>
        /// Creates a new project instance with a fileName.
        /// </summary>
        public Project CreateProject(string name)
        {
            Verify.IsValidPath(name, "name");

            var project = new Project(this, name);
            projects.Add(project);
            AddRecentProject(project.FileName);
            Trace.TraceInformation("Project {0} created at {1}", project.Name, project.FileName);
            return project;
        }

        /// <summary>
        /// Opens a new project instance from file.
        /// </summary>
        public Project OpenProject(string fileName)
        {
            Verify.FileExists(fileName, "fileName");

            var project = projects.FirstOrDefault(p => FileHelper.FileNameEquals(p.FileName, fileName));
            if (project != null)
                return project;

            project = new Project(this, fileName);
            projects.Add(project);
            AddRecentProject(fileName);
            Trace.TraceInformation("Project {0} opened at {1}", project.Name, project.FileName);
            return project;
        }

        internal void CloseProject(Project project)
        {   
            Verify.IsNotNull(project, "project");

            projects.Remove(project);
            Trace.TraceInformation("Project {0} closed at {1}", project.Name, project.FileName);
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();
            Extensions.Dispose();
            Trace.TraceInformation("Editor disposed");
        }
        #endregion
    }
}
