namespace Nine.Graphics
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Helper class to create texture transformation matrices.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TextureTransform
    {
        /// <summary>
        /// Creates a texture transform matrix from translation.
        /// </summary>
        public static Matrix CreateTranslation(float x, float y)
        {
            return Matrix.CreateTranslation(-x, -y, 0);
        }

        /// <summary>
        /// Creates a texture transform matrix from scale.
        /// </summary>
        public static Matrix CreateScale(float x, float y)
        {
            return Matrix.CreateScale(1 / x, 1 / y, 1);
        }

        /// <summary>
        /// Creates a texture transform matrix from rotation.
        /// </summary>
        public static Matrix CreateRotation(float radius)
        {
            return Matrix.CreateRotationZ(-radius);
        }

        /// <summary>
        /// Creates a texture transform matrix from a source rectangle.
        /// </summary>
        public static Matrix CreateFromSourceRectange(Texture2D texture, Rectangle? rectangle)
        {
            if (rectangle == null || texture == null)
                return Matrix.Identity;

            float scaleX = 1.0f * rectangle.Value.Width / texture.Width;
            float scaleY = 1.0f * rectangle.Value.Height / texture.Height;
            float translationX = 1.0f * rectangle.Value.Left / texture.Width;
            float translationY = 1.0f * rectangle.Value.Top / texture.Height;
            
            Matrix result = new Matrix();
            result.M11 = scaleX;
            result.M22 = scaleY;
            result.M33 = 1;
            result.M44 = 1;
            result.M41 = translationX;
            result.M42 = translationY;

            return result;
        }

        /// <summary>
        /// Transforms a texture coordinate based on the texture transform.
        /// </summary>
        public static Vector2 Transform(Matrix matrix, Vector2 uv)
        {
            Vector3 v3 = new Vector3(uv, 0);
            Vector3.Transform(ref v3, ref matrix, out v3);
            return new Vector2(v3.X, v3.Y);
        }

        /// <summary>
        /// Returns a 3x3 array representation of the texture transform.
        /// </summary>
        public static float[] ToArray(Matrix matrix)
        {
            FloatArray[0] = matrix.M11;
            FloatArray[1] = matrix.M12;
            FloatArray[2] = matrix.M14;

            FloatArray[3] = matrix.M21;
            FloatArray[4] = matrix.M22;
            FloatArray[5] = matrix.M24;

            FloatArray[6] = matrix.M41;
            FloatArray[7] = matrix.M42;
            FloatArray[8] = matrix.M44;

            return FloatArray;
        }
        static float[] FloatArray = new float[9];
    }
}
