#region Using Statements
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.ObjectModel;
#endregion

namespace Nine.Graphics.Effects
{
#if !WINDOWS_PHONE

    public partial class AmbientLightEffect : IEffectLights<IAmbientLight>, IEffectMatrices, IEffectTexture, IEffectSkinned
    {
        private Matrix view;
        private Matrix projection;

        public bool SkinningEnabled
        {
            get { return shaderIndex == 1; }
            set { shaderIndex = value ? 1 : 0; }
        }

        public Matrix View
        {
            get { return view; }
            set { view = value; dirtyFlag |= worldViewProjectionDirtyFlag; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; dirtyFlag |= worldViewProjectionDirtyFlag; }
        }

        public Texture2D Texture
        {
            get { return diffuseTexture; }
            set { diffuseTexture = value; diffuseTextureEnabled = (value != null); }
        }

        public ReadOnlyCollection<IAmbientLight> Lights { get; private set; }

        private void OnCreated()
        {
            Lights = new ReadOnlyCollection<IAmbientLight>(new AmbientLight[]
            {
                new AmbientLight(),
                new AmbientLight(),
                new AmbientLight(),
                new AmbientLight(),
            });
        }

        private void OnClone(AmbientLightEffect cloneSource)
        {
            view = cloneSource.view;
            projection = cloneSource.projection;
        }

        private void OnApplyChanges()
        {
            if ((dirtyFlag & worldViewProjectionDirtyFlag) != 0 ||
                (dirtyFlag & WorldDirtyFlag) != 0)
            {
                Matrix wvp;
                Matrix.Multiply(ref _World, ref view, out wvp);
                Matrix.Multiply(ref wvp, ref projection, out wvp);
                worldViewProjection = wvp;

                Matrix viewInverse;
                Matrix.Invert(ref view, out viewInverse);
                eyePosition = viewInverse.Translation;
            }

            Vector3 color = new Vector3();
            foreach (var light in Lights)
            {
                color += light.AmbientLightColor;
            }
            ambientLightColor = color;
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Diffuse)
                Texture = texture as Texture2D;
        }

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            bones = boneTransforms;
        }

        class AmbientLight : IAmbientLight
        {
            public Vector3 AmbientLightColor { get; set; }
            public AmbientLight()
            {
                AmbientLightColor = Vector3.One * 0.2f;
            }
        }
    }

#endif
}
