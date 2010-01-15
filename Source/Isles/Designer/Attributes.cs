#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion


namespace Isles.Designer
{
    public sealed class PositionAttribute : Attribute { }

    public sealed class RotationAttribute : Attribute { }

    public sealed class ScaleAttribute : Attribute { }

    public sealed class TransformAttribute : Attribute { }

    public sealed class ColorAttribute : Attribute { }

    public sealed class SliderAttribute : Attribute
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public float Step { get; set; }
    }
}