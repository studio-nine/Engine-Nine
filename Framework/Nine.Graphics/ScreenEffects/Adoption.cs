#region Using Statements
using System;

#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    internal partial class Adoption : IUpdateable
    {
        public bool HighQuality
        {
            get { return shaderIndex == 1; }
            set { shaderIndex = value ? 1 : 0; }
        }

		private void OnClone(Adoption cloneSource) { }

		private void OnCreated() 
        {
            Speed = 1;
            HighQuality = true;
        }

		private void OnApplyChanges() { }
        
        public void Update(TimeSpan elapsedTime)
        {
            deltaTime = (float)elapsedTime.TotalSeconds;
        }
    }

#endif
}
