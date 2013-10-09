namespace Nine.Graphics.UI.Navigation
{
    using System;
    using System.Collections;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// 
    /// </summary>
    [System.Windows.Markup.ContentProperty("Content")]
    public class Page : UIElement, IContainer
    {
        /// <summary>
        /// Gets and sets the Content.
        /// </summary>
        public UIElement Content
        {
            get { return content; }
            set
            {
                content = value;
                content.Parent = this;
            }
        }
        private UIElement content;

        IList IContainer.Children { get { return new UIElement[] { Content }; } }

        protected override Microsoft.Xna.Framework.Vector2 MeasureOverride(Vector2 availableSize)
        {
            if (Visible == Visibility.Collapsed) return Vector2.Zero;
            if (content != null)
                content.Measure(availableSize);
            return base.MeasureOverride(availableSize);
        }

        protected override Microsoft.Xna.Framework.Vector2 ArrangeOverride(Vector2 finalSize)
        {
            if (Visible == Visibility.Collapsed) return Vector2.Zero;
            if (content != null)
                content.Arrange(new BoundingRectangle(0, 0, finalSize.X, finalSize.Y));
            return base.ArrangeOverride(finalSize);
        }

        protected internal override void OnRender(Renderer.Renderer renderer)
        {
            if (Visible != Visibility.Visible) return;
            base.OnRender(renderer);
            if (Content != null)
                Content.OnRender(renderer);
        }
    }
}
