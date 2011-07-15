#region Using Statements
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Effects
{
#if !WINDOWS_PHONE

    public partial class PointLightEffect : IEffectLights<IPointLight>, IEffectMatrices, IEffectTexture, IEffectSkinned
    {
        private Matrix view;
        private Matrix projection;
        private bool viewProjectionChanged;

        public bool SkinningEnabled
        {
            get { return ShaderIndex == 0; }
            set { ShaderIndex = value ? 1 : 0; }
        }

        public Matrix View
        {
            get { return view; }
            set { view = value; viewProjectionChanged = true; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; viewProjectionChanged = true; }
        }

        public Texture2D Texture
        {
            get { return diffuseTexture; }
            set { diffuseTexture = value; diffuseTextureEnabled = (value != null); }
        }

        int MaxLights
        {
            get { return GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach ? 1 : 4; }
        }

        public ReadOnlyCollection<IPointLight> Lights { get; private set; }

        private void OnCreated()
        {
            numLights = MaxLights;
            SpecularPower = 16;
            Lights = new ReadOnlyCollection<IPointLight>(lights);
        }

        private void OnClone(PointLightEffect cloneSource)
        {
            view = cloneSource.view;
            projection = cloneSource.projection;
            viewProjectionChanged = true;
        }

        private void OnApplyChanges()
        {
            if (viewProjectionChanged)
            {
                eyePosition = Matrix.Invert(view).Translation;
                viewProjection = view * projection;
                viewProjectionChanged = false;
            }

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].DiffuseColor == Vector3.Zero &&
                    lights[i].SpecularColor == Vector3.Zero)
                {
                    numLights = i;
                    break;
                }
            }
        }

        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.Diffuse)
                Texture = texture as Texture2D;
        }

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            bones = boneTransforms;
        }

        partial class Class_lights : IPointLight { }
    }

#endif
}

