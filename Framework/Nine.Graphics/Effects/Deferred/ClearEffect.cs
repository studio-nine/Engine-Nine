#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Effects.Deferred
{
#if !WINDOWS_PHONE

    partial class ClearEffect
    {
		private void OnCreated() { }
		private void OnClone(ClearEffect cloneSource) { }
		private void OnApplyChanges() { }
    }	

#endif
}
