namespace Nine.Studio.Shell.ViewModels
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;

    public class ProjectItemView : INotifyPropertyChanged
    {
        public ICommand ShowCommand { get; private set; }
        public string Name { get { return ProjectItem.Name; } }
        public string FileName { get { return ProjectItem.FileName; } }
        public string Title { get { return Name; } }

        public ProjectItem ProjectItem { get; private set; }
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

        public ProjectItemView(ProjectView projectView, ProjectItem projectItem)
        {
            this.ProjectItem = projectItem;
            this.ProjectView = projectView;
            this.ShowCommand = new DelegateCommand(Show);
            
            this.Visualizers = new ObservableCollection<VisualizerView>(
                Editor.Extensions.Visualizers.Where(v => v.Value.TargetType.IsAssignableFrom(projectItem.ObjectModel.GetType()))
                                             .Select(v => new VisualizerView(this, v.Value)));
            
            this.DefaultVisualizer = Visualizers.SingleOrDefault(v => v.IsDefault) ?? Visualizers.FirstOrDefault();
            this.ActiveVisualizer = DefaultVisualizer;
        }

        public void Show()
        {
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
