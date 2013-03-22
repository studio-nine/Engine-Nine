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
    using Microsoft.Xna.Framework;
    using System;

    /// <summary>
    ///     Represents a <see cref = "Brush">Brush</see> of the specified <see cref = "Color">Color</see> which can be used to paint an area with a solid color.
    /// </summary>
    public class SolidColorBrush : Brush //, IConvertible
    {
        /// <summary>
        ///     The Color of the SolidColorBrush.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "SolidColorBrush">SolidColorBrush</see> class.
        /// </summary>
        /// <param name = "color">The <see cref = "Color">Color</see> with which to create this <see cref = "SolidColorBrush">SolidColorBrush</see>.</param>
        public SolidColorBrush(Color color)
        {
            this.Color = color;
        }

        public SolidColorBrush(string Hash)
        { // Not sure if this is going to be here
            var Number = int.Parse(Hash);
            this.Color = Number.ConvertToColor();
        }

        public override string ToString()
        {
            return this.Color.ToString();
        }
    }
}
