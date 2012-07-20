namespace Nine.Content
{
    using System;
    using System.Xaml;
    using Microsoft.Xna.Framework.Content;

    class AttachableMemberIdentifierReader : ContentTypeReader<AttachableMemberIdentifier>
    {
        protected override AttachableMemberIdentifier Read(ContentReader input, AttachableMemberIdentifier existingInstance)
        {
            var typeName = input.ReadString();
            var member = input.ReadString();
            var type = Type.GetType(typeName);
            return new AttachableMemberIdentifier(type, member);
        }
    }
}
