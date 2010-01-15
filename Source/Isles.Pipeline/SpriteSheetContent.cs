#region File Description
//-----------------------------------------------------------------------------
// SpriteSheetContent.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace Isles.Pipeline
{
    /// <summary>
    /// Build-time type used to hold the output data from the SpriteSheetProcessor.
    /// This is saved into XNB format by the SpriteSheetWriter helper class, then
    /// at runtime, the SpriteSheetReader loads the data into a SpriteSheet object.
    /// </summary>
    public class SpriteSheetContent
    {
        // Single texture contains many separate sprite images.
        public Texture2DContent Texture { get; set; }

        // Remember where in the texture each sprite has been placed.
        public List<Rectangle> SpriteRectangles { get; set; }

        // Store the original sprite filenames, so we can look up sprites by name.
        public Dictionary<string, int> SpriteNames { get; set; }


        public SpriteSheetContent()
        {
            Texture = new Texture2DContent();
            SpriteRectangles = new List<Rectangle>();
            SpriteNames = new Dictionary<string, int>();
        }
    }


    /// <summary>
    /// Content pipeline support class for saving sprite sheet data into XNB format.
    /// </summary>
    [ContentTypeWriter]
    public class SpriteSheetWriter : ContentTypeWriter<SpriteSheetContent>
    {
        /// <summary>
        /// Saves sprite sheet data into an XNB file.
        /// </summary>
        protected override void Write(ContentWriter output, SpriteSheetContent value)
        {
            output.WriteObject(value.Texture);
            output.WriteObject(value.SpriteRectangles.ToArray());
            output.WriteObject(value.SpriteNames);
        }


        /// <summary>
        /// Tells the content pipeline what worker type
        /// will be used to load the sprite sheet data.
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(Isles.Graphics.SpriteSheetReader).AssemblyQualifiedName;
        }


        /// <summary>
        /// Tells the content pipeline what CLR type the sprite sheet
        /// data will be loaded into at runtime.
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Isles.Graphics.SpriteSheet).AssemblyQualifiedName;
        }
    }
}
