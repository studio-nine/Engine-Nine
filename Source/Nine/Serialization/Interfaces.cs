namespace Nine.Serialization
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Specifies that an object should be replaced with another object during
    /// the save phase of the serialization.
    /// </summary>
    /// <remarks>
    /// <see cref="ISerializationOverride"/> interface can be used to save an object
    /// to a different object. This is useful to save objects that are loaded from
    /// XAML markup extension. 
    /// 
    /// For example, a sprite may contain a texture property that cannot 
    /// be saved directly, when saving that texture property, the BinarySerializer
    /// will first try to get a serialization override using the 
    /// <see cref="TryGetOverride"/>
    /// method, it will get a ContentReference object that contains
    /// the orginal filename where the texture was loaded, in this case, the
    /// ContentReference is saved instead.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISerializationOverride
    {
        /// <summary>
        /// Removes the override object of the target value.
        /// </summary>
        bool RemoveOverride(object target);

        /// <summary>
        /// Sets the override object of the target value, override the existing value if one already exists.
        /// </summary>
        void SetOverride(object target, object targetOverride);

        /// <summary>
        /// Tries to get the override object of the target value.
        /// </summary>
        bool TryGetOverride(object target, out object targetOverride);
    }

    /// <summary>
    /// Specifies that the target object should be shared with other objects
    /// during the save phase of the serialization.
    /// </summary>
    /// <remarks>
    /// An object can be saved to multiple files, in most cases, these files are
    /// specific to that object and the content is unique for that object.
    /// This interface allows the an object to save a global file and that file
    /// can be shared among other objects.
    /// 
    /// For example, different materials might be using the same shader code but
    /// with different paramter values. When saving these materials, it is ideal
    /// to share the shader code with all the materials.
    /// <see cref="Share"/> method will share the shader code
    /// and returns an unique filename to access that shared resource.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISerializationSharing
    {
        /// <summary>
        /// Specified that the target object should be shared using the share key.
        /// </summary>
        /// <param name="key">
        /// When this parameter is empty or null, the key is computed from the serialized
        /// bytes of the target object using MD5 hashing algorithm.
        /// </param>
        /// <returns>
        /// The filename to access the shared content.
        /// </returns>
        string Share(object target, string key);
    }

    // TODO: ISupportInitialize
    public interface ISupportInitialize
    {
        void BeginInit();
        void EndInit();
    }
}