namespace Nine.Serialization
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Specifies that an object should be replaced with another object during
    /// the save phase of the serialization.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISerializationOverride
    {
        /// <summary>
        /// Removes the override object of the target value.
        /// </summary>
        bool RemoveOverride(object target);

        /// <summary>
        /// Sets the override object of the target value.
        /// </summary>
        void SetOverride(object target, object targetOverride);

        /// <summary>
        /// Tries to get the override object of the target value.
        /// </summary>
        bool TryGetOverride(object target, out object targetOverride);
    }
}