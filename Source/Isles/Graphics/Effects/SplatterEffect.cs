#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Isles.Graphics.Vertices;
#endregion

namespace Isles.Graphics.Effects
{
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


    public partial class SplatterEffect
    {
        public const int MaxLayers = 4;

        public SplatterTextureCollection Textures { get; private set; }


        public bool FogEnabled
        {
            get { return fogMask > 0.5f; }
            set { fogMask = (value ? 1.0f : 0.0f); }
        }
                
        public SplatterEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics))
        {
            InitializeComponent();

            Textures = new SplatterTextureCollection(this);
        }

        protected override void  OnApply()
        {
            textureX = Textures[0];
            textureY = Textures[1];
            textureZ = Textures[2];
            textureW = Textures[3];

            Vector4 m;

            m.X = (Textures[0] != null ? 1.0f : 0.0f);
            m.Y = (Textures[1] != null ? 1.0f : 0.0f);
            m.Z = (Textures[2] != null ? 1.0f : 0.0f);
            m.W = (Textures[3] != null ? 1.0f : 0.0f);

            mask = m;

            eyePosition = Matrix.Invert(View).Translation;

 	        base.OnApply();
        }
    }
}
