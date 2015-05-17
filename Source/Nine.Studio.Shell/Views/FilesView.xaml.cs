namespace Nine.Studio.Shell
{
    using Microsoft.Win32;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    public partial class FilesView : DockPanel
    {
        public FilesView()
        {
            InitializeComponent();
        }

        private async void CreateProject(object sender, RoutedEventArgs e)
        {
            if (await App.EnsureSavedAsync())
            {
                var newProjectView = new NewProjectView();
                if (Strings.Create == await App.Shell.ShowDialogAsync(Strings.New, null, newProjectView, Strings.Create))
                {
                    App.Shell.EndDialog(this, null);
                    await App.Shell.RunAsync(() => { App.Editor.CreateProject(newProjectView.ProjectName, newProjectView.ProjectDirectory); });
                    // TODO: We should also save the project
                }
            }
        }

        private async void OpenProject(object sender, RoutedEventArgs e)
        {
            if (await App.EnsureSavedAsync())
            {
                var dialog = new OpenFileDialog();                
                dialog.Title = " ";
                dialog.CheckFileExists = true;
                dialog.DereferenceLinks = true;
                dialog.DefaultExt = Project.SourceFileExtension;
                dialog.Filter = string.Format(@"{0}|*.nine|{1}|*.*", Strings.NineProject, Strings.AllFiles);

                bool? result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    App.Shell.EndDialog(this, null);
                    await App.Shell.RunAsync(() => { App.Editor.OpenProject(dialog.FileName); });
                }
            }
        }

        private async void OpenRecentProject(object sender, RoutedEventArgs e)
        {
            if (await App.EnsureSavedAsync())
            {
                var fileName = ((FrameworkElement)sender).DataContext.ToString();
                App.Shell.EndDialog(this, null);
                await App.Shell.RunAsync(() => { App.Editor.OpenProject(fileName); });
            }
        }
        
        private void Options(object sender, RoutedEventArgs e)
        {

        }
    }
}
