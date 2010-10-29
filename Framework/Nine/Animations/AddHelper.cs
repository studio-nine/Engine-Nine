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

namespace Nine.Animations
{
    /// <summary>
    /// Defines an operation on the specified type.
    /// </summary>
    public delegate T Operation<T>(T x, T y);

    /// <summary>
    /// Helper class to add common types.
    /// </summary>
    internal static class AddHelper
    {
        public static int Add(int x, int y)
        {
            return x + y;
        }

        public static float Add(float x, float y)
        {
            return x + y;
        }

        public static double Add(double x, double y)
        {
            return x + y;
        }

        public static Rectangle Add(Rectangle x, Rectangle y)
        {
            Rectangle rc = new Rectangle();

            rc.X = x.X + y.X;
            rc.Y = x.Y + y.Y;
            rc.Width = x.Width + y.Width;
            rc.Height = x.Height + y.Height;

            return rc;
        }

        public static Point Add(Point x, Point y)
        {
            Point rc = new Point();

            rc.X = x.X + y.X;
            rc.Y = x.Y + y.Y;

            return rc;
        }

        public static Color Add(Color x, Color y)
        {
            Color r = new Color();

            r.R = (x.R + y.R < 0xFF ? (byte)(x.R + y.R) : (byte)0xFF);
            r.G = (x.G + y.G < 0xFF ? (byte)(x.G + y.G) : (byte)0xFF);
            r.B = (x.B + y.B < 0xFF ? (byte)(x.B + y.B) : (byte)0xFF);
            r.A = (x.A + y.A < 0xFF ? (byte)(x.A + y.A) : (byte)0xFF);

            return r;
        }

        public static Ray Add(Ray x, Ray y)
        {
            Ray r = new Ray();

            r.Position = x.Position + y.Position;
            r.Direction = Vector3.Normalize(Vector3.Add(x.Direction, y.Direction));

            return r;
        }
    }
}