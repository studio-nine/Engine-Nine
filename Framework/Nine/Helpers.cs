#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// A method used to interpolate the specified type.
    /// </summary>
    public delegate T Interpolate<T>(T x, T y, float amount);

    /// <summary>
    /// Helper class to interpolate common types.
    /// </summary>
    internal static class LerpHelper
    {
        public static int Lerp(int x, int y, float amount)
        {
            return (int)(x * (1 - amount) + y * amount);
        }

        public static double Lerp(double x, double y, float amount)
        {
            return x * (1 - amount) + y * amount;
        }

        public static Rectangle Lerp(Rectangle x, Rectangle y, float amount)
        {
            Rectangle rc = new Rectangle();

            rc.X = Lerp(x.X, y.X, amount);
            rc.Y = Lerp(x.Y, y.Y, amount);
            rc.Width = Lerp(x.Width, y.Width, amount);
            rc.Height = Lerp(x.Height, y.Height, amount);

            return rc;
        }

        public static Point Lerp(Point x, Point y, float amount)
        {
            Point rc = new Point();

            rc.X = Lerp(x.X, y.X, amount);
            rc.Y = Lerp(x.Y, y.Y, amount);

            return rc;
        }

        public static Ray Lerp(Ray x, Ray y, float amount)
        {
            Ray r;

            r.Position = Vector3.Lerp(x.Position, y.Position, amount);
            r.Direction = Vector3.Lerp(x.Direction, y.Direction, amount);

            return r;
        }

        /// <summary>
        /// Roughly decomposes two matrices and performs spherical linear interpolation
        /// </summary>
        /// <param name="a">Source matrix for interpolation</param>
        /// <param name="b">Destination matrix for interpolation</param>
        /// <param name="amount">Ratio of interpolation</param>
        /// <returns>The interpolated matrix</returns>
        public static Matrix Slerp(Matrix a, Matrix b, float amount)
        {
            Quaternion rotationA, rotationB, rotation;
            Vector3 scaleA, scaleB, scale;
            Vector3 translationA, translationB, translation;
            Matrix mxScale, mxRotation;

            if (a.Decompose(out scaleA, out  rotationA, out translationA) &&
                b.Decompose(out scaleB, out  rotationB, out translationB))
            {
                Vector3.Lerp(ref scaleA, ref scaleB, amount, out scale);
                Vector3.Lerp(ref translationA, ref translationB, amount, out translation);
                Quaternion.Slerp(ref rotationA, ref rotationB, amount, out rotation);

                Matrix.CreateScale(ref scale, out mxScale);
                Matrix.CreateFromQuaternion(ref rotation, out mxRotation);
                Matrix.Multiply(ref mxScale, ref mxRotation, out mxRotation);

                mxRotation.M41 += translation.X;
                mxRotation.M42 += translation.Y;
                mxRotation.M43 += translation.Z;
                return mxRotation;
            }

            throw new InvalidOperationException(Strings.CannotDecomposeMatrix);
        }

        /// <summary>
        /// Implemements a weighted sum algorithm of quaternions.
        /// See http://en.wikipedia.org/wiki/Generalized_quaternion_interpolation for detailed math formulars.
        /// </summary>
        public static Quaternion WeightedSum(IEnumerable<Quaternion> quaternions, IEnumerable<float> weights, int steps)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Contains helper method for Matrix.
    /// </summary>
    internal static class MatrixHelper
    {
        public static Matrix CreateRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            Vector3 axis = Vector3.Cross(fromDirection, toDirection);

            if (axis == Vector3.Zero)
                return Matrix.Identity;
            
            axis.Normalize();
            return Matrix.CreateFromAxisAngle(axis, (float)Math.Acos(Vector3.Dot(fromDirection, toDirection)));
        }

        public static float GetFarClip(this Matrix projection)
        {
            return Math.Abs(projection.M43 / (Math.Abs(projection.M33) - 1));
        }

        public static float GetNearClip(this Matrix projection)
        {
            return Math.Abs(projection.M43 / projection.M33);
        }
    }
}