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
        public WINDOWPLACEMENT WindowPlacement { get; set; }

        public GeneralSettings()
        {
            Language = "en-US";
        }
    }
}
