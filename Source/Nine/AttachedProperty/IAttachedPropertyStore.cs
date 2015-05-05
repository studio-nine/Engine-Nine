namespace Nine.AttachedProperty
{
    using System.Collections.Generic;
    
    public interface IAttachedPropertyStore
    {
        int PropertyCount { get; }

        void CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index);

        bool RemoveProperty(AttachableMemberIdentifier attachableMemberIdentifier);

        void SetProperty(AttachableMemberIdentifier attachableMemberIdentifier, object value);

        bool TryGetProperty(AttachableMemberIdentifier attachableMemberIdentifier, out object value);
    }
}
