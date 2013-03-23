namespace Nine.Graphics.UI.Controls
{
    using Nine.Graphics.UI.Controls.Primitives;

    public class ToggleButton : ButtonBase
    {
        public bool IsChecked { get; set; }

        public ToggleButton()
        {
            IsChecked = false;
        }

        // TODO: Behavior

        protected internal override void OnRender(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.OnRender(spriteBatch);

            // TODO: Rendering
            switch (IsChecked)
            {
                case true:
                    break;
                case false:
                    break;
            }
        }
    }
}
