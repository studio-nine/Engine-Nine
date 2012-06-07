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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
#endregion

namespace Nine.Studio
{
    /// <summary>
    /// Represents an instance of editor object.
    /// </summary>
    public class Editor : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Gets the title of the editor.
        /// </summary>
        public string Title { get { return Strings.Title; } }

        /// <summary>
        /// Gets the version of the editor
        /// </summary>
        public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        /// <summary>
        /// Gets all the projects currently opened by the editor.
        /// </summary>
        public ReadOnlyObservableCollection<Project> Projects { get; private set; }

        /// <summary>
        /// Gets a list of recent files.
        /// </summary>
        public ReadOnlyObservableCollection<string> RecentProjects { get; private set; }

        /// <summary>
        /// Gets the extensions of this editor.
        /// </summary>
        public EditorExtensions Extensions { get; private set; }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        private ObservableCollection<string> recentFiles;
        private ObservableCollection<Project> projects;

        #region Initialize
        /// <summary>
        /// Creates a new instance of the editor.
        /// </summary>
        public static Editor Launch()
        {
            Assert.CheckThread();
            Trace.TraceInformation("Lauching Editor {0}", DateTime.Now);
            return new Editor();
        }

        static Editor()
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) => 
            {
                Trace.TraceError("Unhandled Exception Occured");
                Trace.WriteLine(e.ExceptionObject.ToString());
                if (e.ExceptionObject is Exception && ((Exception)e.ExceptionObject).InnerException != null)
                {
                    Trace.WriteLine("Inner Exception:");
                    Trace.WriteLine(((Exception)e.ExceptionObject).InnerException.ToString());
                }
                Trace.Flush(); 
            };

            InitializeTrace();
        }

        private static void InitializeTrace()
        {
            try
            {
                string appDataPath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData), Strings.Title, Global.VersionString);

                if (!Directory.Exists(appDataPath))
                    Directory.CreateDirectory(appDataPath);

                Stream traceOutput = new FileStream(Path.Combine(appDataPath, Global.TraceFilename), FileMode.Create);

                Trace.AutoFlush = true;
                Trace.Listeners.Add(new TextWriterTraceListener(traceOutput));
            }
            catch { }
        }

        private Editor()
        {
            Extensions = new EditorExtensions();
            projects = new ObservableCollection<Project>();
            Projects = new ReadOnlyObservableCollection<Project>(projects);
            recentFiles = new ObservableCollection<string>();
            RecentProjects = new ReadOnlyObservableCollection<string>(recentFiles);

            LoadRecentFiles();
        }
        #endregion

        #region Recent Files
        private void LoadRecentFiles()
        {
            object value = null;
            string keyName = Global.GetUserRegistry("Recent");
            
            recentFiles.Clear();
            for (int i = 0; i < Constants.MaxRecentFilesCount; i++)
            {
                if ((value = Microsoft.Win32.Registry.GetValue(keyName, i.ToString(), null)) == null)
                    break;

                if (File.Exists(value.ToString()) && !RecentProjects.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase))
                    recentFiles.Add(value.ToString());
            }
        }

        private void SaveRecentFiles()
        {
            string keyName = Global.GetUserRegistry("Recent");
            for (int i = 0; i < Constants.MaxRecentFilesCount; i++)
            {
                if (i < RecentProjects.Count)
                    Microsoft.Win32.Registry.SetValue(keyName, i.ToString(), RecentProjects[i]);
                else
                    Microsoft.Win32.Registry.SetValue(keyName, i.ToString(), "");
            }
        }

        internal void RecentProject(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                recentFiles.Remove(filename);
                recentFiles.Insert(0, filename);
                while (RecentProjects.Count > Constants.MaxRecentFilesCount)
                    recentFiles.RemoveAt(RecentProjects.Count - 1);

                //JumpList.AddToRecentCategory(filename);
            }

            SaveRecentFiles();
        }
        #endregion

        /// <summary>
        /// Closes the current project.
        /// </summary>
        public void Close()
        {
            Assert.CheckThread();
            Projects.ForEach(proj => proj.Close());
        }

        /// <summary>
        /// Creates a new project instance with a filename.
        /// </summary>
        public Project CreateProject(string name)
        {
            Assert.CheckThread();
            Verify.IsValidPath(name, "name");

            var project = new Project(this, name);
            projects.Add(project);
            RecentProject(project.Filename);
            Trace.TraceInformation("Project {0} created at {1}", project.Name, project.Filename);
            return project;
        }

        /// <summary>
        /// Opens a new project instance from file.
        /// </summary>
        public Project OpenProject(string filename)
        {
            Assert.CheckThread();
            Verify.FileExists(filename, "filename");

            var project = projects.FirstOrDefault(p => Global.FilenameEquals(p.Filename, filename));
            if (project != null)
                return project;

            project = new Project(this, filename);
            projects.Add(project);
            RecentProject(filename);
            Trace.TraceInformation("Project {0} opened at {1}", project.Name, project.Filename);
            return project;
        }

        internal void CloseProject(Project project)
        {
            Assert.CheckThread();
            Verify.IsNotNull(project, "project");
            Verify.IsTrue(project.Editor == this, "");

            projects.Remove(project);
            Trace.TraceInformation("Project {0} closed at {1}", project.Name, project.Filename);
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
            Extensions.Dispose();
            Trace.TraceInformation("Editor disposed");
        }
    }
}
