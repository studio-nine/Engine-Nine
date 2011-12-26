#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics.ScreenEffects;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace Nine.Content.Pipeline.Graphics.ScreenEffects
{
    [ContentProperty("Effects")]
    partial class ScreenEffectContent { }

    [ContentProperty("Effects")]
    partial class ChainedScreenEffectContent { }

    [ContentProperty("Passes")]
    partial class MultiPassScreenEffectContent { }

    [ContentProperty("Effects")]
    partial class MultiPassScreenEffectPassContent { }

    [ContentProperty("Effect")]
    partial class BasicScreenEffectContent
    {
        [SelfProcess]
        static BasicScreenEffectContent Process(BasicScreenEffectContent input, ContentProcessorContext context)
        {
            if (input.Effect is string)
            {
                input.Effect = new ContentReference<CompiledEffectContent>(input.Effect.ToString());
            }
            return input;
        }
    }
}
