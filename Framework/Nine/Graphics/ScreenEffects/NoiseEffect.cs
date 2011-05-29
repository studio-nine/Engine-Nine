#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    public partial class NoiseEffect : IUpdateable
    {
        private void OnCreated() { NoiseAmount = 1; }
		private void OnClone(NoiseEffect cloneSource) { }
		private void OnApplyChanges() { }

        void IUpdateable.Update(TimeSpan elapsedTime)
        {
            timer += (float)elapsedTime.TotalSeconds;
        }
    }

#endif
}
