namespace Nine.Studio.Shell
{
    using Nine.Studio.Extensibility;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading;
    using System.Windows;

    [Export(typeof(ISettings))]
    [LocalizedCategory("General")]
    [LocalizedDisplayName("General")]
    public class GeneralSettings : ISettings
    {
        public string Language { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public double WindowWidth { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public double WindowHeight { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool WindowMaximized { get; set; }

        public GeneralSettings()
        {
            Language = "en-US";
            WindowWidth = 1280;
            WindowHeight = 720;
        }
    }
}
