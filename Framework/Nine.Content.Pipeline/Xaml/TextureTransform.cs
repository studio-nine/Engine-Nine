namespace Nine.Content.Pipeline.Xaml
{
    using System;
    using System.Text;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;

    public class TextureTransform : MarkupExtension, IEquatable<TextureTransform>
    {
        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        public Vector2 Scale { get; set; }
        
        /// <summary>
        /// Gets or sets rotation on x, y, z axis.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the translation
        /// </summary>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// Gets the identity transform.
        /// </summary>
        public static readonly TextureTransform Identity = new TextureTransform();

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureTransform"/> class.
        /// </summary>
        public TextureTransform()
        {
            Scale = Vector2.One;
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
            return ToMatrix();
        }

        /// <summary>
        /// Checks whether the current Triangle intersects a Ray.
        /// </summary>
        public Matrix ToMatrix()
        {
            return Nine.Graphics.TextureTransform.CreateScale(Scale.X, Scale.Y) *
                   Nine.Graphics.TextureTransform.CreateRotation(Rotation) *
                   Nine.Graphics.TextureTransform.CreateTranslation(Offset.X, Offset.Y);
        }

        public bool Equals(TextureTransform other)
        {
            return Scale == other.Scale && Rotation == other.Rotation && Offset == other.Offset;
        }

        public override bool Equals(object obj)
        {
            if (obj is TextureTransform)
                return Equals((TextureTransform)obj);
            return false;
        }

        public static bool operator ==(TextureTransform value1, TextureTransform value2)
        {
            return ((value1.Scale == value2.Scale) && (value1.Rotation == value2.Rotation) && (value1.Offset == value2.Offset));
        }

        public static bool operator !=(TextureTransform value1, TextureTransform value2)
        {
            return !(value1.Scale == value2.Scale && value1.Rotation == value2.Rotation && value1.Offset == value2.Offset);
        }

        public override int GetHashCode()
        {
            return Scale.GetHashCode() ^ Rotation.GetHashCode() ^ Offset.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Scale.X);
            builder.Append(", ");
            builder.Append(Scale.Y);
            builder.Append(";");
            builder.Append(Rotation);
            builder.Append(";");
            builder.Append(Offset.X);
            builder.Append(", ");
            builder.Append(Offset.Y);
            return builder.ToString();
        }

        #region AttachedProperties
        /// <summary>
        /// Gets the scale of the target object
        /// </summary>
        public static Vector2 GetScale(object target)
        {
            Matrix transform;
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            return (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation)) ? new Vector2(1 / scale.X, 1 / scale.Y) : Vector2.One;
        }

        /// <summary>
        /// Sets the scale of the target object.
        /// </summary>
        public static void SetScale(object target, Vector2 value)
        {
            Matrix transform;
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            if (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation))
            {
                scale.X = 1 / value.Y;
                scale.Y = 1 / value.Y;
                TrySetTransform(target, scale, rotation, translation);
            }
        }

        /// <summary>
        /// Gets the position of the target object
        /// </summary>
        public static Vector2 GetOffset(object target)
        {
            Matrix transform;
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            return (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation)) ? new Vector2(-translation.X, -translation.Y) : Vector2.One;
        }

        /// <summary>
        /// Sets the position of the target object.
        /// </summary>
        public static void SetOffset(object target, Vector2 value)
        {
            Matrix transform;
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            if (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation))
            {
                translation.X = -value.X;
                translation.Y = -value.Y;
                TrySetTransform(target, scale, rotation, translation);
            }
        }

        private static bool TryGetTransform(object target, out Matrix transform)
        {
            try
            {
                dynamic d = target;
                transform = d.TextureTransform;
                return true;
            }
            catch
            {
                transform = Matrix.Identity;
                return false;
            }
        }

        private static bool TrySetTransform(object target, Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            return TrySetTransform(target, Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(translation));
        }

        private static bool TrySetTransform(object target, Matrix transform)
        {
            try
            {
                dynamic d = target;
                d.TextureTransform = transform;
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}