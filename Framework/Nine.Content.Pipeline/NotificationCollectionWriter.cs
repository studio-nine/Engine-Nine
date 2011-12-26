#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace Nine.Content
{
    [ContentTypeWriter()]
    class NotificationCollectionWriter<T> : ContentTypeWriter<NotificationCollection<T>>
    {
        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        protected override void Write(ContentWriter output, NotificationCollection<T> value)
        {
            output.Write(value.Count);
            foreach (var item in value)
                output.WriteObject<T>(item);
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
