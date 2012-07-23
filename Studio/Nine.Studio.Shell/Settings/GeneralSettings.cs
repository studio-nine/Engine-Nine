namespace Nine.Studio.Settings
{
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using Nine.Studio.Extensibility;

    [Export(typeof(ISettings))]
    [LocalizedDisplayName("General")]
    [LocalizedCategory("General")]
    public class GeneralSettings : ISettings
    {
        #region ISettings
        public static GeneralSettings Current { get; private set; }

        static GeneralSettings()
        {
            Current = new GeneralSettings();
        }

        object ISettings.SettingsObject
        {
            get { return Current; }
        }
        #endregion

        [EditorBrowsable]
        public bool ShowStartPage { get; set; }
    }
}
