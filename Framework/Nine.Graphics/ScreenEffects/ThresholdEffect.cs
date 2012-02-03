#region Using Statements

#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    /// <summary>
    /// A post processing screen effect that only show pixels brighter than the threshold.
    /// </summary>
    [ContentSerializable]
    public partial class ThresholdEffect
    {
        private void OnCreated() { }
        private void OnClone(ThresholdEffect cloneSource) { }
        private void OnApplyChanges() { }
    }

#endif
}
