namespace Nine.Content.Pipeline.Xaml
{
    using System.Windows.Markup;
    using System.Xaml;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

    [ContentTypeWriter]
    class ContentReferenceWriter : ContentTypeWriter<ContentReference>
    {
        protected override void Write(ContentWriter output, ContentReference value)
        {
            output.Write(value.AssetName);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return (typeof(ContentTypeReader).Namespace + ".ExternalReferenceReader");
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

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            if (targetPlatform == TargetPlatform.Windows)
                return typeof(AttachableMemberIdentifier).AssemblyQualifiedName;
            return "System.Xaml.AttachableMemberIdentifier, Nine, Version=1.4.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AttachableMemberIdentifierReader).AssemblyQualifiedName;
        }
    }
}
