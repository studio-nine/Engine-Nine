namespace Nine.Graphics
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Helper class to create color matrices.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ColorMatrix
    {
        /// <summary>
        /// Transforms RGB colours in YIQ space.
        /// </summary>
        static Matrix YiqTransform = new Matrix(0.299f, 0.587f, 0.114f, 0.000f,
                                                0.596f, -.274f, -.321f, 0.000f,
                                                0.211f, -.523f, 0.311f, 0.000f,
                                                0.000f, 0.000f, 0.000f, 1.000f);
        /// <summary>
        /// Transforms YIQ colours in RGB space.
        /// </summary>
        static Matrix RgbTransform = Matrix.Invert(YiqTransform);

        public static Matrix Create(float brightness, float contrast, float saturation, float hue)
        {
            return CreateBrightness(brightness) *
                   CreateContrast(contrast) *
                   CreateHue(hue) *
                   CreateSaturation(saturation);
        }

        public static Matrix CreateBrightness(float amount)
        {
            return YiqTransform * Matrix.CreateTranslation(amount, 0, 0) * RgbTransform;
        }

        public static Matrix CreateContrast(float amount)
        {
            return Matrix.CreateScale(amount);
        }

        public static Matrix CreateHue(float amount)
        {
            return YiqTransform * Matrix.CreateRotationX(amount * MathHelper.Pi * 2) * RgbTransform;
        }

        public static Matrix CreateSaturation(float amount)
        {
            float s = amount;

            float rwgt = 0.299f;
            float gwgt = 0.587f;
            float bwgt = 0.114f;

            float a = (1.0f - s) * rwgt + s;
            float b = (1.0f - s) * rwgt;
            float c = (1.0f - s) * rwgt;
            float d = (1.0f - s) * gwgt;
            float e = (1.0f - s) * gwgt + s;
            float f = (1.0f - s) * gwgt;
            float g = (1.0f - s) * bwgt;
            float h = (1.0f - s) * bwgt;
            float i = (1.0f - s) * bwgt + s;

            return new Matrix
            (
                a, b, c, 0.0f,
                d, e, f, 0.0f,
                g, h, i, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );
        }

        public static Matrix CreateGrayscale()
        {
            return new Matrix
            (
                0.299f, 0.299f, 0.299f, 0,
                0.587f, 0.587f, 0.587f, 0,
                0.114f, 0.114f, 0.114f, 0,
                0, 0, 0, 1
            );
        }
        
        public static Matrix CreateNegative()
        {
            return new Matrix
            (
                -1, 0, 0, 0,
                0, -1, 0, 0,
                0, 0, -1, 0,
                1, 1, 1, 1
            );
        }
    }
}
