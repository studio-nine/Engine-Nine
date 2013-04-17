namespace Nine.Graphics.UI.Controls
{
    using System;

    // TODO: Rework select system

    /// <summary>
    /// A <see cref="ListBoxItem">ListBoxItem</see> that only can be used in a <see cref="ListBox">ListBox</see>.
    /// </summary>
    public class ListBoxItem : ContentControl
    {
        public bool IsSelected 
        {
            get 
            {
                if (ListBox.SelectedItem == this)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    ListBox.SelectedItem = this;
                else
                    ListBox.SelectedIndex = -1;

                if (IsSelectedChanged != null)
                    IsSelectedChanged(value);
            }
        }

        protected ListBox ListBox
        {
            get
            {
                // Unstable
                var listBox = Parent.Parent as ListBox;
                if (listBox != null)
                    return listBox;
                else
                    throw new NotSupportedException("Parent");
            }
        }

        // This is only called when selected!
        public event Action<bool> IsSelectedChanged;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            switch (ListBox.SelectionMode)
            {
                case SelectionMode.None:
                case SelectionMode.Single:
                case SelectionMode.Multiple:
                    break;

                case SelectionMode.GameMode:
                    {
                        IsSelected = true;
                        break;
                    }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            switch (ListBox.SelectionMode)
            {
                case SelectionMode.None:
                    break;

                case SelectionMode.Single:
                    IsSelected = true;
                    break;

                case SelectionMode.Multiple:
                    throw new NotImplementedException();

                case SelectionMode.GameMode: 
                    break;
            }
        }
    }
}
