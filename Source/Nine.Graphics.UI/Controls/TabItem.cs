namespace Nine.Graphics.UI.Controls
{
    public class TabItem : ContentControl
    {
        public bool IsSelected { get; set; }

        public bool HasHeader { get { return Header != string.Empty; } }
        public string Header { get; set; }
    }
}
