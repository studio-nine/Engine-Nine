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
    using System;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Internal;

    /// <summary>
    /// Represents a <see cref="Brush">Brush</see> of the specified <see cref="Vector3">Vector3</see> 
    /// which can be used to paint an area with a solid color.
    /// </summary>
    [ContentProperty("Color")]
    public sealed class SolidColorBrush : Brush
    {
        /// <summary>
        /// The Color of the SolidColorBrush.
        /// </summary>
        public Vector3 Color { get; set; }

        // Should I store this in color instead?
        public Color Test
        {
            get 
            { 
                return new Color(Color.X, Color.Y, Color.Z, Alpha); 
            }
            set
            {
                Color = value.ToVector3();
                Alpha = value.A;
            }
        }

        #region ctors

        public SolidColorBrush() 
        { 

        }

        public SolidColorBrush(int r, int g, int b) : this(r, g, b, 1) { }
        public SolidColorBrush(float r, float g, float b) : this(r, g, b, 1) { }
        public SolidColorBrush(float r, float g, float b, float a) : this(new Color(r, g, b, a)) { }
        public SolidColorBrush(int r, int g, int b, int a) : this(new Color(r, g, b, a)) { }

        public SolidColorBrush(Color color)
        {
            this.Color = color.ToVector3();
            Alpha = color.A;
        }

        public SolidColorBrush(Vector3 color)
        {
            this.Color = color;
        }

        #endregion

        public static implicit operator SolidColorBrush(Color c)
        {
            return new SolidColorBrush(c.R, c.G, c.B, c.A);
        }

        public static explicit operator Color(SolidColorBrush c)
        {
            return new Color(c.Color.X, c.Color.Y, c.Color.Z, c.Alpha);
        }

        protected internal override void OnRender(Renderer.Renderer renderer, BoundingRectangle bound)
        {
            renderer.Draw(bound, (Color)this);
        }

        public override string ToString()
        {
            return string.Format("[ R: {0}, G: {2}, B: {3}, A: {4} ]", Color.X, Color.Y, Color.Z, Alpha);
        }
    }
}
