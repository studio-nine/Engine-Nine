namespace Nine
{
    using System;
    using System.ComponentModel;
    using System.Xaml;

    /// <summary>
    /// Provides data for various property changed events.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AttachedPropertyChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the identifier for the attached property where the value change occurred.
        /// </summary>
        public AttachableMemberIdentifier Property { get; internal set; }

        /// <summary>
        /// Gets the value of the property after the change.
        /// </summary>
        public object NewValue { get; internal set; }

        /// <summary>
        /// Gets the value of the property before the change.
        /// </summary>
        public object OldValue { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachedPropertyChangedEventArgs"/> class.
        /// </summary>
        public AttachedPropertyChangedEventArgs(AttachableMemberIdentifier property, object newValue, object oldValue)
        {
            Property = property;
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}