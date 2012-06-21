using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Nine.Studio.Shell.ViewModels;

namespace Nine.Studio.Shell.Windows.Controls
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : UserControl
    {
        public LibraryView LibraryView { get; private set; }

        public Library()
        {
            InitializeComponent();
        }

        private void Library_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LibraryView = DataContext as LibraryView;
            if (LibraryView == null) 
                return;

            ICollectionView view = CollectionViewSource.GetDefaultView(LibraryView.ProjectView.ProjectItems);
            view.Filter = (o) =>
            {
                string searchPattern = SearchTextBox.Text;
                return ((ProjectItemView)o).FileName.IndexOf(searchPattern, StringComparison.InvariantCultureIgnoreCase) >= 0;
            };
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LibraryView != null)
                CollectionViewSource.GetDefaultView(LibraryView.ProjectView.ProjectItems).Refresh();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)(e.Source)).DataContext = LibraryView.EditorView;
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LibraryView != null;
        }

        private void ExecuteDelete(object sender, ExecutedRoutedEventArgs e)
        {
            var doc = ((FrameworkElement)e.OriginalSource).DataContext as ProjectItemView;
            if (doc != null && LibraryView != null)
            {
                LibraryView.ProjectView.DeleteProjectItem(doc);
            }
        }
    }
}
