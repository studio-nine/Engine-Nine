namespace Nine.Graphics.UI.Controls
{
    public abstract class RangeBase : Control
    {
        public float Maximum { get; set; }
        public float Minimum { get; set; }
        public float LargeChange { get; set; }
        public float SmallChange { get; set; }
        public float Value { get; set; }

        protected RangeBase()
        {
            this.Maximum = 100;
            this.Minimum = 0;
            this.LargeChange = 10;
            this.SmallChange = 5;
            this.Value = 0;
        }
    }
}
