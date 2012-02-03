#region Using Statements
using System;

#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    [ContentSerializable]
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
