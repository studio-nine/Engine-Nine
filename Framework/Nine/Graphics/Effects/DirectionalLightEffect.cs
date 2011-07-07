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

    public partial class DirectionalLightEffect :  IEffectLights<IDirectionalLight>, IEffectMatrices, IEffectTexture, IEffectSkinned
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
            set { diffuseTexture = value; diffuseTextureEnabled =(value != null); }
        }

        int MaxLights
        {
            get { return GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach ? 1 : 8; }
        }

        public ReadOnlyCollection<IDirectionalLight> Lights { get; private set; }

        public DirectionalLightEffect(GraphicsDevice graphics)
            : base(graphics, graphics.GraphicsProfile == GraphicsProfile.Reach ? EffectCode : DirectionalLightEffectHiDef.EffectCode)
        {
            CacheEffectParameters(Parameters);

            Lights = new ReadOnlyCollection<IDirectionalLight>(lights);
        }

        private void OnCreated()
        {

        }

        private void OnClone(DirectionalLightEffect cloneSource)
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
        }

        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.Diffuse)
                Texture = texture as Texture2D;
        }

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            throw new NotImplementedException();
        }
    }

#endif
}
