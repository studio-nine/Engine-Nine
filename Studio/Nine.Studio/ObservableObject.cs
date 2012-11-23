namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Allows you to obtain the method or property name of the caller.
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerMemberNameAttribute : Attribute { }
}

namespace Nine.Studio
{
    using System;
    using System.Xaml;
    using System.Windows.Markup;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Collections.Generic;

    /// <summary>
    /// Base class that implements INotifyPropertyChanged and IAttachedPropertyStore
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged, IAttachedPropertyStore
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// http://www.trelford.com/blog/post/CallMeMaybe.aspx
        /// </summary>
        protected virtual void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

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
                attachedProperties = new AttachableMemberIdentifierCollection();

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
        private AttachableMemberIdentifierCollection attachedProperties;

        /// <summary>
        /// Reusing this same event args.
        /// </summary>
        private static AttachedPropertyChangedEventArgs AttachedPropertyChangedEventArgs = new AttachedPropertyChangedEventArgs(null, null, null);

        /// <summary>
        /// Occurs when any of the attached property changed.
        /// </summary>
        internal event EventHandler<AttachedPropertyChangedEventArgs> AttachedPropertyChanged;
        #endregion
    }
}