#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Effects.Deferred
{
#if !WINDOWS_PHONE

    public partial class DeferredPointLight : IDisplayObject
    {
        /*
        public Matrix World
        {
            get { return world; }
            set
            {
                world = value;
                worldView = World * View;
            }
        }
        private Matrix world;

        public Matrix View
        {
            get { return view; }
            set
            {
                view = value;
                worldView = World * View;
            }
        }
        private Matrix view;

        public Matrix Projection
        {
            get { return projection; }
            set
            {
                projection = value;
                projectionInverse = Matrix.Invert(projection);
            }
        } 
        */

		private void OnCreated() { }
		private void OnClone(DeferredPointLight cloneSource) { }
		private void OnApplyChanges() { }

        public void Draw(GameTime gameTime)
        {

        }
    }

    class Sphere
    {

    }

#endif
}
