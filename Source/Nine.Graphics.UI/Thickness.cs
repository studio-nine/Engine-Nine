#region License
/* The MIT License
 *
 * Copyright (c) 2013 Engine Nine
 * Copyright (c) 2011 Red Badger Consulting
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/
#endregion

namespace Nine.Graphics.UI
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using Nine.Graphics.UI.Internal;

    [DebuggerDisplay("{Left}, {Top}, {Right}, {Bottom}")]
#if WINDOWS
    [Serializable()]
    [TypeConverter(typeof(Nine.Design.ThicknessConverter))]
#endif
    public struct Thickness : IEquatable<Thickness>
    {
        public static readonly Thickness Empty = new Thickness();

        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public Thickness(float left, float top)
            : this(left, top, left, top)
        {
        }

        public Thickness(float uniformLength)
            : this(uniformLength, uniformLength, uniformLength, uniformLength)
        {
        }

        public Thickness(float left, float top, float right, float bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public static implicit operator Thickness(int i)
        {
            return new Thickness(0);
        }

        /// <summary>
        ///     Adds two <see cref = "Thickness">Thickness</see> instances together (e.g. adds the two <see cref = "Left">Left</see> components, the two <see cref = "Top">Top</see> components etc).
        /// </summary>
        /// <param name = "left">The first <see cref = "Thickness">Thickness</see></param>
        /// <param name = "right">The second <see cref = "Thickness">Thickness</see></param>
        /// <returns>A <see cref = "Thickness">Thickness</see> whose components represent the sum of the two <see cref = "Thickness">Thickness</see> instances.</returns>
        public static Thickness operator +(Thickness left, Thickness right)
        {
            return new Thickness(
                left.Left + right.Left, left.Top + right.Top, left.Right + right.Right, left.Bottom + right.Bottom);
        }

        public static bool operator ==(Thickness left, Thickness right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Thickness left, Thickness right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (obj.GetType() != typeof(Thickness))
            {
                return false;
            }

            return this.Equals((Thickness)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = this.Bottom.GetHashCode();
                result = (result * 397) ^ this.Left.GetHashCode();
                result = (result * 397) ^ this.Right.GetHashCode();
                result = (result * 397) ^ this.Top.GetHashCode();
                return result;
            }
        }

        public bool Equals(Thickness other)
        {
            return other.Bottom.IsCloseTo(this.Bottom) && other.Left.IsCloseTo(this.Left) &&
                   other.Right.IsCloseTo(this.Right) && other.Top.IsCloseTo(this.Top);
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", Left, Top, Right, Bottom);
        }
    }
}
