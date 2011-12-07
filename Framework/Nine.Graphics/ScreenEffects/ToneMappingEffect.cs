#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    [ContentSerializable]
    public partial class ToneMappingEffect : IEffectTexture
    {
		private void OnClone(ToneMappingEffect cloneSource) { }
		private void OnApplyChanges() { }

		private void OnCreated() 
        {
            Exposure = 0.6f;
            MaxLuminance = 16;
        }

        public void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Luminance)
                LuminanceTexture = texture as Texture2D;
            else if (usage == TextureUsage.Bloom)
                BloomTexture = texture as Texture2D;
        }

        Texture2D IEffectTexture.Texture
        {
            get { return null; }
            set { }
        }
    }	

#endif
}
