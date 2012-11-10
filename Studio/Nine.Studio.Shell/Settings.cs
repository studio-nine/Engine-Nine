namespace Nine.Studio.Shell
{
    using Nine.Studio.Extensibility;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading;
    using System.Windows;

    [Export(typeof(ISettings))]
    public class Settings : ISettings
    {
        public string Language { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public double WindowWidth { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public double WindowHeight { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool WindowMaximized { get; set; }

        public Settings()
        {
            Language = "en-US";
            WindowWidth = 1280;
            WindowHeight = 720;
        }
    }
}
