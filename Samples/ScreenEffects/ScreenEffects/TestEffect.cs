#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace ScreenEffects
{
#if !WINDOWS_PHONE

    public partial class TestEffect
    {
        public TestEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics))
        {
            InitializeComponent();
        }
    }

#endif
}
