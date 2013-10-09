namespace Nine.Graphics.UI.Controls
{
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Base of a Range Slider.
    /// </summary>
    public abstract class RangeBase : Control
    {
        /// <summary>
        /// Gets or sets the Maximum Value.
        /// </summary>
        public float Maximum 
        {
            get { return maximum; }
            set
            {
                if (Minimum > maximum)
                    throw new System.ArgumentOutOfRangeException("value");
                maximum = value;
            }
        }
        private float maximum = 100;

        /// <summary>
        /// Gets or sets the Minimum Value
        /// </summary>
        public float Minimum 
        {
            get { return minimum; }
            set
            {
                if (minimum > Maximum)
                    throw new System.ArgumentOutOfRangeException("value");
                minimum = value;
            }
        }
        private float minimum = 0;

        /// <summary>
        /// Gets or sets the largets value change.
        /// </summary>
        public float LargeChange { get; set; }

        /// <summary>
        /// Gets or sets the smallest valye change.
        /// </summary>
        public float SmallChange { get; set; }

        /// <summary>
        /// Gets or sets the current Value
        /// </summary>
        public float Value
        {
            get { return value; }
            set
            {
                this.value = MathHelper.Clamp(value, Minimum, Maximum);
            }
        }
        private float value = 0;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RangeBase()
        {
            this.LargeChange = 10;
            this.SmallChange = 5;
        }
    }
}
