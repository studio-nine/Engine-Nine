namespace System.Xaml
{
    using System;
    using System.Collections.Generic;

    public class AttachableMemberIdentifier : IEquatable<AttachableMemberIdentifier>
    {
        private Type declaringType;
        private string memberName;

        public Type DeclaringType { get { return declaringType; } }
        public string MemberName { get { return memberName; } }

        public AttachableMemberIdentifier(Type declaringType, string memberName)
        {
            this.declaringType = declaringType;
            this.memberName = memberName;
        }

        public static bool operator !=(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
        {
            return !(left == right);
        }

        public static bool operator ==(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
        {
            if (object.ReferenceEquals(left, null))
                return object.ReferenceEquals(right, null);
            return !object.ReferenceEquals(right, null) && left.declaringType == right.declaringType && left.memberName == right.memberName;
        }

        public bool Equals(AttachableMemberIdentifier other)
        {
            return other != null && other.declaringType == declaringType && other.memberName == memberName;
        }

        public override bool Equals(object obj)
        {
            var other = obj as AttachableMemberIdentifier;
            return other != null ? Equals(other) : false;
        }

        public override int GetHashCode()
        {
            return declaringType.GetHashCode() ^ memberName.GetHashCode();
        }
    }
    
    public interface IAttachedPropertyStore
    {
        int PropertyCount { get; }
        void CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index);
        bool RemoveProperty(AttachableMemberIdentifier attachableMemberIdentifier);
        void SetProperty(AttachableMemberIdentifier attachableMemberIdentifier, object value);
        bool TryGetProperty(AttachableMemberIdentifier attachableMemberIdentifier, out object value);
    }

    public class AttachablePropertyServices
    {
        public static void CopyPropertiesTo(object instance, KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
        {
            var store = instance as IAttachedPropertyStore;
            if (store != null)
                store.CopyPropertiesTo(array, index);
        }
        
        public static int GetAttachedPropertyCount(object instance)
        {
            var store = instance as IAttachedPropertyStore;
            if (store != null)
                return store.PropertyCount;
            return 0;
        }
        
        public static bool RemoveProperty(object instance, AttachableMemberIdentifier name)
        {
            var store = instance as IAttachedPropertyStore;
            if (store != null)
                return store.RemoveProperty(name);
            return false;
        }
        
        public static void SetProperty(object instance, AttachableMemberIdentifier name, object value)
        {
            var store = instance as IAttachedPropertyStore;
            if (store != null)
                store.SetProperty(name, value);
        }
        
        public static bool TryGetProperty(object instance, AttachableMemberIdentifier name, out object value)
        {
            var store = instance as IAttachedPropertyStore;
            if (store != null)
                return store.TryGetProperty(name, out value);
            value = null;
            return false;
        }
        
        public static bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
        {
            object obj;
            var store = instance as IAttachedPropertyStore;
            if (store != null && store.TryGetProperty(name, out obj) && obj is T)
            {
                value = (T)obj;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}