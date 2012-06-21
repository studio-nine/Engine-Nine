﻿#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics.Materials;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace Nine.Content.Pipeline.Graphics.Materials
{
    [ContentTypeWriter]
    class MaterialGroupWriter : ContentTypeWriter<MaterialGroup>
    {
        protected override void Write(ContentWriter output, MaterialGroup value)
        {
            // Windows Phone doesn't support custom shaders.
            if (output.TargetPlatform == TargetPlatform.WindowsPhone)
                return;

            WriteObject(output, value, "AttachedProperties", value.AttachedProperties);

            output.Write(value.IsTransparent);
            output.Write(value.TwoSided);
            output.WriteExternalReference(new ExternalReference<CompiledEffectContent>(value.Reference));

            output.Write(value.MaterialParts.Count);
            for (var i = 0; i < value.MaterialParts.Count; i++)
                output.WriteObject(value.MaterialParts[i]);
            
            output.WriteObject(value.ExtendedMaterials);
        }

        private void WriteObject(Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentWriter output, System.Object parent, string member, System.Object value)
        {
            var propertyInstance = new Nine.Content.Pipeline.Xaml.PropertyInstance(parent, member);
            var serializationData = Nine.Content.Pipeline.Xaml.XamlSerializer.SerializationData;
            if (serializationData.ContainsKey(propertyInstance))
                output.WriteObject(serializationData[propertyInstance]);
            else
                output.WriteObject(value);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            // Set this to avoid the complaining from the content pipeline,
            // so the build process won't fall if you happened to include
            // this in a Windows Phone project.
            if (targetPlatform == TargetPlatform.WindowsPhone)
                return typeof(System.String).AssemblyQualifiedName;

            return typeof(MaterialGroup).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // Set this to avoid the complaining from the content pipeline,
            // so the build process won't fall if you happened to include
            // this in a Windows Phone project.
            if (targetPlatform == TargetPlatform.WindowsPhone)
                return typeof(System.String).AssemblyQualifiedName;

            return typeof(MaterialGroupReader).AssemblyQualifiedName;
        }
    }

}