namespace Nine.Graphics
{
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Xna.Framework.Graphics;

    static class PipelineGraphics
    {
        static GraphicsDevice graphics;
        static public GraphicsDevice GraphicsDevice
        {
            get { return graphics ?? (graphics = CreateGraphicsDevice()); }
        }

        private static GraphicsDevice CreateGraphicsDevice()
        {
            Form dummy = new Form();

            PresentationParameters parameters = new PresentationParameters();
            parameters.BackBufferWidth = 1;
            parameters.BackBufferHeight = 1;
            parameters.DeviceWindowHandle = dummy.Handle;

            GraphicsAdapter.UseNullDevice = true;
            GraphicsAdapter adapter = TryFindHiDefAdapter();

            return new GraphicsDevice(adapter
                                    , adapter.IsProfileSupported(GraphicsProfile.HiDef) ? GraphicsProfile.HiDef : GraphicsProfile.Reach
                                    , parameters);
        }

        private static GraphicsAdapter TryFindHiDefAdapter()
        {
            return GraphicsAdapter.Adapters.FirstOrDefault(adapter => adapter.IsProfileSupported(GraphicsProfile.HiDef)) ?? GraphicsAdapter.DefaultAdapter;
        }
    }
}
