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
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    #region Interpolate
    /// <summary>
    /// A method used to interpolate the specified type.
    /// </summary>
    public delegate T Interpolate<T>(T x, T y, float amount);
    #endregion

    #region LerpHelper
    public static class LerpHelper
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
            Rectangle rc;

            rc.X = Lerp(x.X, y.X, amount);
            rc.Y = Lerp(x.Y, y.Y, amount);
            rc.Width = Lerp(x.Width, y.Width, amount);
            rc.Height = Lerp(x.Height, y.Height, amount);

            return rc;
        }

        public static Point Lerp(Point x, Point y, float amount)
        {
            Point rc;

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
        /// <param name="start">Source matrix for interpolation</param>
        /// <param name="end">Destination matrix for interpolation</param>
        /// <param name="slerpAmount">Ratio of interpolation</param>
        /// <returns>The interpolated matrix</returns>
        public static Matrix Slerp(Matrix start, Matrix end, float slerpAmount)
        {
            if (start == end)
                return start;
        
            Quaternion qStart, qEnd, qResult;
            Vector3 curTrans, nextTrans, lerpedTrans;
            Vector3 curScale, nextScale, lerpedScale;
            Matrix startRotation, endRotation;
            Matrix returnMatrix;

            // Get rotation components and interpolate (not completely accurate but I don't want 
            // to get into polar decomposition and this seems smooth enough)
            Quaternion.CreateFromRotationMatrix(ref start, out qStart);
            Quaternion.CreateFromRotationMatrix(ref end, out qEnd);
            Quaternion.Lerp(ref qStart, ref qEnd, slerpAmount, out qResult);

            // Get final translation components
            curTrans.X = start.M41;
            curTrans.Y = start.M42;
            curTrans.Z = start.M43;
            nextTrans.X = end.M41;
            nextTrans.Y = end.M42;
            nextTrans.Z = end.M43;
            Vector3.Lerp(ref curTrans, ref nextTrans, slerpAmount, out lerpedTrans);

            // Get final scale component
            Matrix.CreateFromQuaternion(ref qStart, out startRotation);
            Matrix.CreateFromQuaternion(ref qEnd, out endRotation);
            curScale.X = start.M11 - startRotation.M11;
            curScale.Y = start.M22 - startRotation.M22;
            curScale.Z = start.M33 - startRotation.M33;
            nextScale.X = end.M11 - endRotation.M11;
            nextScale.Y = end.M22 - endRotation.M22;
            nextScale.Z = end.M33 - endRotation.M33;
            Vector3.Lerp(ref curScale, ref nextScale, slerpAmount, out lerpedScale);

            // Create the rotation matrix from the slerped quaternions
            Matrix.CreateFromQuaternion(ref qResult, out returnMatrix);

            // Set the translation
            returnMatrix.M41 = lerpedTrans.X;
            returnMatrix.M42 = lerpedTrans.Y;
            returnMatrix.M43 = lerpedTrans.Z;

            // And the lerped scale component
            returnMatrix.M11 += lerpedScale.X;
            returnMatrix.M22 += lerpedScale.Y;
            returnMatrix.M33 += lerpedScale.Z;

            return returnMatrix;
        }

    }
    #endregion
}