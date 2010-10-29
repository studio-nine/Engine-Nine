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
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Helper class to create texture transformation matrices.
    /// </summary>
    public static class TextureTransform
    {
        public static Matrix CreateTranslation(float x, float y)
        {
            return Matrix.CreateTranslation(x, y, 0);
        }

        public static Matrix CreateScale(float x, float y)
        {
            return Matrix.CreateScale(x, y, 1);
        }

        public static Matrix CreateRotation(float radius)
        {
            return Matrix.CreateRotationZ(radius);
        }

        public static float[] ToArray(Matrix matrix)
        {
            return new float[]
            {
                matrix.M11, matrix.M12, matrix.M14,
                matrix.M21, matrix.M22, matrix.M24,
                matrix.M41, matrix.M42, matrix.M44,
            };
        }
    }
}
