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
            return left.declaringType != right.declaringType || left.memberName != right.memberName;
        }

        public static bool operator ==(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
        {
            return left.declaringType == right.declaringType && left.memberName == right.memberName;
        }

        public bool Equals(AttachableMemberIdentifier other)
        {
            return other.declaringType == declaringType && other.memberName == memberName;
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
}