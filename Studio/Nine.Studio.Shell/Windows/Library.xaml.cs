using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Nine.Studio.Shell.ViewModels;

namespace Nine.Studio.Shell.Windows
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : UserControl
    {
        ICollectionView viewSource;

        public Library()
        {
            InitializeComponent();    
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (viewSource == null)
            {
                viewSource = (ICollectionView)ProjectItemsList.ItemsSource;
                viewSource.Filter = (o) =>
                {
                    string searchPattern = SearchTextBox.Text;
                    return ((ProjectItemView)o).Name.IndexOf(searchPattern, StringComparison.InvariantCultureIgnoreCase) >= 0;
                };
            }
            viewSource.Refresh();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            //((FrameworkElement)(e.Source)).DataContext = LibraryView.EditorView;
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = LibraryView != null;
        }

        private void ExecuteDelete(object sender, ExecutedRoutedEventArgs e)
        {
            /*
            var doc = ((FrameworkElement)e.OriginalSource).DataContext as ProjectItemView;
            if (doc != null && LibraryView != null)
            {
                LibraryView.ProjectView.DeleteProjectItem(doc);
            }
             */
        }
    }
}
