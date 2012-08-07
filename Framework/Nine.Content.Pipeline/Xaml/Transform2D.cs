namespace Nine.Content.Pipeline.Xaml
{
    /*
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Nine.Content.Pipeline.Design;

    /// <summary>
    /// Defines a markup extension that constructs a 2D transform from scale,
    /// rotation and translation components.
    /// </summary>
    [MarkupExtensionReturnType(typeof(Matrix))]
    public class Transform2D : MarkupExtension, IEquatable<Transform2D>
    {
        /// <summary>
        /// Gets or sets scale on x, y axis.
        /// </summary>
        public Vector2 Scale
        {
            get { return scale; }
            set { scale = value; }
        }
        private Vector2 scale;

        /// <summary>
        /// Gets or sets scale on x axis.
        /// </summary>
        public float ScaleX
        {
            get { return scale.X; }
            set { scale.X = value; }
        }

        /// <summary>
        /// Gets or sets scale on y axis.
        /// </summary>
        public float ScaleY
        {
            get { return scale.Y; }
            set { scale.Y = value; }
        }

        /// <summary>
        /// Gets or sets rotation in radius.
        /// </summary>
        public float Rotation 
        {
            get { return rotation; }
            set { rotation = value; }
        }
        private float rotation;

        /// <summary>
        /// Gets or sets position on x, y axis.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        private Vector2 position;

        /// <summary>
        /// Gets or sets position on x axis.
        /// </summary>
        public float X
        {
            get { return position.X; }
            set { position.X = value; }
        }

        /// <summary>
        /// Gets or sets position on y axis.
        /// </summary>
        public float Y
        {
            get { return position.Y; }
            set { position.Y = value; }
        }

        /// <summary>
        /// Gets the identity transform.
        /// </summary>
        public static readonly Transform2D Identity = new Transform2D();

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform2D"/> class.
        /// </summary>
        public Transform2D()
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
            var value = new Matrix();
            value.M11 = Scale.X;
            value.M22 = Scale.Y;
            value.M22 = 1;
            value.M44 = 1;

            Matrix temp;
            Matrix.CreateRotationZ(-rotation, out temp);
            Matrix.Multiply(ref value, ref temp, out value);

            value.M41 = Position.X;
            value.M42 = Position.Y;
            return value;
        }
        
        public bool Equals(Transform2D other)
        {
            return Scale == other.Scale && Rotation == other.Rotation &&
                   RotationOrder == other.RotationOrder && Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if (obj is Transform2D)
                return Equals((Transform2D)obj);

            return false;
        }

        public static bool operator ==(Transform2D value1, Transform2D value2)
        {
            return ((value1.Scale == value2.Scale) && (value1.Rotation == value2.Rotation) &&
                    (value1.Position == value2.Position));
        }

        public static bool operator !=(Transform2D value1, Transform2D value2)
        {
            return !(value1.Scale == value2.Scale && value1.Rotation == value2.Rotation &&
                     value1.Position == value2.Position);
        }

        public override int GetHashCode()
        {
            return Scale.GetHashCode() ^ Rotation.GetHashCode() ^ Position.GetHashCode();
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
            builder.Append(Position.X);
            builder.Append(", ");
            builder.Append(Position.Y);
            return builder.ToString();
        }

        #region AttachedProperties
        /// <summary>
        /// Gets the scale of the target object
        /// </summary>
        public static Vector2 GetScale(object target)
        {
            Matrix transform;
            Vector2 scale;
            Vector2 translation;
            Quaternion rotation;

            return (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation)) ? scale : Vector2.One;
        }

        /// <summary>
        /// Sets the scale of the target object.
        /// </summary>
        public static void SetScale(object target, Vector2 value)
        {
            Matrix transform;
            Vector2 scale;
            Vector2 translation;
            Quaternion rotation;

            if (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation))
            {
                scale = value;
                TrySetTransform(target, scale, rotation, translation);
            }
        }

        /// <summary>
        /// Gets the position of the target object
        /// </summary>
        public static Vector2 GetPosition(object target)
        {
            Matrix transform;
            Vector2 scale;
            Vector2 translation;
            Quaternion rotation;

            return (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation)) ? translation : Vector2.One;
        }

        /// <summary>
        /// Sets the position of the target object.
        /// </summary>
        public static void SetPosition(object target, Vector2 value)
        {
            Matrix transform;
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            if (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation))
            {
                translation.X = value.X;
                translation.Y = value.Y;
                translation.Z = 0;
                TrySetTransform(target, scale, rotation.Z, translation);
            }
        }

        private static bool TryGetTransform(object target, out Matrix transform)
        {
            try
            {
                dynamic d = target;
                transform = d.Transform;
                return true;
            }
            catch
            {
                transform = Matrix.Identity;
                return false;
            }
        }

        private static bool TrySetTransform(object target, Vector3 scale, float rotation, Vector3 translation)
        {
            return TrySetTransform(target, Matrix.CreateScale(scale.X, scale.Y, 1) * 
                                           Matrix.CreateRotationZ(-rotation) * 
                                           Matrix.CreateTranslation(translation.X, translation.Y, 0));
        }

        private static bool TrySetTransform(object target, Matrix transform)
        {
            try
            {
                dynamic d = target;
                d.Transform = transform;
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
     */
}