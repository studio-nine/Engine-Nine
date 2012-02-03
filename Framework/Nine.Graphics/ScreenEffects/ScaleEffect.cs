#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    [ContentSerializable]
    public partial class ScaleEffect
    {
		private void OnCreated() { }
        private void OnClone(ScaleEffect cloneSource) { }

        private void OnApplyChanges()
        {
            pixelSize = new Vector2(1.0f / GraphicsDevice.Viewport.Width, 1.0f / GraphicsDevice.Viewport.Height);
        }
    }	

#endif
}
