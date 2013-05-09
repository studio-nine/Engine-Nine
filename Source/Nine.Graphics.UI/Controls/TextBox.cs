namespace Nine.Graphics.UI.Controls
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Media;

    [System.Windows.Markup.ContentProperty("Content")]
    public class TextBox : Control
    {
        /// <summary>
        /// Gets or sets the text contents of the text box.
        /// </summary>
        public string Text
        {
            get { return textBlock.Text; }
            set { textBlock.Text = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of characters that can be manually entered
        //  into the text box.
        /// </summary>
        public int MaxChars
        {
            get { return maxChars; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("MaxChars must be higher then Zero.");
                maxChars = value;
            }
        }
        private int maxChars = int.MaxValue;

        /// <summary>
        /// Allow Multiline.
        /// </summary>
        public bool Multiline { get; set; }

        /// <summary>
        /// Get or sets the current font.
        /// </summary>
        public SpriteFont Font
        {
            get { return textBlock.Font; }
            set { textBlock.Font = value; }
        }

        /// <summary>
        /// Gets or sets the Font Color.
        /// </summary>
        public SolidColorBrush Foreground
        {
            get { return textBlock.Foreground; }
            set { textBlock.Foreground = value; }
        }

        /// <summary>
        /// Gets or sets the Padding.
        /// </summary>
        public Thickness Padding
        {
            get { return textBlock.Padding; }
            set { textBlock.Padding = value; }
        }

        /// <summary>
        /// Gets or sets the Current Text Wrapping.
        /// </summary>
        public TextWrapping Wrapping
        {
            get { return textBlock.Wrapping; }
            set { textBlock.Wrapping = value; }
        }

        private TextBlock textBlock;

        public TextBox()
        {
            textBlock = new TextBlock() { Parent = this };
        }

        protected override void OnKeyDown(KeyboardEventArgs e)
        {
            Window window;
            if (TryGetRootElement(out window))
            {
                string text = Text;
                window.Input.EditString(ref text, e.Key, Multiline, text.Length, MaxChars);
                Text = text;
            }
        }

        protected internal override void OnRender(Renderer.IRenderer renderer)
        {
            textBlock.OnRender(renderer);
            base.OnRender(renderer);
        }
    }
}
