#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Nine.Studio.Extensibility;
using Nine.Studio.Shell.Windows;
#endregion

namespace Nine.Studio.Shell.ViewModels
{
    public class ProjectItemView : INotifyPropertyChanged
    {
        public ICommand ShowCommand { get; private set; }
        public string Name { get { return Path.GetFileName(Filename); } }
        public string Filename { get; private set; }
        public string Title { get { return Name; } }

        public ProjectItem Document { get; private set; }
        public Project Project { get { return ProjectView.Project; } }
        public ProjectView ProjectView { get; private set; }
        public Editor Editor { get { return EditorView.Editor; } }
        public EditorView EditorView { get { return ProjectView.EditorView; } }
        public VisualizerView DefaultVisualizer { get; private set; }
        public ObservableCollection<VisualizerView> Visualizers { get; private set; }

        public VisualizerView ActiveVisualizer
        {
            get { return _ActiveVisualizer; }
            set
            {
                if (value != _ActiveVisualizer)
                {
                    if (_ActiveVisualizer != null)
                        _ActiveVisualizer.IsActive = false;
                    _ActiveVisualizer = value;
                    if (_ActiveVisualizer != null)
                        _ActiveVisualizer.IsActive = true;
                    NotifyPropertyChanged("ActiveVisualizer");
                }
            }
        }
        private VisualizerView _ActiveVisualizer;

        public DocumentWindow DocumentWindow
        {
            get { return documentWindow; }
        }
        private DocumentWindow documentWindow;

        public ProjectItemView(ProjectView projectView, ProjectItem document)
        {
            /*
            this.Document = document;
            this.ProjectView = projectView;
            this.Filename = document.Filename ?? Path.GetFullPath(Global.NextName(DocumentType.DisplayName, null));
            this.ShowCommand = new DelegateCommand(Show);
            this.DocumentVisualizers = new ObservableCollection<VisualizerView>();
            this.DocumentVisualizers.AddRange(Editor.Extensions.FindVisualizers(document.DocumentObject.GetType())
                                                               .Select(v => new VisualizerView(this, v)));
            this.DefaultVisualizer = DocumentVisualizers.FirstOrDefault(v => v.DocumentVisualizer == 
                                       Editor.Extensions.FindVisualizer(document.DocumentObject.GetType())) ??
                                     DocumentVisualizers.FirstOrDefault();
            this.ActiveVisualizer = DefaultVisualizer;
             */
        }

        public void Show()
        {
            if (documentWindow == null)
                documentWindow = new DocumentWindow() { DataContext = this };
            EditorView.ActiveProjectItem = this;
        }

        public void Hide()
        {
            EditorView.ActiveProjectItem = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
