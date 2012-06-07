#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Nine.Studio.Extensibility;

#endregion

namespace Nine.Studio.Shell.ViewModels
{
    public class VisualizerView : INotifyPropertyChanged
    {
        public ICommand ShowCommand { get; private set; }
        public ProjectItem Document { get { return DocumentView.Document; } }
        public ProjectItemView DocumentView { get; private set; }
        public IVisualizer DocumentVisualizer { get; private set; }
        public Project Project { get { return ProjectView.Project; } }
        public ProjectView ProjectView { get { return DocumentView.ProjectView; } }
        public Editor Editor { get { return EditorView.Editor; } }
        public EditorView EditorView { get { return ProjectView.EditorView; } }
        public ObservableCollection<PropertyView> Properties { get; private set; }
        public bool IsDefault { get { return DocumentView.DefaultVisualizer == this; } }

        /*
        public string DisplayName 
        {
            get
            {
                return IsDefault ? string.Format("{0} ({1})", DocumentVisualizer.DisplayName, Strings.Default) :
                                   DocumentVisualizer.DisplayName;
            }
        }
        */

        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (value != _IsActive)
                {
                    _IsActive = value;
                    NotifyPropertyChanged("IsActive");
                }
            }
        }
        private bool _IsActive;
        

        public VisualizerView(ProjectItemView documentView, IVisualizer documentVisualizer)
        {
            this.DocumentView = documentView;
            this.DocumentVisualizer = documentVisualizer;
            this.Properties = new ObservableCollection<PropertyView>();
            this.Properties.AddRange(PropertyHelper.GetBrowsableProperties(documentVisualizer)
                                                             .Select(p => new PropertyView(documentVisualizer, p)));
            this.ShowCommand = new DelegateCommand(Show);
        }

        public void Show()
        {
            DocumentView.ActiveVisualizer = this;
        }

        public object Visualize()
        {
            // FIXME: Set new visualizer property
            return ((IVisualizer)Activator.CreateInstance(DocumentVisualizer.GetType())).Visualize(Document);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
