namespace Nine.Studio
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
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
        /// Gets the title of the editor independent of the current culture settings.
        /// </summary>
        public string TitleInvarient
        {
            get { return Strings.ResourceManager.GetString("Title", CultureInfo.InvariantCulture); }
        }

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
        public IList<Project> Projects { get; private set; }
        internal IList<Project> projects;

        /// <summary>
        /// Gets a list of recent files.
        /// </summary>
        public IList<string> RecentFiles { get; private set; }
        private IList<string> recentFiles;

        /// <summary>
        /// Gets the extensions of this editor.
        /// </summary>
        public EditorExtensions Extensions { get; private set; }

        internal readonly string AppDataDirectory;
        internal readonly string SettingsDirectory;
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

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            AppDataDirectory = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), TitleInvarient, VersionString);

            if (!Directory.Exists(AppDataDirectory))
                Directory.CreateDirectory(AppDataDirectory);
            
            SettingsDirectory = Path.Combine(AppDataDirectory, "Settings");
            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            InitializeTrace();

            Trace.TraceInformation("Launching Editor {0}", DateTime.Now);
            Trace.TraceInformation("Working Directory {0}", Directory.GetCurrentDirectory());
            Trace.TraceInformation("Application Data Directory {0}", AppDataDirectory);

            Extensions = new EditorExtensions();
            Projects = new ReadOnlyObservableCollection<Project>(projects = new ObservableCollection<Project>());
            RecentFiles = new ReadOnlyObservableCollection<string>(recentFiles = new BindableCollection<string>(new ObservableCollection<string>()));

            LoadRecentFiles();
        }

        private void InitializeTrace()
        {
            try
            {
                var traceFile = Path.Combine(AppDataDirectory, "Editor.log");
                if (File.Exists(traceFile))
                    File.Delete(traceFile);

                Trace.AutoFlush = true;                
                Trace.Listeners.Add(new TextWriterTraceListener(File.OpenWrite(traceFile)));
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
            File.WriteAllLines(Path.Combine(AppDataDirectory, RecentFilesName), recentFiles.Take(MaxRecentFilesCount));
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

        private void RemoveRecentProject(string fileName)
        {
            for (var i = 0; i < recentFiles.Count; ++i)
            {
                if (FileHelper.FileNameEquals(recentFiles[i], fileName))
                {
                    recentFiles.RemoveAt(i);
                    --i;
                }
            }
            SaveRecentFiles();
        }

        /// <summary>
        /// Loads the editor extensions from the target path.
        /// </summary>
        public void LoadExtensions(string path = ".")
        {
            Extensions.Load(this, path);

            LoadSettings();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Returns the specified settings from editor extensions.
        /// </summary>
        public T FindSettings<T>() where T : Nine.Studio.Extensibility.ISettings
        {
            return Extensions.Settings.Select(x => x.Value).OfType<T>().FirstOrDefault();
        }

        private void LoadSettings()
        {
            Directory.GetFiles(SettingsDirectory).ForEach(fileName => LoadSettings(fileName));
        }

        private void LoadSettings(string fileName)
        {
            try
            {
                Trace.TraceInformation("Loading settings " + fileName);
                var result = System.Xaml.XamlServices.Load(fileName);
                Extensions.Settings.Where(x => x.Value.GetType() == result.GetType())
                          .ForEach(x => x.Value = (Nine.Studio.Extensibility.ISettings)result);
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Error loading settings file " + fileName);
                FileHelper.DeleteFile(fileName);
            }
        }

        private void SaveSettings()
        {
            Extensions.Settings.ForEach(x => System.Xaml.XamlServices.Save(
                Path.Combine(SettingsDirectory, x.Value.GetType().FullName), x.Value));
        }
        
        /// <summary>
        /// Creates an empty project in the editor.
        /// </summary>
        public Project CreateProject(string name, string directory)
        {
            var result = new Project(this, name, directory);
            projects.Add(result);
            ActiveProject = result;
            return result;
        }
        
        /// <summary>
        /// Creates a project from a existing file in the editor.
        /// </summary>
        public Project OpenProject(string fileName)
        {
            try
            {
                var result = new Project(this, fileName);
                projects.Add(result);
                ActiveProject = result;
                return result;
            }
            catch
            {
                RemoveRecentProject(fileName);
                throw;
            }
        }

        /// <summary>
        /// Closes the current project.
        /// </summary>
        public void Close()
        {
            var order = new int[Projects.Count];
            DependencyGraph.Sort(Projects, order, this);
            order.Select(i => Projects[i]).ToArray().ForEach(proj => proj.Close());

            SaveSettings();

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
