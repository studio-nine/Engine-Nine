#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Windows;
using System.Windows.Controls;
using Nine.Studio.Shell.Windows.Controls;
#endregion

namespace Nine.Studio.Shell.Windows
{
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
