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

namespace Nine.Graphics.UI.Media
{
    using System;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Represents a <see cref="Brush">Brush</see> of the specified <see cref="Vector4">Vector4</see> which can be used to paint an area with a solid color.
    /// </summary>
    [ContentProperty("Color")]
    public class SolidColorBrush : Brush
    {
        /// <summary>
        /// The Color of the SolidColorBrush.
        /// </summary>
        public Vector4 Color { get; set; }

        public SolidColorBrush() { }
        public SolidColorBrush(Color color)
        {
            this.Color = color.ToVector4();
        }
        public SolidColorBrush(Vector4 color)
        {
            this.Color = color;
        }

        public Color ToColor()
        {
            return new Color(Color.X, Color.Y, Color.Z, Color.W);
        }

        public override string ToString()
        {
            return this.Color.ToString();
        }
    }
}
