#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    public partial class DepthOfFieldEffect : IEffectTexture
    {
		private void OnClone(DepthOfFieldEffect cloneSource) { }
        private void OnApplyChanges() { }

        private void OnCreated()
        {
            FocalPlane = 0;
            FocalDistance = 0.5f;
        }

        public void SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.Blur)
                BlurTexture = texture as Texture2D;
            else if (name == TextureNames.DepthMap)
                DepthTexture = texture as Texture2D;
        }

        Texture2D IEffectTexture.Texture
        {
            get { return null; }
            set { }
        }
    }	

#endif
}
