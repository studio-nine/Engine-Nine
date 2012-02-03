#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace Nine.Content
{
    [ContentTypeWriter()]
    class NotificationCollectionWriter<T> : ContentTypeWriter<NotificationCollection<T>>
    {
        ContentTypeWriter elementWriter;

        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        protected override void Initialize(ContentCompiler compiler)
        {
            elementWriter = compiler.GetTypeWriter(typeof(T));
        }

        protected override void Write(ContentWriter output, NotificationCollection<T> value)
        {
            output.Write(value.Count);
            foreach (var item in value)
                output.WriteObject<T>(item, elementWriter);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(NotificationCollection<T>).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(NotificationCollectionReader<T>).AssemblyQualifiedName;
        }
    }
}
