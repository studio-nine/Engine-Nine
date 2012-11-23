namespace Nine.Studio.Shell
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

        private void OptionsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Tree.Items.Count > 0)
            {
                var item = Tree.ItemContainerGenerator.ContainerFromItem(Tree.Items[0]) as TreeViewItem;
                if (item != null)
                    item.IsSelected = true;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            WindowHelper.RemoveIcon(this);
        }
    }
}
