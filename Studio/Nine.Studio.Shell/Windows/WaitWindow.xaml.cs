namespace Nine.Studio.Shell.Windows
{
    using System;
    using System.Windows;


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
