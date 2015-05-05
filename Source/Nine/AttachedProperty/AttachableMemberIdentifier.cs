namespace Nine.AttachedProperty
{
    using System;
    using System.Linq;
    using System.Reflection;
    
    /// <summary>
    /// 
    /// </summary>
    public class AttachableMemberIdentifier : IEquatable<AttachableMemberIdentifier>
    {
        // TODO: AttachableMemberIdentifier

        /// <summary>  </summary>
        public Type DeclaringType { get; set; }

        /// <summary>  </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public AttachableMemberIdentifier(Type declaringType, string memberName)
        {
            this.DeclaringType = declaringType;
            this.MemberName = memberName;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Apply(object target, object value)
        {
            if (this.DeclaringType == null)
                return;

            var runtimeMethods = this.DeclaringType.GetRuntimeMethods();
            var setMethod = runtimeMethods.Where(e => e.Name == string.Concat("Set", this.MemberName) && (e.IsStatic || e.IsPublic || e.IsPrivate)).First();
            if (setMethod != null)
                setMethod.Invoke(null, new object[] { target, value });
        }

        public bool Equals(AttachableMemberIdentifier other)
        {
            return this.DeclaringType == other.DeclaringType && this.MemberName == other.MemberName;
        }
    }
}
