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
    using Microsoft.Xna.Framework;

    internal static class ThicknessExtensions
    {
        public static Vector2 Collapse(this Thickness thickness)
        {
            return new Vector2(thickness.Left + thickness.Right, thickness.Top + thickness.Bottom);
        }

        public static bool IsDifferentFrom(this Thickness thickness1, Thickness thickness2)
        {
            return thickness1.Left.IsDifferentFrom(thickness2.Left) ||
                   thickness1.Right.IsDifferentFrom(thickness2.Right) || thickness1.Top.IsDifferentFrom(thickness2.Top) ||
                   thickness1.Bottom.IsDifferentFrom(thickness2.Bottom);
        }
    }
}
