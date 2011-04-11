#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Content.Pipeline.Graphics
{
    /// <summary>
    /// Content representation for all types that requires a GraphicsDevice in their constructor.
    /// </summary>
    public class GraphicsContent<T> : ContentItem
    {
        /// <summary>
        /// Gets and sets the value of this GraphicsContent.
        /// </summary>
        public T Value { get; set; }

        public GraphicsContent() { }
        public GraphicsContent(T value) { Value = value; }
    }

    [ContentTypeWriter]
    class GraphicsContentWriter<T> : ContentTypeWriter<GraphicsContent<T>>
    {
        protected override void Write(ContentWriter output, GraphicsContent<T> value)
        {

        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(object).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(GraphicsContentReader).AssemblyQualifiedName;
        }
    }

    [ContentTypeSerializer]
    class GraphicsContentSerializer<T> : ContentTypeSerializer<GraphicsContent<T>>
    {
        protected override GraphicsContent<T> Deserialize(IntermediateReader input, ContentSerializerAttribute format, GraphicsContent<T> existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new GraphicsContent<T>();
            if (existingInstance.Value == null)
                existingInstance.Value = ReflectionHelper.CreateInstance<T>(MockedGraphicsDevice.GraphicsDevice);

            ContentTypeSerializer serializer = input.Serializer.GetTypeSerializer(typeof(T));
            existingInstance.Value = (T)ReflectionHelper.Invoke(serializer, "Deserialize", input, format, existingInstance.Value);
            return existingInstance;
        }

        protected override void Serialize(IntermediateWriter output, GraphicsContent<T> value, ContentSerializerAttribute format)
        {

        }
    }
}
