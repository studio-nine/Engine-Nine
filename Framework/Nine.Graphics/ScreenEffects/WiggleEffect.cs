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
    public partial class WiggleEffect : IUpdateable
    {
		private void OnCreated()
        {
            Repeat = 10;
            Amplitude = 10;
            Speed = 6;
        }

		private void OnClone(WiggleEffect cloneSource) { }

        private void OnApplyChanges()
        {
            pixelSize = new Vector2(1.0f / GraphicsDevice.Viewport.Width, 1.0f / GraphicsDevice.Viewport.Height);
        }

        void IUpdateable.Update(TimeSpan elapsedTime)
        {
            fTimer += (float)elapsedTime.TotalSeconds;
        }
    }	

#endif
}
