namespace Nine.Content
{
    using System.ComponentModel;

    /// <summary>
    /// Contains properties for content build.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ContentProperties
    {
        /// <summary>
        /// Determines whether we are currently building the content.
        /// </summary>
        public static bool IsContentBuild { get; internal set; }
    }
}