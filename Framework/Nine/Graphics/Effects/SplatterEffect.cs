#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Effects
{
#if !WINDOWS_PHONE

    #region SplatterTextureCollection
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class SplatterTextureCollection : IEnumerable<Texture2D>
    {
        SplatterEffect effect;

        internal SplatterTextureCollection(SplatterEffect effect)
        {
            this.effect = effect;
        }

        public Texture2D this[int index]
        {
            get 
            {
                if (index == 0)
                    return effect.textureX;
                if (index == 1)
                    return effect.textureY;
                if (index == 2)
                    return effect.textureZ;
                if (index == 3)
                    return effect.textureW;

                throw new IndexOutOfRangeException();
            }
            set
            {
                if (index == 0)
                    effect.textureX = value;
                else if (index == 1)
                    effect.textureY = value;
                else if (index == 2)
                    effect.textureZ = value;
                else if (index == 3)
                    effect.textureW = value;
                else
                    throw new IndexOutOfRangeException();
            }
        }

        public IEnumerator<Texture2D> GetEnumerator()
        {
            yield return effect.textureX;
            yield return effect.textureY;
            yield return effect.textureZ;
            yield return effect.textureW;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    #endregion

    #region SplatterEffect
    public partial class SplatterEffect : IEffectMatrices, IEffectFog, IEffectLights
    {
        public const int MaxLayers = 4;

        public SplatterTextureCollection Textures { get; private set; }
        
        public bool FogEnabled
        {
            get { return fogMask > 0.5f; }
            set { fogMask = (value ? 1.0f : 0.0f); }
        }

        public DirectionalLight DirectionalLight0 { get; private set; }
        public DirectionalLight DirectionalLight1 { get; private set; }
        public DirectionalLight DirectionalLight2 { get; private set; }

        public bool LightingEnabled { get; set; }

        public void EnableDefaultLighting()
        {
            LightingEnabled = true;

            DirectionalLight0.Direction = Vector3.Normalize(-Vector3.One);
            DirectionalLight0.DiffuseColor = Color.Yellow.ToVector3();
            DirectionalLight0.SpecularColor = Color.White.ToVector3();
        }
        
		private void OnCreated() 
        {
            DirectionalLight0 = new DirectionalLight(_lightDirectionParameter, _lightDiffuseColorParameter, _lightSpecularColorParameter, null);
            DirectionalLight1 = new DirectionalLight(_lightDirectionParameter, _lightDiffuseColorParameter, _lightSpecularColorParameter, null);
            DirectionalLight2 = new DirectionalLight(_lightDirectionParameter, _lightDiffuseColorParameter, _lightSpecularColorParameter, null);

            Textures = new SplatterTextureCollection(this);
        }

        private void OnClone(SplatterEffect cloneSource) 
        {
            FogEnabled = cloneSource.FogEnabled;
            LightingEnabled = cloneSource.LightingEnabled;
        }
        
		private void OnApplyChanges()
        {
            textureX = Textures[0];
            textureY = Textures[1];
            textureZ = Textures[2];
            textureW = Textures[3];

            Vector4 m = new Vector4();

            m.X = (Textures[0] != null ? 1.0f : 0.0f);
            m.Y = (Textures[1] != null ? 1.0f : 0.0f);
            m.Z = (Textures[2] != null ? 1.0f : 0.0f);
            m.W = (Textures[3] != null ? 1.0f : 0.0f);

            mask = m;

            eyePosition = Matrix.Invert(View).Translation;
        }
    }
    #endregion

#endif
}
