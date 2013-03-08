#region License
/* The MIT License
 *
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

namespace Nine.Graphics.UI.Internal
{
    using System;

    internal static class FloatExtensions
    {
        public static float EnsurePositive(this float value)
        {
            return Math.Max(value, 0f);
        }

        public static bool IsCloseTo(this float value1, float value2)
        {
            return !value1.IsDifferentFrom(value2);
        }

        public static bool IsDifferentFrom(this float value1, float value2)
        {
            if (value1 == value2)
            {
                return false;
            }

            float epsilon = (float)(Math.Abs(value1) + Math.Abs(value2) + 10.0) * 1e-15f;
            float difference = value1 - value2;
            return !(-epsilon < difference && difference < epsilon);
        }

        public static bool IsGreaterThan(this float value1, float value2)
        {
            return value1 > value2 && value1.IsDifferentFrom(value2);
        }

        public static bool IsGreaterThanOrCloseTo(this float value1, float value2)
        {
            return value1 > value2 || value1.IsCloseTo(value2);
        }

        public static bool IsLessThan(this float value1, float value2)
        {
            return value1 < value2 && value1.IsDifferentFrom(value2);
        }

        public static bool IsLessThanOrCloseTo(this float value1, float value2)
        {
            return value1 < value2 || value1.IsCloseTo(value2);
        }
    }
}
