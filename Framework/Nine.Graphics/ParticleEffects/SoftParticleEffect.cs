#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ParticleEffects
{
#if !WINDOWS_PHONE

    partial class SoftParticleEffect : IEffectMatrices, IEffectTexture
    {
        private Matrix world;
        private Matrix view;

        private void OnCreated()
        {
            DepthFade = 1;
        }
        private void OnClone(SoftParticleEffect cloneSource) { }

        public Matrix World
        {
            get { return world; }
            set { world = value; dirtyFlag |= worldViewDirtyFlag; }
        }

        public Matrix View
        {
            get { return view; }
            set { view = value; dirtyFlag |= worldViewDirtyFlag; }
        }

        public void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.DepthBuffer)
                DepthBuffer = texture as Texture2D;
        }

        private void OnApplyChanges()
        {
            if ((dirtyFlag & worldViewDirtyFlag) != 0)
            {
                Matrix wv;
                Matrix.Multiply(ref world, ref view, out wv);
                worldView = wv;
            }

            if ((dirtyFlag & ProjectionDirtyFlag) != 0)
            {
                projectionInverse = Matrix.Invert(Projection);
            }
        }
    }

#endif
}
