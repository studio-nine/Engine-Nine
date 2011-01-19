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

        public bool Equals(LineSegment other)
        {
            return Start == other.Start && End == other.End;
        }

        public override bool Equals(object obj)
        {
            if (obj is LineSegment)
                return Equals((LineSegment)obj);

            return false;
        }

        public static bool operator ==(LineSegment value1, LineSegment value2)
        {
            return ((value1.Start == value2.Start) && (value1.End == value2.End));
        }

        public static bool operator !=(LineSegment value1, LineSegment value2)
        {
            return !(value1.Start == value2.Start && value1.End == value2.End);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() + End.GetHashCode();
        }

        public override string ToString()
        {
            return Start.ToString() + " - " + End.ToString();
        }
    }
}