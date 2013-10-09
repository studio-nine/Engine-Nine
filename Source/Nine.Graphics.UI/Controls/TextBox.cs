namespace Nine.Graphics.UI.Controls
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Nine.Graphics.UI.Media;

    // TODO: Marker only work on single Line

    [System.Windows.Markup.ContentProperty("Content")]
    public class TextBox : Control
    {
        #region Properties

        /// <summary>
        /// Gets or sets the text contents of the text box.
        /// </summary>
        public string Text
        {
            get { return textBlock.Text; }
            set 
            { 
                textBlock.Text = value; 
                MarkerLocation = value.Length; 
            }
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
        /// Gets or sets the Current Text Wrapping.
        /// </summary>
        public TextWrapping Wrapping
        {
            get { return textBlock.Wrapping; }
            set { textBlock.Wrapping = value; }
        }

        /// <summary>
        /// Gets or sets the Padding.
        /// </summary>
        public Thickness Padding
        {
            get { return textBlock.Padding; }
            set { textBlock.Padding = value; }
        }

        public bool EnableMarker { get; set; }

        public SolidColorBrush MarkerBrush { get; set; }

        /// <summary>
        /// The Char when the Marker Locations is in the middle of the text.
        /// </summary>
        public char MarkerChar1 { get; set; }

        /// <summary>
        /// The Char when the Marker Locations is at the end of the text.
        /// </summary>
        public char MarkerChar2 { get; set; }

        public int MarkerLocation
        {
            get { return markerLocation; }
            set { markerLocation = value; }
        }
        private int markerLocation = 0;

        public string Watermark { get; set; }
        public SolidColorBrush WatermarkBrush { get; set; }

        #endregion

        private TextBlock textBlock;

        public TextBox()
        {
            textBlock = new TextBlock() { Parent = this };
            EnableMarker = true;
            MarkerBrush = Color.Gray;
            MarkerChar1 = '|';
            MarkerChar2 = '_';
            Watermark = "";
            WatermarkBrush = Color.DarkGray;
        }

        public TextBox(SpriteFont font)
        {
            textBlock = new TextBlock() { Font = font, Parent = this };
            EnableMarker = true;
            MarkerBrush = Color.Gray;
            MarkerChar1 = '|';
            MarkerChar2 = '_';
            Watermark = "";
            WatermarkBrush = Color.DarkGray;
        }

        protected override void OnKeyDown(KeyboardEventArgs e)
        {
            Window window;
            if (EnableMarker && e.Key == Keys.Left && MarkerLocation > 0)
            {
                MarkerLocation--;
            }
            else if (EnableMarker && e.Key == Keys.Right && MarkerLocation < Text.Length)
            {
                MarkerLocation++;
            }
            else if (TryGetRootElement(out window))
            {
                // Rewrite some parts of this
                string text = Text;
                var prevMarker = markerLocation;
                if (AllowInput(window.Input.EditString(ref text, e.Key, Multiline, ref markerLocation, MaxChars)))
                {
                    textBlock.Text = text;
                }
                else
                {
                    markerLocation = prevMarker;
                }
            }
        }
        
        protected virtual bool AllowInput(Nine.TextChange change)
        {
            return true;
        }

        protected internal override void OnRender(Renderer.Renderer renderer)
        {
            if (Visible != Visibility.Visible) return;
            base.OnRender(renderer);

            // not the best way to display watermark text
            var wt = textBlock.Text;
            var wb = textBlock.Foreground;
            if (wt == string.Empty)
            {
                textBlock.Text = Watermark;
                textBlock.Foreground = WatermarkBrush;
            }
            textBlock.Text = wt;
            textBlock.Foreground = wb;
            textBlock.OnRender(renderer);

            if (EnableMarker && HasFocus)
            { 
                // TODO: Marker Flashing with Interval ofc
                if (MarkerLocation < 0 || MarkerLocation > Text.Length)
                    throw new IndexOutOfRangeException("MarkerLocation");

                var MarkerText = Text.Substring(0, MarkerLocation);
                var MarkerPosition = textBlock.TextStartPosition;
                // (-2), we want it to be closer to the character.
                MarkerPosition.X += Font.MeasureString(MarkerText).X - 2;

                var mChar = (MarkerLocation == Text.Length) ? MarkerChar2 : MarkerChar1;
                renderer.DrawString(Font, mChar.ToString(), MarkerPosition, (Color)MarkerBrush);
            }
        }
    }
}
