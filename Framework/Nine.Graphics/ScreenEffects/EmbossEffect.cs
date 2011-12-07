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
    public partial class EmbossEffect
    {
		private void OnCreated() 
        {
            Emboss = 1;
        }
		private void OnClone(EmbossEffect cloneSource) { }
		private void OnApplyChanges()
        {
            pixelSize = new Vector2(1.0f / GraphicsDevice.Viewport.Width, 1.0f / GraphicsDevice.Viewport.Height);
        }
    }	

#endif
}
