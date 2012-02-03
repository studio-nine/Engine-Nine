#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Content.Pipeline.Graphics.ParticleEffects
{
    [ContentProperty("Controllers")]
    partial class ParticleEffectContent 
    {
        partial void OnCreate()
        {
            BlendState = BlendState.Additive;
        }
    }

    partial class PointEmitterContent
    {
        partial void OnCreate()
        {
            Spread = MathHelper.ToDegrees(Spread);
        }
    }

    partial class PointEmitterContentWriter
    {
        partial void BeginWrite(ContentWriter output, PointEmitterContent value)
        {
            value.Spread = MathHelper.ToRadians(value.Spread);
        }
    }

    partial class BoxEmitterContent
    {
        partial void OnCreate()
        {
            Spread = MathHelper.ToDegrees(Spread);
        }
    }

    partial class BoxEmitterContentWriter
    {
        partial void BeginWrite(ContentWriter output, BoxEmitterContent value)
        {
            value.Spread = MathHelper.ToRadians(value.Spread);
        }
    }

    partial class SphereEmitterContent
    {
        partial void OnCreate()
        {
            Spread = MathHelper.ToDegrees(Spread);
        }
    }

    partial class SphereEmitterContentWriter
    {
        partial void BeginWrite(ContentWriter output, SphereEmitterContent value)
        {
            value.Spread = MathHelper.ToRadians(value.Spread);
        }
    }

    partial class CylinderEmitterContent
    {
        partial void OnCreate()
        {
            Spread = MathHelper.ToDegrees(Spread);
        }
    }

    partial class CylinderEmitterContentWriter
    {
        partial void BeginWrite(ContentWriter output, CylinderEmitterContent value)
        {
            value.Spread = MathHelper.ToRadians(value.Spread);
        }
    }

    partial class LineEmitterContent
    {
        partial void OnCreate()
        {
            Spread = MathHelper.ToDegrees(Spread);
        }
    }

    partial class LineEmitterContentWriter
    {
        partial void BeginWrite(ContentWriter output, LineEmitterContent value)
        {
            value.Spread = MathHelper.ToRadians(value.Spread);
        }
    }
}
