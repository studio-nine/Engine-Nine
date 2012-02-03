#region Using Statements

#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    /// <summary>
    /// A post processing screen effect that pixelate the whole screen.
    /// </summary>
    [ContentSerializable]
    public partial class PixelateEffect
    {
		private void OnCreated() { }
		private void OnClone(PixelateEffect cloneSource) { }
		private void OnApplyChanges() { }
    }	

#endif
}
