#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;

#endregion

namespace Nine.Content.Pipeline.Graphics.Effects
{
    using Nine.Content.Pipeline.Processors;

    [DefaultContentProcessor(typeof(LinkedMaterialProcessor))]
    partial class LinkedMaterialContent
    {
        partial void OnCreate()
        {
            EffectParts = new List<LinkedEffectPartContent>();
        }
    }
}
