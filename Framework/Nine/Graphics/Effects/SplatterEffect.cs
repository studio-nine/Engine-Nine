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
        Texture2D[] textures;
        SplatterEffect effect;

        internal SplatterTextureCollection(SplatterEffect effect)
        {
            this.effect = effect;
            this.textures = new Texture2D[SplatterEffect.MaxLayers];
        }

        public Texture2D this[int index]
        {
            get { return textures[index]; }
            set { textures[index] = value; }
        }

        public IEnumerator<Texture2D> GetEnumerator()
        {
            return (textures as IEnumerable<Texture2D>).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return textures.GetEnumerator();
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

        public SplatterEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics))
        {
            InitializeComponent();

            DirectionalLight0 = new DirectionalLight(_lightDirection, _lightDiffuseColor, _lightSpecularColor, null);
            DirectionalLight1 = new DirectionalLight(_lightDirection, _lightDiffuseColor, _lightSpecularColor, null);
            DirectionalLight2 = new DirectionalLight(_lightDirection, _lightDiffuseColor, _lightSpecularColor, null);

            Textures = new SplatterTextureCollection(this);
        }

        protected override void  OnApply()
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

 	        base.OnApply();
        }
    }
    #endregion

#endif
}
