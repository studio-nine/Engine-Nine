#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ParticleEffects
{
#if !WINDOWS_PHONE

    public partial class LineSpriteEffect
    {
        private void OnCreated() { }
        private void OnClone(LineSpriteEffect cloneSource) { }
        private void OnApplyChanges() { }
    }	

#endif
}
