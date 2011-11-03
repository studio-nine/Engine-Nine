#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
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
    [TestClass]
    public class GraphicsTest
    {
        public TestGame Game;
        public GraphicsDevice GraphicsDevice { get { return Game.GraphicsDevice; } }

        [TestInitialize()]
        public void InitializeGraphicsTest()
        {
            Game = new TestGame();
        }

        [TestCleanup()]
        public void CleanupGraphicsTest()
        {
            Game.Exit();
        }

        public void Test(Action test)
        {
            Game.Paint += (gameTime) =>
            {
                test();
                Game.Exit();
            };
            Game.Run();
        }
    }
}
