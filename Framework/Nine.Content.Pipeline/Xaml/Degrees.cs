namespace Nine.Content.Pipeline.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Defines a markup extension that converts from degrees to radians.
    /// </summary>
    public class Degrees : MarkupExtension
    {
        public object Value { get; set; }

        public Degrees(float degrees)
        {
            Value = MathHelper.ToRadians(degrees);
        }

        public Degrees(float v1, float v2)
        {
            Value = new Vector2(MathHelper.ToRadians(v1), MathHelper.ToRadians(v2));
        }
        
        public Degrees(float v1, float v2, float v3)
        {
            Value = new Vector3(MathHelper.ToRadians(v1), MathHelper.ToRadians(v2), MathHelper.ToRadians(v3));
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }
}
