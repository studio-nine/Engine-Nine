#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
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
            return Matrix.CreateTranslation(-x, -y, 0);
        }

        public static Matrix CreateScale(float x, float y)
        {
            return Matrix.CreateScale(1 / x, 1 / y, 1);
        }

        public static Matrix CreateRotation(float radius)
        {
            return Matrix.CreateRotationZ(-radius);
        }

        public static Matrix CreateSourceRectange(Texture2D texture, Rectangle? rectangle)
        {
            return Matrix.Identity;
        }

        public static Vector2 Transform(Matrix matrix, Vector2 uv)
        {
            Vector3 v3 = new Vector3(uv, 0);

            v3 = Vector3.Transform(v3, matrix);

            return new Vector2(v3.X, v3.Y);
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
