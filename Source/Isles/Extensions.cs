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
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles
{
    #region ServiceProviderExtensions
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider provider) where T : class
        {
            return provider.GetService(typeof(T)) as T;
        }

        public static K GetService<T, K>(this IServiceProvider provider)
            where T : class, K
            where K : class
        {
            return provider.GetService(typeof(T)) as K;
        }

        public static K GetService<K>(this IServiceProvider provider, Type type)
            where K : class
        {
            return provider.GetService(type) as K;
        }

        public static K GetService<K>(this IServiceProvider provider, string type)
            where K : class
        {
            return provider.GetService(Type.GetType(type)) as K;
        }
    }
    #endregion

    #region MatrixExtensions
    public static class MatrixExtensions
    {
        // Color matrix
        public static Matrix CreateColorMatrix(float brightness, float contrast, float saturation, float hue)
        {
            return Matrix.Identity;
        }

        public static Matrix CreateBrightness(float amount)
        {
            return Matrix.CreateTranslation(amount, amount, amount);
        }

        public static Matrix CreateContrast(float amount)
        {
            throw new NotImplementedException();
        }

        public static Matrix CreateHue(float amount)
        {
            throw new NotImplementedException();
        }

        public static Matrix CreateSaturation(float amount)
        {
            throw new NotImplementedException();
        }

        
        // 2D matrix transform
        public static Matrix CreateTranslation(float x, float y)
        {
            return Matrix.CreateTranslation(x, y, 0);
        }

        public static Matrix CreateRotation(float radians)
        {
            return Matrix.CreateRotationZ(radians);
        }

        public static Matrix CreateScale(float x, float y)
        {
            return Matrix.CreateScale(x, y, 1);
        }
    }
    #endregion

    #region ParseExtensions
    public static class ParseExtensions
    {
        #region String parser
        public static Color ToColor(this string value)
        {
            string[] split = value.Split(new char[] { ',' }, 3);

            if (split.Length == 3)
                return new Color(byte.Parse(split[0]),
                                 byte.Parse(split[1]),
                                 byte.Parse(split[2]), 255);
            else if (split.Length > 3)
                return new Color(byte.Parse(split[0]),
                                 byte.Parse(split[1]),
                                 byte.Parse(split[2]),
                                 byte.Parse(split[3]));

            return Color.White;
        }

        public static Vector2 ToVector2(this string value)
        {
            string[] split = value.Split(new Char[] { ',' }, 2);

            if (split.Length >= 2)
                return new Vector2(
                    float.Parse(split[0]),
                    float.Parse(split[1]));

            return Vector2.Zero;
        }

        public static Vector3 ToVector3(this string value)
        {
            string[] split = value.Split(new Char[] { ',' }, 3);

            if (split.Length >= 3)
                return new Vector3(
                    float.Parse(split[0]),
                    float.Parse(split[1]),
                    float.Parse(split[2]));

            return Vector3.Zero;
        }

        public static Vector4 ToVector4(this string value)
        {
            string[] split = value.Split(new Char[] { ',' }, 3);

            if (split.Length >= 4)
                return new Vector4(
                    float.Parse(split[0]),
                    float.Parse(split[1]),
                    float.Parse(split[2]),
                    float.Parse(split[3]));

            return Vector4.Zero;
        }

        public static Matrix ToMatrix(this string value)
        {
            string[] split = value.Split(new Char[] { ',' }, 16);

            if (split.Length >= 16)
                return new Matrix(
                    float.Parse(split[0]),
                    float.Parse(split[1]),
                    float.Parse(split[2]),
                    float.Parse(split[3]),
                    float.Parse(split[4]),
                    float.Parse(split[5]),
                    float.Parse(split[6]),
                    float.Parse(split[7]),
                    float.Parse(split[8]),
                    float.Parse(split[9]),
                    float.Parse(split[10]),
                    float.Parse(split[11]),
                    float.Parse(split[12]),
                    float.Parse(split[13]),
                    float.Parse(split[14]),
                    float.Parse(split[15]));

            return Matrix.Identity;
        }

        public static Quaternion ToQuaternion(this string value)
        {
            string[] split = value.Split(new Char[] { ',' }, 4);

            if (split.Length >= 3)
                return new Quaternion(
                    float.Parse(split[0]),
                    float.Parse(split[1]),
                    float.Parse(split[2]),
                    float.Parse(split[3]));

            return Quaternion.Identity;
        }

        public static Point ToPoint(this string value)
        {
            string[] split = value.Split(new Char[] { ',' }, 2);

            if (split.Length >= 2)
                return new Point(
                    int.Parse(split[0]),
                    int.Parse(split[1]));

            return Point.Zero;
        }

        public static Rectangle ToRectangle(this string value)
        {
            string[] split = value.Split(new Char[] { ',' }, 4);

            if (split.Length >= 4)
                return new Rectangle(
                    int.Parse(split[0]),
                    int.Parse(split[1]),
                    int.Parse(split[2]),
                    int.Parse(split[3]));

            return Rectangle.Empty;
        }
        #endregion

        #region ToString
        public static string ToString(Color c)
        {
            return c.R + ", " + c.G + ", " + c.B + ", " + c.A;
        }

        public static string ToString(Vector2 v)
        {
            return v.X + ", " + v.Y;
        }

        public static string ToString(Vector3 v)
        {
            return v.X + ", " + v.Y + ", " + v.Z;
        }

        public static string ToString(Vector4 v)
        {
            return v.X + ", " + v.Y + ", " + v.Z + ", " + v.W;
        }

        public static string ToString(Matrix m)
        {
            return
                m.M11 + ", " + m.M12 + ", " + m.M13 + ", " + m.M14 + ", " +
                m.M21 + ", " + m.M22 + ", " + m.M23 + ", " + m.M24 + ", " +
                m.M31 + ", " + m.M32 + ", " + m.M33 + ", " + m.M34 + ", " +
                m.M41 + ", " + m.M42 + ", " + m.M43 + ", " + m.M44;
        }

        public static string ToString(Quaternion q)
        {
            return q.X + ", " + q.Y + ", " + q.Z + ", " + q.W;
        }

        public static string ToString(Point p)
        {
            return p.X + ", " + p.Y;
        }

        public static string ToString(Rectangle r)
        {
            return r.X + ", " + r.Y + ", " + r.Width + ", " + r.Height;
        }
        #endregion
    }
    #endregion
}
