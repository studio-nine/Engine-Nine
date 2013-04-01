namespace Nine.Graphics.UI.Media
{
    public abstract class TileBrush : Brush
    {
        /// <summary>
        /// Gets or sets how the image should be stretched.
        /// </summary>
        public Stretch Stretch
        {
            get { return stretch; }
            set { stretch = value; }
        }
        private Stretch stretch = Stretch.Fill;

        /// <summary>
        /// Gets or sets how the image is scaled.
        /// </summary>
        public StretchDirection StretchDirection
        {
            get { return stretchDirection; }
            set { stretchDirection = value; }
        }
        public StretchDirection stretchDirection = StretchDirection.Both;

        /// <summary>
        /// Gets or sets, if the Image should be flipped.
        /// </summary>
        public Flip Flip
        {
            get { return flip; }
            set { flip = value; }
        }
        private Flip flip = Flip.None;

        /* 
         - Should I add this?
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }

         */
    }
}
