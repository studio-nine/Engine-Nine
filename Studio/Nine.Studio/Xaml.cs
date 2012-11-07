namespace Nine.Studio.Xaml
{
    using Nine.Studio.Extensibility;
    using System.Collections.Generic;
    using System.Windows.Markup;

    [ContentProperty("ProjectItems")]
    public class Project
    {
        public string Version { get; set; }
        public ICollection<ProjectReference> References { get; private set; }
        public ICollection<ProjectItem> ProjectItems { get; private set; }

        public Project()
        {
            References = new List<ProjectReference>();
            ProjectItems = new List<ProjectItem>();
        }
    }

    [ContentProperty("Importer")]
    public class ProjectItem
    {
        public string Source {get;set;}
        public IImporter Importer { get; set; }
    }

    public class ProjectReference
    {
        public string Source { get; set; }
    }
}