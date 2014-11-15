namespace Nine.Graphics.UI.Controls
{
    using Microsoft.Xna.Framework.Graphics;

    // This is going to be generic later on, when I have the abilily to do so with Textbox
    public class NumberBox : TextBox
    {
        public int Value
        {
            get { return int.Parse(Text); }
        }

        public NumberBox()
        {

        }

        public NumberBox(SpriteFont font)
            : base(font)
        {

        }

        protected override bool AllowInput(Nine.TextChange change)
        {
            if (change.Text.Length == 1)
            {
                char result;
                if (char.TryParse(change.Text, out result))
                {
                    if (char.IsNumber(result))
                        return true;
                }
            }
            if (change.Type == TextChange.TextChangeType.CharRemovedNegative ||
                change.Type == TextChange.TextChangeType.CharRemovedPositive)
                return true;
            return false;
        }
    }
}
