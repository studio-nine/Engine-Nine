#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    /// <summary>
    /// A post processing screen effect that pixelate the whole screen.
    /// </summary>
    public partial class PixelateEffect
    {
		private void OnCreated() { }
		private void OnClone(PixelateEffect cloneSource) { }
		private void OnApplyChanges() { }
    }	

#endif
}
