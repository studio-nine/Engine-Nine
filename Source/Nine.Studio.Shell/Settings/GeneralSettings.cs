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

        public bool WindowMaximized { get; set; }

        // TODO: WindowWidth & WindowHeight

        public int WindowWidth
        {
            get
            {
                if (WindowPlacement.HasValue)
                {
                    return WindowPlacement.Value.rcNormalPosition.Width; 
                }
                return 0;
            }
        }

        public int WindowHeight
        {
            get 
            { 
                if (WindowPlacement.HasValue)
                {
                    return WindowPlacement.Value.rcNormalPosition.Height; 
                }
                return 0;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal WINDOWPLACEMENT? WindowPlacement { get; set; }

        public GeneralSettings()
        {
            Language = "en-US";
        }
    }
}
