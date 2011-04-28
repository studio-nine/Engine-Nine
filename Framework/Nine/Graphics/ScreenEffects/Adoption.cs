#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    internal partial class Adoption : IUpdateObject
    {
        public bool HighQuality
        {
            get { return Parameters["ShaderIndex"].GetValueInt32() == 0; }
            set { Parameters["ShaderIndex"].SetValue(value ? 1 : 0); }
        }

		private void OnClone(Adoption cloneSource) { }

		private void OnCreated() 
        {
            Speed = 1;
            HighQuality = true;
        }

		private void OnApplyChanges() { }
        
        public void Update(GameTime gameTime)
        {
            deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

#endif
}
