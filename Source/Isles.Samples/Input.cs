#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Samples
{
    [SampleClass()]
    public class InputSample
    {
        [SampleMethod()]
        public static void Test()
        {
            using (Microsoft.Xna.Framework.Game game = new SampleGame())
            {
                Input input = new Input(game);

                input.MouseUp += new EventHandler<MouseEventArgs>(input_LeftButtonUp);
                input.DoubleClick += new EventHandler<MouseEventArgs>(input_DoubleClick);
                input.KeyUp += new EventHandler<KeyboardEventArgs>(input_KeyDown);

                game.Components.Add(input);
                game.Run();
            }
        }

        static void input_LeftButtonUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButton.Left)
                MessageBox.Show("Left Up");
        }

        static void input_DoubleClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("Double Click");
        }

        static void input_KeyDown(object sender, KeyboardEventArgs e)
        {
            MessageBox.Show(e.Key.ToString());
        }
    }
}
