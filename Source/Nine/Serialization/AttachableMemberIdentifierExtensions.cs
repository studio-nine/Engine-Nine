namespace Nine.Serialization
{
    using System.Reflection;
    using System.Xaml;

    static class AttachableMemberIdentifierExtensions
    {
        public static void Apply(this AttachableMemberIdentifier identifier, object target, object value)
        {
            if (identifier == null || identifier.DeclaringType == null)
                return;

#if WINRT
            var setMethod = identifier.DeclaringType.GetTypeInfo().GetDeclaredMethod(string.Concat("Set", identifier.MemberName));
#else
            var setMethod = identifier.DeclaringType.GetMethod(string.Concat("Set", identifier.MemberName), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);            
#endif
            if (setMethod != null)
                setMethod.Invoke(null, new object[] { target, value });
        }
    }
}
