#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
#endregion

namespace Nine.Content.Pipeline.Graphics
{
    class MockedGraphicsDevice
    {
        static GraphicsDevice device;

        public static GraphicsDevice GraphicsDevice 
        {
            get
            {
                if (device == null)
                    CreateGraphicsDevice(); 
                return device;
            }
        }

        static void CreateGraphicsDevice()
        {
            // Create graphics device
            Form dummy = new Form();

            PresentationParameters parameters = new PresentationParameters();
            parameters.BackBufferWidth = 800;
            parameters.BackBufferHeight = 600;
            parameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            parameters.DeviceWindowHandle = dummy.Handle;

            GraphicsAdapter.UseNullDevice = true;
            device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.Reach, parameters);
        }
    }
}
