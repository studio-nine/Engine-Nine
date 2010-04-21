#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion


namespace Isles.Graphics.ScreenEffects
{
    /// <summary>
    /// A post processing screen effect that transforms the color of the whole screen.
    /// </summary>
    public partial class ColorMatrixEffect
    {
        /// <summary>
        /// Creates a new instance of color matrix post processing.
        /// </summary>
        public ColorMatrixEffect(GraphicsDevice graphicsDevice) : this(graphicsDevice, null) { }

        /// <summary>
        /// Creates a new instance of color matrix post processing.
        /// </summary>
        public ColorMatrixEffect(GraphicsDevice graphicsDevice, EffectPool effectPool) :
            base(graphicsDevice, effectCode, CompilerOptions.None, effectPool)
        {
            InitializeComponent();
        }

        public static Matrix CreateColorMatrix(float brightness, float contrast, float saturation, float hue)
        {
            return CreateBrightness(brightness) *
                   CreateContrast(contrast) *
                   CreateHue(hue) *
                   CreateSaturation(saturation);
        }

        public static Matrix CreateBrightness(float amount)
        {
            return Matrix.CreateTranslation(amount, amount, amount);
        }

        public static Matrix CreateContrast(float amount)
        {
            return Matrix.CreateScale(amount);
        }

        public static Matrix CreateHue(float amount)
        {
            return Matrix.CreateFromAxisAngle(Vector3.One, amount * MathHelper.Pi * 2);
        }

        public static Matrix CreateSaturation(float amount)
        {
            float s = amount;

            float rwgt = 0.3086f;
            float gwgt = 0.6094f;
            float bwgt = 0.0820f;

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
