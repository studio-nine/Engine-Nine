namespace Nine.Graphics.UI.Controls
{
    using Nine.AttachedProperty;
    using Nine.Graphics.UI.Controls.Primitives;
    using System;

#if !PCL
    using System.Xaml;
#endif

    public static class ToolTipService
    {
        static readonly AttachableMemberIdentifier IsEnabledMember = new AttachableMemberIdentifier(typeof(bool), "IsEnabled");
        static readonly AttachableMemberIdentifier ShowOnDisabledMember = new AttachableMemberIdentifier(typeof(bool), "ShowOnDisabled");
        static readonly AttachableMemberIdentifier PlacementMember = new AttachableMemberIdentifier(typeof(PlacementMode), "Placement");
        static readonly AttachableMemberIdentifier ShowDurationMember = new AttachableMemberIdentifier(typeof(float), "ShowDuration");
        static readonly AttachableMemberIdentifier HorizontalOffsetMember = new AttachableMemberIdentifier(typeof(float), "HorizontalOffset");
        static readonly AttachableMemberIdentifier VerticalOffsetMember = new AttachableMemberIdentifier(typeof(float), "VerticalOffset");
        static readonly AttachableMemberIdentifier ToolTipMember = new AttachableMemberIdentifier(typeof(UIElement), "ToolTip");


        // public static void SetBetweenShowDelay(UIElement element, int value);
        // public static void SetInitialShowDelay(UIElement element, int value);
        

        #region IsEnabled
        public static bool GetIsEnabled(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (element.AttachedProperties.ContainsKey(IsEnabledMember))
                return (bool)element.AttachedProperties[IsEnabledMember];
            else
                return true;
        }
        public static void SetIsEnabled(UIElement element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.AttachedProperties[IsEnabledMember] = value;
        }
        #endregion

        #region ShowOnDisabled
        public static bool GetShowOnDisabled(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (element.AttachedProperties.ContainsKey(ShowOnDisabledMember))
                return (bool)element.AttachedProperties[ShowOnDisabledMember];
            else
                return false;
        }
        public static void SetShowOnDisabled(UIElement element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.AttachedProperties[ShowOnDisabledMember] = value;
        }
        #endregion

        #region Placement
        public static PlacementMode GetPlacement(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (element.AttachedProperties.ContainsKey(PlacementMember))
                return (PlacementMode)element.AttachedProperties[PlacementMember];
            else
                return PlacementMode.Mouse;
        }
        public static void SetPlacement(UIElement element, PlacementMode value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.AttachedProperties[PlacementMember] = value;
        }
        #endregion

        #region ShowDuration
        public static void SetShowDuration(UIElement element, float value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.AttachedProperties[ShowDurationMember] = value;
        }
        public static float GetShowDuration(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (element.AttachedProperties.ContainsKey(ShowDurationMember))
                return (float)element.AttachedProperties[ShowDurationMember];
            else
                return float.PositiveInfinity; // TODO: Or NaN?
        }
        #endregion

        #region HorizontalOffset
        public static void SetHorizontalOffset(UIElement element, float value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.AttachedProperties[HorizontalOffsetMember] = value;
        }
        public static float GetHorizontalOffset(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (element.AttachedProperties.ContainsKey(HorizontalOffsetMember))
                return (float)element.AttachedProperties[HorizontalOffsetMember];
            else
                return 0;
        }
        #endregion

        #region VerticalOffset
        public static void SetVerticalOffset(UIElement element, float value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.AttachedProperties[VerticalOffsetMember] = value;
        }
        public static float GetVerticalOffset(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (element.AttachedProperties.ContainsKey(VerticalOffsetMember))
                return (float)element.AttachedProperties[VerticalOffsetMember];
            else
                return 0;
        }
        #endregion

        #region ToolTip
        public static void SetToolTip(UIElement element, UIElement value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.AttachedProperties[ToolTipMember] = value;
        }
        public static UIElement GetToolTip(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (element.AttachedProperties.ContainsKey(ToolTipMember))
                return (UIElement)element.AttachedProperties[ToolTipMember];
            else
                return null;
        }
        #endregion
    }
}
