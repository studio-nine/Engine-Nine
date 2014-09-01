namespace Nine.Graphics.UI
{
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Controls;
    using Nine.Graphics.UI.Controls.Primitives;
    using System;
    
    public abstract partial class UIElement
    {
        public object ToolTip
        {
            get
            {
                var tooltipElement = ToolTipService.GetToolTip(this);

                // Should I handle it as string if it is a TextBlock?
                var textBlock = tooltipElement as Nine.Graphics.UI.Controls.TextBlock;
                if (textBlock != null)
                    return textBlock.Text;

                return tooltipElement;
            }
            set
            {
                var tooltipElement = ToolTipService.GetToolTip(this);
                if (tooltipElement == null)
                    ToolTipService.SetToolTip(this, ObjectToElement(value));

                if (value is string)
                {
                    var textBlock = tooltipElement as Nine.Graphics.UI.Controls.TextBlock;
                    if (textBlock != null)
                    {
                        textBlock.Text = value as string;
                    }
                    else
                        throw new ArgumentException();
                }
                else
                {
                    var element = value as UIElement;
                    if (element != null)
                    {
                        ToolTipService.SetToolTip(this, element);
                    }
                    else
                        throw new ArgumentException();
                }
            }
        }

        // TODO: Move this
        private bool showToolTip = false;

        partial void ShowToolTip()
        {
            this.showToolTip = true;
        }

        partial void HideToolTip()
        {
            this.showToolTip = false;
        }

        partial void DrawToolTip(Renderer.Renderer renderer)
        {
            var tooltipElement = ToolTipService.GetToolTip(this);
            if (tooltipElement == null)
                return;

            if (showToolTip)
            {
                var position = GetToolTipPosition(tooltipElement);

                tooltipElement.Measure(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
                tooltipElement.Arrange(new BoundingRectangle(position.X, position.Y, tooltipElement.DesiredSize.X, tooltipElement.DesiredSize.Y));

                renderer.AddPostElemenet(tooltipElement);
            }
        }

        private Vector2 GetToolTipPosition(UIElement tooltip)
        {
            var absRT = AbsoluteRenderTransform;

            var result = Vector2.Zero;

            result.X += ToolTipService.GetHorizontalOffset(this);
            result.Y += ToolTipService.GetVerticalOffset(this);

            // TODO: ToolTip PlacementMode (2)
            switch (ToolTipService.GetPlacement(this))
            {
                case PlacementMode.Absolute: break;
                case PlacementMode.Relative: throw new NotImplementedException();
                case PlacementMode.Bottom:      result += new Vector2(absRT.Left, absRT.Bottom); break;
                case PlacementMode.Center:      result += new Vector2(absRT.Left + (this.ActualWidth / 2), absRT.Top + (this.ActualHeight / 2)); break;
                case PlacementMode.Right:       result += new Vector2(absRT.Right, absRT.Top); break;
                case PlacementMode.Left:        result += new Vector2(absRT.Left - tooltip.ActualWidth, absRT.Top); break;
                case PlacementMode.Top:         result += new Vector2(absRT.Left, absRT.Top - tooltip.ActualHeight); break;
                default:
                case PlacementMode.Mouse:
                    {
                        // Is there a better way to get the mouse position?
                        var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
                        result += new Vector2(mouseState.X, mouseState.Y);

                    } break;
                case PlacementMode.Custom:
                    {
                        throw new NotImplementedException();
                    } break;
            }

            return result;
        }

        // Move this, is this used for something else?
        // TODO: String To TextBlock needs Default Font
        internal UIElement ObjectToElement(object element)
        {
            if (element is UIElement)
                return element as UIElement;

            var isString = element as string;
            if (isString != null)
            {
                return new Nine.Graphics.UI.Controls.TextBlock
                {
                    Text = isString
                };
            }

            return null;
        }
    }
}
