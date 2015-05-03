
namespace System.ComponentModel
{
    public interface ISupportInitialize
    {
        void BeginInit();
        void EndInit();
    }
}

namespace System.Xaml
{
    using System;
    using System.Collections.Generic;

    public class AttachableMemberIdentifier
    {
        public Type DeclaringType 
        {
            get { return declaringType; } 
        }
        private Type declaringType;

        public string MemberName 
        {
            get { return memberName; }
        }
        private string memberName;

        public AttachableMemberIdentifier(Type declaringType, string memberName)
        {
            this.declaringType = declaringType;
            this.memberName = memberName;
        }

        public static bool operator !=(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
        {
            return left.DeclaringType != right.DeclaringType && left.MemberName != right.MemberName;
        }

        public static bool operator ==(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
        {
            return left.DeclaringType == right.DeclaringType && left.MemberName == right.MemberName;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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
        public static bool TryGetProperty(object instance, AttachableMemberIdentifier name, out object value)
        {
            var storage = instance as IAttachedPropertyStore;
            if (storage != null)
            {
                return storage.TryGetProperty(name, out value);
            }
            value = null;
            return false;
        }

        public static bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
        {
            object result;
            if (!TryGetProperty(instance, name, out result)) 
            {
                value = default (T);
                return false;
            }
            value = (T)result;
            return true;
        }

        public static void SetProperty(object instance, AttachableMemberIdentifier name, object value)
        {
            var storage = instance as IAttachedPropertyStore;
            if (storage != null)
            {
                storage.SetProperty(name, value);
            }
        }
    }
}

namespace System.Windows.Markup
{
    public sealed class ContentPropertyAttribute : Attribute
    {
        public string Name
        {
            get { return name; }
        }
        private string name;

        public ContentPropertyAttribute()
        {

        }

        public ContentPropertyAttribute(string name)
        {
            this.name = name;
        }
    }

    public abstract class MarkupExtension
    {
        protected MarkupExtension()
        {

        }

        public abstract object ProvideValue(IServiceProvider serviceProvider);
    }
}
