namespace Nine
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using System.Xaml;
    using Microsoft.Xna.Framework.Content;
    using Nine.Content;

    /// <summary>
    /// Defines a basic named object that can be extended using attached properties.
    /// </summary>
    [RuntimeNameProperty("Name")]
    [DictionaryKeyProperty("Name")]
    public class Object : IAttachedPropertyStore
    {
        #region Properties
        /// <summary>
        /// Gets or sets the name of this transformable.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region IAttachedPropertyStore
        void IAttachedPropertyStore.CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
        {
            if (attachedProperties != null)
                ((ICollection<KeyValuePair<AttachableMemberIdentifier, object>>)attachedProperties).CopyTo(array, index);
        }

        int IAttachedPropertyStore.PropertyCount
        {
            get { return attachedProperties != null ? attachedProperties.Count : 0; }
        }

        bool IAttachedPropertyStore.RemoveProperty(AttachableMemberIdentifier attachableMemberIdentifier)
        {
            if (attachedProperties == null)
                return false;

            object oldValue;
            attachedProperties.TryGetValue(attachableMemberIdentifier, out oldValue);
            AttachedPropertyChangedEventArgs.OldValue = oldValue;
            if (attachedProperties.Remove(attachableMemberIdentifier))
            {
                if (AttachedPropertyChanged != null)
                {
                    AttachedPropertyChangedEventArgs.Property = attachableMemberIdentifier;
                    AttachedPropertyChangedEventArgs.NewValue = null;
                    AttachedPropertyChanged(this, AttachedPropertyChangedEventArgs);
                }
                return true;
            }
            return false;
        }

        void IAttachedPropertyStore.SetProperty(AttachableMemberIdentifier attachableMemberIdentifier, object value)
        {
            if (attachedProperties == null)
                attachedProperties = new Dictionary<AttachableMemberIdentifier, object>();

            object oldValue;
            attachedProperties.TryGetValue(attachableMemberIdentifier, out oldValue);
            AttachedPropertyChangedEventArgs.OldValue = oldValue;
            attachedProperties[attachableMemberIdentifier] = value;

            if (AttachedPropertyChanged != null)
            {
                AttachedPropertyChangedEventArgs.Property = attachableMemberIdentifier;
                AttachedPropertyChangedEventArgs.NewValue = value;
                AttachedPropertyChanged(this, AttachedPropertyChangedEventArgs);
            }
        }

        bool IAttachedPropertyStore.TryGetProperty(AttachableMemberIdentifier attachableMemberIdentifier, out object value)
        {
            value = null;
            return attachedProperties != null && attachedProperties.TryGetValue(attachableMemberIdentifier, out value);
        }

        [ContentSerializer]
        internal Dictionary<AttachableMemberIdentifier, object> AttachedProperties
        {
            get { return attachedProperties; }
            set
            {
                if (value != null)
                    foreach (var pair in value)
                        pair.Key.Apply(this, pair.Value);
            }
        }
        private Dictionary<AttachableMemberIdentifier, object> attachedProperties;

        /// <summary>
        /// Reusing this same event args.
        /// </summary>
        private static AttachedPropertyChangedEventArgs AttachedPropertyChangedEventArgs = new AttachedPropertyChangedEventArgs(null, null, null);

        /// <summary>
        /// Occurs when any of the attached property changed.
        /// </summary>
        public event EventHandler<AttachedPropertyChangedEventArgs> AttachedPropertyChanged;
        #endregion

        #region ToString
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return Name != null ? string.Concat(Name, ":", GetType().Name) : GetType().Name;
        }
        #endregion
    }
}