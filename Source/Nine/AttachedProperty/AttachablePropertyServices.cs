namespace Nine.AttachedProperty
{
    using System;
    using System.Reflection;

    // TODO: AttachablePropertyServices

    public class AttachablePropertyServices
    {
        public static void SetProperty(object instance, AttachableMemberIdentifier name, object value)
        {
            var attachable = instance as IAttachedPropertyStore;
            if (attachable != null)
            {
                attachable.SetProperty(name, value);
            }
        }

        public static bool TryGetProperty(object instance, AttachableMemberIdentifier name, out object value)
        {
            throw new System.NotImplementedException();
        }

        public static bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
        {
            if (instance == null || name == null)
                throw new ArgumentNullException();
            
            var attachable = instance as IAttachedPropertyStore;
            if (attachable != null)
            {
                object valueResult;
                if (attachable.TryGetProperty(name, out valueResult))
                {
                    value = (T)valueResult;
                    return true;
                }
            }

            value = default(T);
            return false;
        }
    }
}
