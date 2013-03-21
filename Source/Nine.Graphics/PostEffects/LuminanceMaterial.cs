namespace Nine.Graphics.Materials
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [Nine.Serialization.NotBinarySerializable]
    partial class LuminanceMaterial
    {
        public bool IsDownScale;

        partial void BeginApplyLocalParameters(DrawingContext context, LuminanceMaterial previousMaterial)
        {
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            if ((GraphicsDevice.Textures[0] = texture) != null)
            {
                var halfTexel = new Vector2();
                halfTexel.X = 0.5f / texture.Width;
                halfTexel.Y = 0.5f / texture.Height;
                effect.HalfTexel.SetValue(halfTexel);
            }
            effect.CurrentTechnique = effect.Techniques[IsDownScale ? 1 : 0];
        }
    }
}