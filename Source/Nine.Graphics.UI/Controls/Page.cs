namespace Nine.Graphics.UI.Controls
{
    using System;
    using System.Collections;

    [System.Windows.Markup.ContentProperty("Content")]
    public class Page : UIElement, IContainer
    {
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

        protected override Microsoft.Xna.Framework.Vector2 MeasureOverride(Microsoft.Xna.Framework.Vector2 availableSize)
        {
            if (content != null)
                content.Measure(availableSize);
            return base.MeasureOverride(availableSize);
        }

        protected override Microsoft.Xna.Framework.Vector2 ArrangeOverride(Microsoft.Xna.Framework.Vector2 finalSize)
        {
            if (content != null)
                content.Arrange(new BoundingRectangle(0, 0, finalSize.X, finalSize.Y));
            return base.ArrangeOverride(finalSize);
        }

        protected internal override void OnRender(Renderer.Renderer renderer)
        {
            base.OnRender(renderer);
            if (Content != null)
                Content.OnRender(renderer);
        }
    }
}
