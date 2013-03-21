namespace Nine.Studio
{
    using Nine.Studio.Extensibility;

    [Export(typeof(ISettings))]
    public class EditorSettings : ISettings
    {
        public string IntermediateDirectory { get; set; }
    }
}
