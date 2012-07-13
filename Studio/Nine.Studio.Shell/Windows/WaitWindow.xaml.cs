using System;
using System.Windows;
using Nine.Studio.Shell.Windows;

namespace Nine.Studio.Shell.Windows
{
    public partial class WaitWindow : Window
    {
        public WaitWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            WindowHelper.RemoveIcon(this);
            WindowHelper.RemoveCloseButton(this);
        }
    }
}
