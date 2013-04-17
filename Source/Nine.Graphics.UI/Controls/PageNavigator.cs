namespace Nine.Graphics.UI.Controls
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;

    [System.Windows.Markup.ContentProperty("Pages")]
    public class PageNavigator : UIElement, IContainer, INotifyCollectionChanged<UIElement>
    {
        public IList<Page> Pages { get { return pages; } }
        private NotificationCollection<Page> pages;

        public int SelectedIndex { get; set; }

        IList IContainer.Children { get { return (IList)Pages; } }

        public event Action<UIElement> Added;
        public event Action<UIElement> Removed;

        public PageNavigator()
        {
            pages = new NotificationCollection<Page>();
            pages.Sender = this;
            pages.Added += Child_Added;
            pages.Removed += Child_Removed;
        }

        void Child_Added(object value)
        {
            var element = value as UIElement;
            if (element != null)
            {
                element.Parent = this;
                if (Added != null)
                    Added.Invoke(element);
            }
        }
        void Child_Removed(object value)
        {
            if (Removed != null)
                Removed.Invoke(value as UIElement);
        }

        public bool NextPageInList()
        {
            if (SelectedIndex < pages.Count)
            {
                SelectedIndex++;
                return true;
            }
            return false;
        }
        public bool PrevPageInList()
        {
            if (SelectedIndex < pages.Count && SelectedIndex > 0)
            {
                SelectedIndex--;
                return true;
            }
            return false;
        }

        public bool NavigateTo(string name)
        {
            var spage = pages.Where(o => o.Name == name).First();
            if (spage != null)
            {
                SelectedIndex = pages.IndexOf(spage);
                return true;
            }
            return false;
        }

        protected override Microsoft.Xna.Framework.Vector2 MeasureOverride(Microsoft.Xna.Framework.Vector2 availableSize)
        {
            pages[SelectedIndex].Measure(availableSize);
            return base.MeasureOverride(availableSize);
        }

        protected override Microsoft.Xna.Framework.Vector2 ArrangeOverride(Microsoft.Xna.Framework.Vector2 finalSize)
        {
            pages[SelectedIndex].Arrange(new BoundingRectangle(0, 0, finalSize.X, finalSize.Y));
            return base.ArrangeOverride(finalSize);
        }

        protected internal override void OnRender(Renderer.IRenderer renderer)
        {
            base.OnRender(renderer);
            pages[SelectedIndex].OnRender(renderer);
        }
    }
}
