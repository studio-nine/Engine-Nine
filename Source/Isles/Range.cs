#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
#endregion

namespace Isles
{
    [Serializable()]
    public struct Range<T> : IEquatable<Range<T>>
    {
        public T Min;
        public T Max;

        public Range(T min, T max)
        {
            Min = min;
            Max = max;
        }

        public static implicit operator Range<T>(T value)
        {
            return new Range<T> { Min = value, Max = value };
        }

        public bool Equals(Range<T> other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }
    }
}