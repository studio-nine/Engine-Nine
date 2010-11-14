#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    public partial class ThresholdEffect
    {
        public ThresholdEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics))
        {
            InitializeComponent();
        }
    }

#endif
}
