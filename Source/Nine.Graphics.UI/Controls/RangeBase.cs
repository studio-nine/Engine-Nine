namespace Nine.Graphics.UI.Controls
{
    using Microsoft.Xna.Framework;

    public abstract class RangeBase : Control
    {
        /// <summary>
        /// The Maximum Value
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
        /// The Minimum Value
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

        public float LargeChange 
        { 
            get; 
            set; 
        }

        public float SmallChange 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Current Value
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

        protected RangeBase()
        {
            this.LargeChange = 10;
            this.SmallChange = 5;
        }
    }
}
