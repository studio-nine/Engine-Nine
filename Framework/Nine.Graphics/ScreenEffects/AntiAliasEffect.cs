#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    public partial class AntiAliasEffect : IEffectTexture
    {
		private void OnCreated() 
        {
            Weight = 0.5f;
        }

        private void OnClone(AntiAliasEffect cloneSource) { }

		private void OnApplyChanges() 
        {
            pixelSize = new Vector2(1.0f / GraphicsDevice.Viewport.Width, 1.0f / GraphicsDevice.Viewport.Height);
        }

        public Texture2D Texture { get; set; }

        public void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.NormalMap)
                NormalTexture = texture as Texture2D;
        }
    }	

#endif
}
