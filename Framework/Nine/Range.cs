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

namespace Nine
{
    /// <summary>
    /// A generic struct that contains a Min and Max value.
    /// </summary>
#if WINDOWS
    [Serializable()]
#endif
    public struct Range<T> : IEquatable<Range<T>>
    {
        /// <summary>
        /// Gets or sets the min value.
        /// </summary>
        public T Min;

        /// <summary>
        /// Gets or sets the max value.
        /// </summary>
        public T Max;

        /// <summary>
        /// Create a new instance of Range object.
        /// </summary>
        public Range(T min, T max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Create a new instance of Range object from a value.
        /// </summary>
        public static implicit operator Range<T>(T value)
        {
            return new Range<T> { Min = value, Max = value };
        }

        /// <summary>
        /// Determines if a Range<T> object equals to another Range<T> object
        /// </summary>
        public bool Equals(Range<T> other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        public override string ToString()
        {
            return Min.ToString() + " ~ " + Max.ToString();
        }
    }
}