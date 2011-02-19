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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Content.Pipeline
{    
    /*
    [ContentTypeSerializer]
    class RangeSerializer<T> : ContentTypeSerializer<Range<T>>
    {
        protected override Range<T> Deserialize(IntermediateReader input, ContentSerializerAttribute format, Range<T> existingInstance)
        {
            Range<T> range = new Range<T>();

            input.Xml.ReadStartElement("Min");
            range.Min = input.ReadObject<T>(null);
            input.Xml.ReadEndElement();

            input.Xml.ReadStartElement("Max");
            range.Max = input.ReadObject<T>(null);
            input.Xml.ReadEndElement();

            return range;
        }

        protected override void Serialize(IntermediateWriter output, Range<T> value, ContentSerializerAttribute format)
        {
            if (value.Min.Equals(value.Max))
            {
                output.WriteObject(value.Min, format);
            }
            else
            {
                output.Xml.WriteStartElement("Min");
                output.WriteObject(value.Min, format);
                output.Xml.WriteEndElement();

                output.Xml.WriteStartElement("Max");
                output.WriteObject(value.Max, format);
                output.Xml.WriteEndElement();
            }
        }
    }
     */
}
