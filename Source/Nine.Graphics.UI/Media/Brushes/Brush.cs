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

namespace Nine.Graphics.UI.Media
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Nine.Design;
    using Nine.Serialization;

    /// <summary>
    /// Represents a Brush used to paint to the screen. The type of Brush describes how the area is to be painted.
    /// </summary>
    [BinarySerializable]
    [TypeConverter(typeof(SolidColorBrushConverter))]
    public abstract class Brush
    {
        public float Alpha = 1;

        protected internal virtual void OnRender(Renderer.Renderer renderer, BoundingRectangle bound)
        {

        }

        public static implicit operator Brush(Color c)
        {
            return new SolidColorBrush(c.R, c.G, c.B, c.A);
        }

        public static implicit operator Brush(Microsoft.Xna.Framework.Graphics.Texture2D texture)
        {
            return new ImageBrush(texture);
        }
    }
}
