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
        public Range(T value)
        {
            Min = value;
            Max = value;
        }

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

        public bool Equals(Range<T> other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        public override bool Equals(object obj)
        {
            if (obj is Range<T>)
                return Equals((Range<T>)obj);

            return false;
        }

        public static bool operator ==(Range<T> value1, Range<T> value2)
        {
            return (value1.Equals(value2.Min) && value1.Max.Equals(value2.Max));
        }

        public static bool operator !=(Range<T> value1, Range<T> value2)
        {
            if (value1.Min.Equals(value2.Min))
            {
                return !(value1.Max.Equals(value2.Max));
            }
            return true;
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
    }
}