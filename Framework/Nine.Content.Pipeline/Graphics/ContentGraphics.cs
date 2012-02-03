#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Content.Pipeline.Graphics
{
    static class ContentGraphics
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
            return new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);
        }
    }
}
