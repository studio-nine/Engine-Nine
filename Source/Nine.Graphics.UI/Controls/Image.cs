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
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Media;

    /// <summary>
    /// Represents a control that displays an image.
    /// </summary>
    public class Image : UIElement
    {
        /// <summary>
        /// Gets or sets the represented Image.
        /// </summary>
        public Texture2D Source { get; set; }

        /// <summary>
        /// Gets or sets how the image should be stretched.
        /// </summary>
        public Stretch Stretch { get; set; }

        /// <summary>
        /// Gets or sets how the image is scaled.
        /// </summary>
        public StretchDirection StretchDirection { get; set; }

        /// <summary>
        /// Gets or sets if the Image should be flipped.
        /// </summary>
        public Flip Flip { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Image">Image</see> with <see cref="Image.Source">Source</see> empty.
        /// </summary>
        public Image() : this(null) { }

        /// <summary>
        /// Initializes a new instance of <see cref="Image">Image</see>.
        /// </summary>
        /// <param name="Source">Image</param>
        public Image(Texture2D Source)
        {
            if (Source != null)
                this.Source = Source;

            Stretch = Stretch.Fill;
            Flip = Media.Flip.None;
        }

        protected internal override void OnRender(Nine.Graphics.UI.Renderer.Renderer renderer)
        {
            if (Visible != Visibility.Visible) return;
            base.OnRender(renderer);
            if (this.Source != null)
            {
                renderer.Draw(Source, AbsoluteRenderTransform, null, Color.White, Flip);
            }
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            if (Visible == Visibility.Collapsed) return Vector2.Zero;
            return this.GetScaledImageSize(finalSize);
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            if (Visible == Visibility.Collapsed) return Vector2.Zero;
            return this.GetScaledImageSize(availableSize);
        }

        private Vector2 GetScaledImageSize(Vector2 givenSize)
        {
            if (Source == null)
                return new Vector2();
            Vector2 contentSize = new Vector2(Source.Width, Source.Height);
            Vector2 scale = Nine.Graphics.UI.Internal.Controls.Viewbox.ComputeScaleFactor(givenSize, contentSize, this.Stretch, this.StretchDirection);
            return new Vector2(contentSize.X * scale.X, contentSize.Y * scale.Y);
        }
    }
}
