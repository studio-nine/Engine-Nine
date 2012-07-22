namespace Nine.Studio.Shell.ViewModels
{

    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using Nine.Studio.Extensibility;

    public class VisualizerView : INotifyPropertyChanged
    {
        public ICommand ShowCommand { get; private set; }
        public ProjectItem ProjectItem { get { return ProjectItemView.ProjectItem; } }
        public ProjectItemView ProjectItemView { get; private set; }
        public IVisualizer Visualizer { get; private set; }
        public Project Project { get { return ProjectView.Project; } }
        public ProjectView ProjectView { get { return ProjectItemView.ProjectView; } }
        public Editor Editor { get { return EditorView.Editor; } }
        public EditorView EditorView { get { return ProjectView.EditorView; } }
        public ObservableCollection<PropertyView> Properties { get; private set; }
        public bool IsDefault { get { return ProjectItemView.DefaultVisualizer == this; } }

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
        

        public VisualizerView(ProjectItemView projectItemView, IVisualizer visualizer)
        {
            this.ProjectItemView = projectItemView;
            this.Visualizer = visualizer;
            this.Properties = new ObservableCollection<PropertyView>();
            this.Properties.AddRange(PropertyHelper.GetBrowsableProperties(visualizer)
                                                             .Select(p => new PropertyView(visualizer, p)));
            this.ShowCommand = new DelegateCommand(Show);
        }

        public void Show()
        {
            ProjectItemView.ActiveVisualizer = this;
        }

        public object Visualize()
        {
            return Visualizer.Visualize(ProjectItem.ObjectModel);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
