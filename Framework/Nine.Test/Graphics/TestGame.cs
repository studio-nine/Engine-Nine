#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Forms;

namespace Nine.Graphics.Test
{
    public class TestGame : Game
    {
        public event Action<GameTime> Paint;

        public TestGame()
        {
            GraphicsDeviceManager manager = new GraphicsDeviceManager(this);
            manager.PreferredBackBufferWidth = 800;
            manager.PreferredBackBufferHeight = 500;

            Form form = (Form)Form.FromHandle(Window.Handle);
            form.TopMost = false;
            form.TopLevel = false;
            form.SendToBack();
            form.Shown += (o, e) => { form.Hide(); };
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(95, 120, 157));

            if (Paint != null)
                Paint(gameTime);

 	         base.Draw(gameTime);
        }
    }
}
