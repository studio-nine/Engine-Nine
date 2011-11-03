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
#endregion

namespace Nine
{
    /// <summary>
    /// A generic struct that contains a Min and Max value.
    /// </summary>
#if WINDOWS
    [Serializable()]
    [TypeConverter(typeof(Nine.Design.RangeConverter))]
#endif
    public struct Range<T> : IEquatable<Range<T>>
    {
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public T Min;

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public T Max;

        /// <summary>
        /// Create a new instance of Range object from a single value.
        /// </summary>
        public Range(T value)
        {
            Min = value;
            Max = value;
        }

        /// <summary>
        /// Create a new instance of Range object from two values.
        /// </summary>
        public Range(T min, T max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Create a new instance of Range object from a single value.
        /// </summary>
        public static implicit operator Range<T>(T value)
        {
            return new Range<T> { Min = value, Max = value };
        }

        /// <summary>
        /// Test if the current Range is equal to another Range.
        /// </summary>
        /// <param name="other">The Range to compare.</param>
        /// <returns>True if the other is equal.</returns>
        public bool Equals(Range<T> other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        /// <summary>
        /// Test if the current Range is equal to a specified object.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns>True if the object is equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Range<T>)
                return Equals((Range<T>)obj);

            return false;
        }

        /// <summary>
        /// Test if two Range values are equal.
        /// </summary>
        /// <param name="value1">The first value to compare.</param>
        /// <param name="value2">The second value to compare.</param>
        /// <returns>True if the values are equal.</returns>
        public static bool operator ==(Range<T> value1, Range<T> value2)
        {
            return value1.Equals(value2);
        }

        /// <summary>
        /// Test if two Range values are unequal.
        /// </summary>
        /// <param name="value1">The first value to compare.</param>
        /// <param name="value2">The second value to compare.</param>
        /// <returns>True if the values are unequal.</returns>
        public static bool operator !=(Range<T> value1, Range<T> value2)
        {
            return !value1.Equals(value2);
        }

        public override int GetHashCode()
        {
            return Min.GetHashCode() + Max.GetHashCode();
        }

        public override string ToString()
        {
            string min = Min.ToString();
            string max = Max.ToString();
#if WINDOWS
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null && converter.CanConvertTo(typeof(string)))
            {
                min = converter.ConvertToInvariantString(Min);
                max = converter.ConvertToInvariantString(Max);
            }
#endif
            return Min.Equals(Max) ? min : string.Format("{0} ~ {1}", min, max);
        }

        /// <summary>
        /// Force a value to lie within a Range.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>The value, or Min or Max if it lies outside the range.</returns>
        public T Clamp<U>(U value) where U : IComparable, T
        {
            return (value.CompareTo(this.Min) < 0) ? this.Min
                 : (value.CompareTo(this.Max) > 0) ? this.Max
                 :  value;
        }

        /// <summary>
        /// Test whether a value lies within a range.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if value lies within the range.</returns>
        public bool Contains<U>(U value) where U : IComparable, T
        {
            return (value.CompareTo(this.Min) >= 0 && value.CompareTo(this.Max) <= 0);
        }
    }
}