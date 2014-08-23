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
    using Nine.Graphics.UI.Controls.Primitives;
    using Nine.Graphics.UI.Media;

    // TODO: A button shall have Content instead of Text

    /// <summary>
    /// A Control that represent a clickable button.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Text")]
    public class Button : Control
    {
        #region Properties

        /// <summary>
        /// Gets or sets the text contents of the button.
        /// </summary>
        public string Text
        {
            get { return textblock.Text; }
            set { textblock.Text = value; }
        }

        /// <summary>
        /// Get or sets the current font.
        /// </summary>
        public SpriteFont Font
        {
            get { return textblock.Font; }
            set { textblock.Font = value; }
        }

        /// <summary>
        /// Gets or sets the Font Color.
        /// </summary>
        public SolidColorBrush Foreground
        {
            get { return textblock.Foreground; }
            set { textblock.Foreground = value; }
        }

        /// <summary>
        /// Gets or sets the Padding.
        /// </summary>
        public Thickness Padding
        {
            get { return textblock.Padding; }
            set { textblock.Padding = value; }
        }

        /// <summary>
        /// Gets or sets the Current Text Wrapping.
        /// </summary>
        public TextWrapping Wrapping
        {
            get { return textblock.Wrapping; }
            set { textblock.Wrapping = value; }
        }

        /// <summary>
        /// Inner Text Horizontal Alignment
        /// </summary>
        public HorizontalAlignment TextHorizontalAlignment
        {
            get { return textblock.HorizontalAlignment; }
            set { textblock.HorizontalAlignment = value; ; }
        }

        /// <summary>
        /// Inner Text Vertical Alignment
        /// </summary>
        public VerticalAlignment TextVerticalAlignment
        {
            get { return textblock.VerticalAlignment; }
            set { textblock.VerticalAlignment = value;; }
        }

        private TextBlock textblock
        {
            get { return InnerBrush.Visual as TextBlock; }
            set { InnerBrush.Visual = value; }
        }
        private VisualBrush InnerBrush;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new Button without a Font.
        /// Font has to be set before it can render!
        /// </summary>
        public Button()
        {
            InnerBrush = new VisualBrush();
            textblock = new TextBlock();
            textblock.HorizontalAlignment = UI.HorizontalAlignment.Center;
            textblock.VerticalAlignment = UI.VerticalAlignment.Center;

            this.Height = 32;
        }

        /// <summary>
        /// Constructs a new button with Font.
        /// </summary>
        /// <param name="font">Text Font</param>
        public Button(SpriteFont font)
            : this(font, string.Empty)
        {

        }

        /// <summary>
        /// Constructs a new button with Font.
        /// </summary>
        /// <param name="font">Text Font</param>
        /// <param name="text"></param>
        public Button(SpriteFont font, string text)
        {
            InnerBrush = new VisualBrush();
            textblock = new TextBlock(font, text);
            textblock.HorizontalAlignment = UI.HorizontalAlignment.Center;
            textblock.VerticalAlignment = UI.VerticalAlignment.Center;

            this.Height = 32;
        }

        #endregion

        #region Methods

        protected override void OnRender(Renderer.Renderer renderer)
        {
            renderer.Draw(AbsoluteRenderTransform, InnerBrush);
        }

        #endregion
    }
}
