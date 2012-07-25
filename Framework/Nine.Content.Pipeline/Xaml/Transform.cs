namespace Nine.Content.Pipeline.Xaml
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Nine.Content.Pipeline.Design;

    /// <summary>
    /// Defines a markup extension that constructs a 3D transform from scale,
    /// rotation and translation components.
    /// </summary>
    [MarkupExtensionReturnType(typeof(Matrix))]
    [TypeConverter(typeof(TransformConverter))]
    public class Transform : MarkupExtension, IEquatable<Transform>
    {
        /// <summary>
        /// Gets or sets scale on x, y, z axis.
        /// </summary>
        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }
        private Vector3 scale;

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
        /// Gets or sets scale on z axis.
        /// </summary>
        public float ScaleZ
        {
            get { return scale.Z; }
            set { scale.Z = value; }
        }
        
        /// <summary>
        /// Gets or sets rotation on x, y, z axis.
        /// </summary>
        public Vector3 Rotation 
        {
            get { return rotation; }
            set { rotation = value; }
        }
        private Vector3 rotation;

        /// <summary>
        /// Gets or sets rotation on x axis.
        /// </summary>
        public float RotationX
        {
            get { return rotation.X; }
            set { rotation.X = value; }
        }

        /// <summary>
        /// Gets or sets rotation on y axis.
        /// </summary>
        public float RotationY
        {
            get { return rotation.Y; }
            set { rotation.Y = value; }
        }

        /// <summary>
        /// Gets or sets rotation on z axis.
        /// </summary>
        public float RotationZ
        {
            get { return rotation.Z; }
            set { rotation.Z = value; }
        }

        /// <summary>
        /// Gets or sets the order of rotation.
        /// </summary>
        public RotationOrder RotationOrder { get; set; }

        /// <summary>
        /// Gets or sets position on x, y, z axis.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        private Vector3 position;

        /// <summary>
        /// Gets or sets position on x axis.
        /// </summary>
        public float PositionX
        {
            get { return position.X; }
            set { position.X = value; }
        }

        /// <summary>
        /// Gets or sets position on y axis.
        /// </summary>
        public float PositionY
        {
            get { return position.Y; }
            set { position.Y = value; }
        }

        /// <summary>
        /// Gets or sets position on z axis.
        /// </summary>
        public float PositionZ
        {
            get { return position.Z; }
            set { position.Z = value; }
        }

        /// <summary>
        /// Gets the identity transform.
        /// </summary>
        public static readonly Transform Identity = new Transform();

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> class.
        /// </summary>
        public Transform()
        {
            Scale = Vector3.One;
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
            value.M33 = Scale.Z;
            value.M44 = 1;

            Matrix temp = MatrixHelper.CreateRotation(Rotation, RotationOrder);
            Matrix.Multiply(ref value, ref temp, out value);

            value.M41 = Position.X;
            value.M42 = Position.Y;
            value.M43 = Position.Z;
            return value;
        }
        
        public bool Equals(Transform other)
        {
            return Scale == other.Scale && Rotation == other.Rotation &&
                   RotationOrder == other.RotationOrder && Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if (obj is Transform)
                return Equals((Transform)obj);

            return false;
        }

        public static bool operator ==(Transform value1, Transform value2)
        {
            return ((value1.Scale == value2.Scale) && (value1.Rotation == value2.Rotation) &&
                    (value1.Position == value2.Position) && (value1.RotationOrder == value2.RotationOrder));
        }

        public static bool operator !=(Transform value1, Transform value2)
        {
            return !(value1.Scale == value2.Scale && value1.Rotation == value2.Rotation &&
                     value1.Position == value2.Position && value1.RotationOrder == value2.RotationOrder);
        }

        public override int GetHashCode()
        {
            return Scale.GetHashCode() ^ Rotation.GetHashCode() ^ Position.GetHashCode() ^ RotationOrder.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Scale.X);
            builder.Append(", ");
            builder.Append(Scale.Y);
            builder.Append(", ");
            builder.Append(Scale.Z);
            builder.Append(";");
            builder.Append(Rotation.X);
            builder.Append(", ");
            builder.Append(Rotation.Y);
            builder.Append(", ");
            builder.Append(Rotation.Z);
            builder.Append(";");
            builder.Append(Position.X);
            builder.Append(", ");
            builder.Append(Position.Y);
            builder.Append(", ");
            builder.Append(Position.Z);
            builder.Append(";");
            builder.Append(RotationOrder.ToString());
            return builder.ToString();
        }

        #region AttachedProperties
        /// <summary>
        /// Gets the scale of the target object
        /// </summary>
        public static Vector3 GetScale(object target)
        {
            Matrix transform;
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            return (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation)) ? scale : Vector3.One;
        }

        /// <summary>
        /// Sets the scale of the target object.
        /// </summary>
        public static void SetScale(object target, Vector3 value)
        {
            Matrix transform;
            Vector3 scale;
            Vector3 translation;
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
        public static Vector3 GetPosition(object target)
        {
            Matrix transform;
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            return (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation)) ? translation : Vector3.One;
        }

        /// <summary>
        /// Sets the position of the target object.
        /// </summary>
        public static void SetPosition(object target, Vector3 value)
        {
            Matrix transform;
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            if (TryGetTransform(target, out transform) && transform.Decompose(out scale, out rotation, out translation))
            {
                translation = value;
                TrySetTransform(target, scale, rotation, translation);
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

        private static bool TrySetTransform(object target, Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            return TrySetTransform(target, Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(translation));
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
}