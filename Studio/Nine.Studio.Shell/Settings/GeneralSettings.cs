namespace Nine.Studio.Settings
{
    using System.ComponentModel;
    using Nine.Studio.Extensibility;

    [Export(typeof(ISettings))]
    [LocalizedDisplayName("General")]
    [LocalizedCategory("General")]
    public class GeneralSettings : ISettings
    {
        [EditorBrowsable]
        public bool ShowStartPage { get; set; }
    }
}
