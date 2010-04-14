#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion


namespace Isles.Graphics.Filters
{
    public partial class ColorMatrixEffect
    {
        public static Matrix CreateColorMatrix(float brightness, float contrast, float saturation, float hue)
        {
            return Matrix.Identity;
        }

        public static Matrix CreateBrightness(float amount)
        {
            return Matrix.CreateTranslation(amount, amount, amount);
        }

        public static Matrix CreateSaturation(float amount)
        {
            /*
            float r = 0.3086f;
			float g = 0.6094f;
			float b = 0.0820f;
            
            return new Matrix(
                r * (1 - amount) + amount, g * (1 - amount), b * (1 - amount), 0,
                r * (1 - amount), g * (1 - amount) + amount, b * (1 - amount), 0,
                r * (1 - amount), g * (1 - amount), b * (1 - amount) + amount, 0,
                0, 0, 0, 1);
             */
            
            return Matrix.Identity;
        }
    }
}

