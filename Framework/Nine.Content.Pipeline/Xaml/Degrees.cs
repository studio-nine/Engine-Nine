namespace Nine.Content.Pipeline.Xaml
{
    using System;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Defines a markup extension that converts from degrees to radians.
    /// </summary>
    public class Degrees : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the degrees.
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Degrees"/> class.
        /// </summary>
        public Degrees() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Degrees"/> class.
        /// </summary>
        public Degrees(float degrees) 
        {
            Value = degrees;
        }

        /// <summary>
        /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return MathHelper.ToRadians(Value);
        }
    }
}
