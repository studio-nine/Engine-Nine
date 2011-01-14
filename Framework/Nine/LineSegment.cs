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
using Microsoft.Xna.Framework;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a 2D line segment.
    /// </summary>
#if WINDOWS
    [Serializable()]
#endif
    public struct LineSegment : IEquatable<LineSegment>
    {
        public Vector2 Start;

        public Vector2 End;

        public Vector2 Normal
        {
            get { return Math2D.Rotate90DegreesCcw(Vector2.Normalize(End - Start)); }
        }

        public Vector2 Center
        {
            get { return (Start + End) * 0.5f; }
        }

        public LineSegment(Vector2 start, Vector2 end)
        {
            this.Start = start;
            this.End = end;
        }

        public float Length()
        {
            return Vector2.Distance(Start, End);
        }

        public float LengthSquared()
        {
            return Vector2.DistanceSquared(Start, End);
        }
        
        public void Offset(float length)
        {
            Vector2 normal = Normal;

            Start += normal * length;
            End += normal * length;
        }

        /// <summary>
        /// Determines if a Range object equals to another Range object
        /// </summary>
        public bool Equals(LineSegment other)
        {
            return Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override string ToString()
        {
            return Start.ToString() + " - " + End.ToString();
        }
    }
}