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

namespace Nine.Graphics.UI.Controls
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Internal.Controls;
    using Nine.Graphics.UI.Media;

    /// <summary>
    /// Represents a control that displays an image.
    /// </summary>
    public class Image : UIElement
    {
        public Texture2D Source { get; set; }
        public Stretch Stretch = Stretch.Fill;
        public StretchDirection StretchDirection { get; set; }

        public Image() { }
        public Image(Texture2D Source)
        {
            this.Source = Source;
        }

        protected internal override void OnRender(SpriteBatch spriteBatch)
        {
            base.OnRender(spriteBatch);
            if (this.Source != null)
            {
                spriteBatch.Draw(this.Source, AbsoluteRenderTransform, Color.White);
            }
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            return this.GetScaledImageSize(finalSize);
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            return this.GetScaledImageSize(availableSize);
        }

        private Vector2 GetScaledImageSize(Vector2 givenSize)
        {
            if (Source == null)
                return new Vector2();
            Vector2 contentSize = new Vector2(Source.Width, Source.Height);
            Vector2 scale = Viewbox.ComputeScaleFactor(givenSize, contentSize, this.Stretch, this.StretchDirection);
            return new Vector2(contentSize.X * scale.X, contentSize.Y * scale.Y);
        }
    }
}
