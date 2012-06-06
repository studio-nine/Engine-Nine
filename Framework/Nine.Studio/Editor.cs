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
using System.IO;
using System.Reflection;
using System.Diagnostics;
#endregion

namespace Nine.Studio
{
    /// <summary>
    /// Represents an instance of editor object.
    /// </summary>
    public class Editor : IDisposable
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
        /// Gets the project currently opened by the editor.
        /// </summary>
        public Project Project { get; internal set; }

        /// <summary>
        /// Gets the extensions of this editor.
        /// </summary>
        public EditorExtension Extensions { get; private set; }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        #region Initialize
        /// <summary>
        /// Creates a new instance of the editor.
        /// </summary>
        public static Editor Launch()
        {
            return new Editor();
        }

        static Editor()
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) => 
            {
                Trace.TraceError("Unhandled Exception Occured");
                Trace.WriteLine(e.ExceptionObject.ToString());
                Trace.Flush(); 
            };

            InitializeTrace();
        }

        private static void InitializeTrace()
        {
            try
            {
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Strings.Title);

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
            Extensions = new EditorExtension();
        }
        #endregion

        /// <summary>
        /// Creates a new project instance with an optional filename.
        /// </summary>
        public Project CreateProject()
        {
            return Project = new Project(this, null);
        }

        /// <summary>
        /// Opens a new project instance from file.
        /// </summary>
        public Project OpenProject(string filename)
        {
            return Project = new Project(this, filename);
        }

        public void Dispose()
        {
            Extensions.Dispose();
        }
    }
}
