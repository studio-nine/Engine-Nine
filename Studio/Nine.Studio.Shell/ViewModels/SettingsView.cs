namespace Nine.Studio.Shell.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Nine.Studio.Extensibility;

    public class SettingsView
    {
        public ISettings DocumentSettings { get; private set; }
        public Editor Editor { get { return EditorView.Editor; } }
        public EditorView EditorView { get; private set; }
        public ObservableCollection<PropertyView> Properties { get; private set; }
        public ObservableCollection<SettingsView> Children { get; private set; }

        //public string DisplayName { get { return Children.Count > 0 ? DocumentSettings.Category : DocumentSettings.DisplayName; } }
        //public string Category { get { return DocumentSettings.Category ?? Nine.Studio.Strings.Misc; } }

        public SettingsView(EditorView editorView, ISettings documentSettings)
            : this(editorView, documentSettings, Enumerable.Empty<SettingsView>())
        {

        }

        public SettingsView(EditorView editorView, ISettings documentSettings, IEnumerable<SettingsView> children)
        {
            this.EditorView = editorView;
            this.DocumentSettings = documentSettings;
            this.Properties = new ObservableCollection<PropertyView>();
            this.Children = new ObservableCollection<SettingsView>(children);
            this.Properties.AddRange(PropertyHelper.GetBrowsableProperties(documentSettings)
                                                   .Select(p => new PropertyView(documentSettings, p)));
        }
    }
}
