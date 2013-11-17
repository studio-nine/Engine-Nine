namespace Nine.Graphics.UI.Controls
{
    using System;
    using System.Xaml;
    using Nine.Graphics.UI.Controls.Primitives;

    public static class ToolTipService
    {
        static readonly AttachableMemberIdentifier IsEnabledMember = new AttachableMemberIdentifier(typeof(bool), "IsEnabled");
        static readonly AttachableMemberIdentifier ShowOnDisabledMember = new AttachableMemberIdentifier(typeof(bool), "ShowOnDisabled");
        static readonly AttachableMemberIdentifier PlacementMember = new AttachableMemberIdentifier(typeof(PlacementMode), "Placement");
        static readonly AttachableMemberIdentifier ShowDurationMember = new AttachableMemberIdentifier(typeof(float), "ShowDuration");
        static readonly AttachableMemberIdentifier ToolTipMember = new AttachableMemberIdentifier(typeof(UIElement), "ToolTip");

        //// Stuff that WPF has but don't want to implement yet or if I should.
        // public static void SetBetweenShowDelay(UIElement element, int value);
        // public static void SetInitialShowDelay(UIElement element, int value);
        // public static void SetHorizontalOffset(UIElement element, double value);
        // public static void SetVerticalOffset(UIElement element, double value);

        #region get

        public static bool GetIsEnabled(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (element.AttachedProperties.ContainsKey(IsEnabledMember))
                return (bool)element.AttachedProperties[IsEnabledMember];
            else
                return true;
        }

        public static bool GetShowOnDisabled(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (element.AttachedProperties.ContainsKey(ShowOnDisabledMember))
                return (bool)element.AttachedProperties[ShowOnDisabledMember];
            else
                return false;
        }

        public static PlacementMode GetPlacement(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (element.AttachedProperties.ContainsKey(PlacementMember))
                return (PlacementMode)element.AttachedProperties[PlacementMember];
            else
                return PlacementMode.Mouse;
        }

        public static float GetShowDuration(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (element.AttachedProperties.ContainsKey(ShowDurationMember))
                return (float)element.AttachedProperties[ShowDurationMember];
            else
                return float.PositiveInfinity; // Or NaN?
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

        #region set

        public static void SetIsEnabled(UIElement element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.AttachedProperties[IsEnabledMember] = value;
        }

        public static void SetShowOnDisabled(UIElement element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.AttachedProperties[ShowOnDisabledMember] = value;
        }

        public static void SetPlacement(UIElement element, PlacementMode value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.AttachedProperties[PlacementMember] = value;
        }

        public static void SetShowDuration(UIElement element, float value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.AttachedProperties[ShowDurationMember] = value;
        }

        public static void SetToolTip(UIElement element, UIElement value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.AttachedProperties[ToolTipMember] = value;
        }

        #endregion
    }
}
