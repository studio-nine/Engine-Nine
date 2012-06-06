#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Content.Pipeline.Xaml;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Content.Pipeline.Xaml
{
    [ContentTypeWriter]
    class ContentReferenceWriter : ContentTypeWriter<ContentReference>
    {
        protected override void Write(ContentWriter output, ContentReference value)
        {
            output.Write(value.Name);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return (typeof(ContentTypeReader).Namespace + ".ExternalReferenceReader");
        }
    }

    [ContentTypeWriter]
    class ReferenceWriter : ContentTypeWriter<Reference>
    {
        protected override void Write(ContentWriter output, Reference value)
        {
            output.WriteSharedResource(0);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "";
        }
    }

    [ContentTypeWriter]
    class AttachableMemberIdentifierWriter : ContentTypeWriter<AttachableMemberIdentifier>
    {
        protected override void Write(ContentWriter output, AttachableMemberIdentifier value)
        {
            output.Write(value.DeclaringType.AssemblyQualifiedName);
            output.Write(value.MemberName);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AttachableMemberIdentifierReader).AssemblyQualifiedName;
        }
    }
}
