namespace Nine.Studio.Extensibility
{


    /// <summary>
    /// Represents an interface that is used to store settings of a given document type.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Gets the settings object can is presented in the property grid.
        /// </summary>
        object SettingsObject { get; }
    }
}
