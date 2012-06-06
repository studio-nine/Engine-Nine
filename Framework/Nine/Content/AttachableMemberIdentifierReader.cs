#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework.Content;
using System.Xaml;
using System;
#endregion

namespace Nine.Content
{
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
