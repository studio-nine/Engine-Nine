namespace Nine.Graphics.UI.TEST
{
    /// <summary>
    /// Userinterface's DrawingContext, this gives more power to the UI by allowing multiple Passes.
    /// </summary>
    public class DrawingContext
    {
        // TODO: Make Post Elements part of this instead.
        public Renderer.Renderer Renderer { get; set; }

        internal void AddDependancy(Nine.Graphics.Drawing.Pass pass)
        {

        }
    }
}
