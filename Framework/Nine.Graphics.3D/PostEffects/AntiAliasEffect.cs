namespace Nine.Graphics.PostEffects
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Represents a antialias post processing effect using Fast Approximate AntiAliasing (FXAA).
    /// </summary>
    public class AntiAliasEffect : PostEffect
    {
        public AntiAliasEffect(GraphicsDevice graphics)
        {
            Order = 100;
            SurfaceFormat = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color;
            base.Material = new AntiAliasMaterial(graphics);
        }

        [Nine.Serialization.NotBinarySerializable]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Material Material
        {
            get { return base.Material; }
            set { base.Material = value; }
        }
    }
}