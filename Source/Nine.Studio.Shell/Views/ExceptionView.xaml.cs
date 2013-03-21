namespace Nine.Studio.Shell
{
    using System;
    using System.IO;
    using System.Windows.Controls;

    public partial class ExceptionView : UserControl
    {
        public string ProjectName { get; set; }
        public string ProjectDirectory { get; set; }

        public ExceptionView()
        {
            ProjectDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), App.Editor.TitleInvarient);
            ProjectName = "MyGame";

            InitializeComponent();
        }
    }
}
