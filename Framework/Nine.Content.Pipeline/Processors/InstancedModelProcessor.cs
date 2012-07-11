#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Nine.Content.Pipeline.Graphics.Materials;
using Nine.Graphics.Materials;
using Nine.Graphics.Materials.MaterialParts;
using Nine.Graphics.ObjectModel;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    [DefaultContentProcessor]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class InstancedModelProcessor : ContentProcessor<InstancedModel, InstancedModel>
    {
        public override InstancedModel Process(InstancedModel input, ContentProcessorContext context)
        {
            if (input != null && input.Template != null)
            {
                for (int i = 0; i < input.Template.Count; i++)
                {
                    MaterialGroup mg = input.Template.GetMaterial(i) as MaterialGroup;
                    if (mg != null && !mg.MaterialParts.OfType<InstancedMaterialPart>().Any())
                        mg.MaterialParts.Add(new InstancedMaterialPart());
                }
            }
            return input;
        }
    }
}
