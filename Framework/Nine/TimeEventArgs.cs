namespace Nine
{
    using System;
    using System.ComponentModel;


    /// <summary>
    /// Event arguments that contains the elapsed time.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TimeEventArgs : EventArgs
    {
        internal TimeEventArgs() { }

        /// <summary>
        /// Gets the elapsed time since last update or draw call.
        /// </summary>
        public TimeSpan ElapsedTime { get; internal set; }
    }
}
