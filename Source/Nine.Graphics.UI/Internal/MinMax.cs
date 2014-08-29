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

    [System.Diagnostics.DebuggerDisplay("Max: {MaxHeight}, {MaxWidth} ; Min: {MinHeight}, {MinWidth}")]
    internal struct MinMax
    {
        internal readonly float MaxHeight;
        internal readonly float MaxWidth;
        internal readonly float MinHeight;
        internal readonly float MinWidth;

        internal MinMax(UIElement element)
        {
            float height = element.Height;
            float minHeight = element.MinHeight;
            float maxHeight = element.MaxHeight;
            this.MaxHeight = MathHelper.Clamp((float.IsNaN(height) ? float.PositiveInfinity : height), minHeight, maxHeight);
            this.MinHeight = MathHelper.Clamp((float.IsNaN(height) ? 0 : height), minHeight, maxHeight);

            float width = element.Width;
            float minWidth = element.MinWidth;
            float maxWidth = element.MaxWidth;
            this.MaxWidth = MathHelper.Clamp((float.IsNaN(width) ? float.PositiveInfinity : width), minWidth, maxWidth);
            this.MinWidth = MathHelper.Clamp((float.IsNaN(width) ? 0 : width), minWidth, maxWidth);
        }
    }
}
