#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Linq;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
using Nine.Content.Pipeline.Graphics.Effects;
using Nine.Content.Pipeline.Graphics.Effects.EffectParts;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// This processor generates the appropriate texture and materials for the input terrain.
    /// </summary>
    [DesignTimeVisible(false)]
    [ContentProcessor(DisplayName="Model Material - Engine Nine")]
    public class ModelMaterialProcessor : ContentProcessor<ModelMaterialContent, LinkedMaterialContent>
    {
        public override LinkedMaterialContent Process(ModelMaterialContent input, ContentProcessorContext context)
        {
            var material = new LinkedMaterialContent();
            material.DepthAlphaEnabled = input.DepthAlphaEnabled;
            material.IsTransparent = input.IsTransparent;
            material.Effect = new ContentReference<CompiledEffectContent>(input.Build(context).Filename);
            return material;
        }
    }
}
